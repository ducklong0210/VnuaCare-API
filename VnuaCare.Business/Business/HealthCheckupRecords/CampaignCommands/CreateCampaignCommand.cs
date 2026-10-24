/**
 * Nghiệp vụ Thêm mới Đợt khám sức khỏe định kỳ (CreateCampaignCommand)
 */

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.Context;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.HealthCheckupRecords.CampaignCommands;

public class CreateCampaignCommand : IRequest<Unit>
{
    public CreateCampaignModel Model { get; }

    public CreateCampaignCommand(CreateCampaignModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<CreateCampaignCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;
        private readonly ICacheService _cacheService;
        private readonly IContextAccessor _contextAccessor;

        public Handler(
            VnuaCareDataContext dataContext,
            ICacheService cacheService,
            Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        public async Task<Unit> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu thêm đợt khám mới: " + JsonSerializer.Serialize(model));

            #region 1. Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() && currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện tạo đợt khám (Yêu cầu quyền Quản trị viên).");
            }
            #endregion

            #region 2. Kiểm tra dữ liệu
            if (string.IsNullOrWhiteSpace(model.CampaignName))
            {
                throw new ArgumentException("Tên đợt khám không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(model.CampaignCode))
            {
                throw new ArgumentException("Mã đợt khám không được để trống.");
            }

            var isCodeExisted = await _dataContext.VcCheckupCampaigns
                .AnyAsync(x => x.CampaignCode == model.CampaignCode.Trim(), cancellationToken);
            if (isCodeExisted)
            {
                throw new ArgumentException($"Mã đợt khám '{model.CampaignCode}' đã tồn tại.");
            }
            #endregion

            #region 3. Tạo entity và lưu CSDL
            var campaign = new VcCheckupCampaigns
            {
                CampaignName = model.CampaignName.Trim(),
                CampaignCode = model.CampaignCode.Trim().ToUpper(),
                Year = model.Year > 0 ? model.Year : DateTime.Now.Year,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                HospitalPartner = string.IsNullOrWhiteSpace(model.HospitalPartner) ? "Bệnh viện Đa khoa MEDLATEC" : model.HospitalPartner.Trim(),
                Status = string.IsNullOrWhiteSpace(model.Status) ? "PLANNING" : model.Status.Trim().ToUpper()
            };

            await _dataContext.VcCheckupCampaigns.AddAsync(campaign, cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region 4. Xóa Cache
            _cacheService.Remove($"campaigns-{VnuaCareCacheConstant.LIST_SELECT}");
            #endregion

            Log.Information($"Tạo đợt khám '{campaign.CampaignName}' thành công với ID: {campaign.CampaignId}");
            return Unit.Value;
        }
    }
}
