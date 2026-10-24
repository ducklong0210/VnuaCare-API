/**
 * Nghiệp vụ Cập nhật Đợt khám sức khỏe định kỳ (UpdateCampaignCommand)
 */

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.HealthCheckupRecords.CampaignCommands;

public class UpdateCampaignCommand : IRequest<Unit>
{
    public UpdateCampaignModel Model { get; }

    public UpdateCampaignCommand(UpdateCampaignModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<UpdateCampaignCommand, Unit>
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

        public async Task<Unit> Handle(UpdateCampaignCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu cập nhật đợt khám: " + JsonSerializer.Serialize(model));

            #region 1. Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() && currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện cập nhật đợt khám (Yêu cầu quyền Quản trị viên).");
            }
            #endregion

            #region 2. Kiểm tra tồn tại
            var entity = await _dataContext.VcCheckupCampaigns
                .FirstOrDefaultAsync(x => x.CampaignId == model.CampaignId, cancellationToken);
            if (entity == null)
            {
                throw new ArgumentException($"Không tìm thấy đợt khám với ID: {model.CampaignId}");
            }
            #endregion

            #region 3. Kiểm tra dữ liệu hợp lệ
            if (string.IsNullOrWhiteSpace(model.CampaignName))
            {
                throw new ArgumentException("Tên đợt khám không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(model.CampaignCode))
            {
                throw new ArgumentException("Mã đợt khám không được để trống.");
            }

            var isCodeExisted = await _dataContext.VcCheckupCampaigns
                .AnyAsync(x => x.CampaignCode == model.CampaignCode.Trim() && x.CampaignId != model.CampaignId, cancellationToken);
            if (isCodeExisted)
            {
                throw new ArgumentException($"Mã đợt khám '{model.CampaignCode}' đã tồn tại ở đợt khám khác.");
            }
            #endregion

            #region 4. Cập nhật dữ liệu
            entity.CampaignName = model.CampaignName.Trim();
            entity.CampaignCode = model.CampaignCode.Trim().ToUpper();
            entity.Year = model.Year > 0 ? model.Year : entity.Year;
            entity.StartDate = model.StartDate;
            entity.EndDate = model.EndDate;
            if (!string.IsNullOrWhiteSpace(model.HospitalPartner))
            {
                entity.HospitalPartner = model.HospitalPartner.Trim();
            }
            if (!string.IsNullOrWhiteSpace(model.Status))
            {
                entity.Status = model.Status.Trim().ToUpper();
            }

            _dataContext.VcCheckupCampaigns.Update(entity);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region 5. Xóa Cache
            _cacheService.Remove($"campaigns-{VnuaCareCacheConstant.LIST_SELECT}");
            #endregion

            Log.Information($"Cập nhật đợt khám '{entity.CampaignName}' thành công.");
            return Unit.Value;
        }
    }
}
