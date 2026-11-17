/**
 * Nghiệp vụ Xóa Đợt khám sức khỏe định kỳ (DeleteCampaignCommand)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.Campaigns.CampaignCommands;

public class DeleteCampaignCommand : IRequest<Unit>
{
    public int CampaignId { get; }

    public DeleteCampaignCommand(int campaignId)
    {
        CampaignId = campaignId;
    }

    public class Handler : IRequestHandler<DeleteCampaignCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để xóa đợt khám
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi xóa dữ liệu
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu, ràng buộc hồ sơ và xóa đợt khám khỏi CSDL
        public async Task<Unit> Handle(DeleteCampaignCommand request, CancellationToken cancellationToken)
        {
            Log.Information($"Bắt đầu xóa đợt khám ID: {request.CampaignId}");

            #region Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() && currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện xóa đợt khám (Yêu cầu quyền Quản trị viên).");
            }
            #endregion

            #region Kiểm tra tồn tại
            var entity = await _dataContext.VcCheckupCampaigns
                .FirstOrDefaultAsync(x => x.CampaignId == request.CampaignId, cancellationToken);
            if (entity == null)
            {
                throw new ArgumentException($"Không tìm thấy đợt khám với ID: {request.CampaignId}");
            }
            #endregion

            #region Kiểm tra ràng buộc dữ liệu
            var hasRecords = await _dataContext.VcHealthCheckupRecords
                .AnyAsync(x => x.CampaignId == request.CampaignId, cancellationToken);
            if (hasRecords)
            {
                throw new ArgumentException("Đợt khám này đã có hồ sơ khám sức khỏe liên kết, không thể xóa trực tiếp. Vui lòng cập nhật trạng thái đợt khám sang 'COMPLETED' nếu đợt khám đã kết thúc.");
            }
            #endregion

            #region Xóa khỏi CSDL
            _dataContext.VcCheckupCampaigns.Remove(entity);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region Xóa Cache
            _cacheService.Remove($"campaigns-{VnuaCareCacheConstant.LIST_SELECT}");
            #endregion

            Log.Information($"Xóa đợt khám ID {request.CampaignId} thành công.");
            return Unit.Value;
        }
    }
}
