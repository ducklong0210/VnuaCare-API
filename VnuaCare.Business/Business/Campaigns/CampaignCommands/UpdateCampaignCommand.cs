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

namespace VnuaCare.Business.Business.Campaigns.CampaignCommands;

public class UpdateCampaignCommand : IRequest<Unit>
{
    public UpdateCampaignModel Model { get; }

    public UpdateCampaignCommand(UpdateCampaignModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<UpdateCampaignCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để cập nhật thông tin đợt khám
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi cập nhật
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu, quyền hạn và cập nhật đợt khám trong CSDL
        public async Task<Unit> Handle(UpdateCampaignCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu cập nhật đợt khám: " + JsonSerializer.Serialize(model));

            #region Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() && currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện cập nhật đợt khám (Yêu cầu quyền Quản trị viên).");
            }
            #endregion

            #region Kiểm tra tồn tại
            var entity = await _dataContext.VcCheckupCampaigns
                .FirstOrDefaultAsync(x => x.CampaignId == model.CampaignId, cancellationToken);
            if (entity == null)
            {
                throw new ArgumentException($"Không tìm thấy đợt khám với ID: {model.CampaignId}");
            }
            #endregion

            #region Kiểm tra tính hợp lệ dữ liệu
            if (string.IsNullOrWhiteSpace(model.CampaignName))
            {
                throw new ArgumentException("Tên đợt khám không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(model.CampaignCode))
            {
                throw new ArgumentException("Mã đợt khám không được để trống.");
            }

            // Kiểm tra trùng mã với đợt khám khác
            var isCodeExisted = await _dataContext.VcCheckupCampaigns
                .AnyAsync(x => x.CampaignCode == model.CampaignCode.Trim() && x.CampaignId != model.CampaignId, cancellationToken);
            if (isCodeExisted)
            {
                throw new ArgumentException($"Mã đợt khám '{model.CampaignCode}' đã tồn tại ở đợt khám khác.");
            }
            #endregion

            #region Cập nhật CSDL
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

            #region Xóa Cache
            _cacheService.Remove(CampaignConstant.BuildCacheKey(entity.CampaignId.ToString()));
            _cacheService.Remove(CampaignConstant.BuildCacheKey());
            #endregion

            Log.Information($"Cập nhật đợt khám '{entity.CampaignName}' thành công.");
            return Unit.Value;
        }
    }
}
