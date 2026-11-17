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

namespace VnuaCare.Business.Business.Campaigns.CampaignCommands;

public class CreateCampaignCommand : IRequest<Unit>
{
    public CreateCampaignModel Model { get; }

    public CreateCampaignCommand(CreateCampaignModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<CreateCampaignCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để ghi dữ liệu đợt khám
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi thêm mới
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu, quyền hạn và lưu đợt khám mới vào CSDL
        public async Task<Unit> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu thêm đợt khám mới: " + model.CampaignName);

            #region Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() && currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện tạo đợt khám (Yêu cầu quyền Quản trị viên).");
            }
            #endregion

            #region Kiểm tra dữ liệu
            if (string.IsNullOrWhiteSpace(model.CampaignName))
            {
                throw new ArgumentException("Tên đợt khám không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(model.CampaignCode))
            {
                throw new ArgumentException("Mã đợt khám không được để trống.");
            }

            // Kiểm tra trùng mã đợt khám
            var isCodeExisted = await _dataContext.VcCheckupCampaigns
                .AnyAsync(x => x.CampaignCode == model.CampaignCode.Trim(), cancellationToken);
            if (isCodeExisted)
            {
                throw new ArgumentException($"Mã đợt khám '{model.CampaignCode}' đã tồn tại.");
            }
            #endregion

            #region Lưu vào CSDL
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

            #region Xóa Cache
            _cacheService.Remove(CampaignConstant.BuildCacheKey());
            #endregion

            Log.Information($"Tạo đợt khám '{campaign.CampaignName}' thành công với ID: {campaign.CampaignId}");
            return Unit.Value;
        }
    }
}
