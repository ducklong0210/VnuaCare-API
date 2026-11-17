/**
 * Nghiệp vụ Xóa chỉ số sinh tồn và thể lực (DeleteVitalSignsCommand)
 * Chuẩn CRUD: Xóa bản ghi chỉ số sinh tồn
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.VitalSigns.VitalSignsCommands;

public class DeleteVitalSignsCommand : IRequest<Unit>
{
    public int VitalSignId { get; }

    public DeleteVitalSignsCommand(int vitalSignId)
    {
        VitalSignId = vitalSignId;
    }

    public class Handler : IRequestHandler<DeleteVitalSignsCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để xóa chỉ số sinh tồn
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi xóa dữ liệu
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu và xóa bản ghi chỉ số sinh tồn khỏi CSDL
        public async Task<Unit> Handle(DeleteVitalSignsCommand request, CancellationToken cancellationToken)
        {
            Log.Information($"Bắt đầu xóa chỉ số sinh tồn ID: {request.VitalSignId}");

            #region Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa chỉ số sinh tồn.");
            }
            #endregion

            #region Kiểm tra tồn tại
            var vital = await _dataContext.VcVitalSigns
                .FirstOrDefaultAsync(x => x.VitalSignId == request.VitalSignId, cancellationToken);
            if (vital == null)
            {
                throw new ArgumentException($"Không tìm thấy bản ghi chỉ số sinh tồn với ID: {request.VitalSignId}");
            }
            #endregion

            #region Xóa khỏi CSDL
            int recordId = vital.RecordId;
            _dataContext.VcVitalSigns.Remove(vital);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region Xóa Cache
            _cacheService.Remove(VitalSignsConstant.BuildCacheKey(recordId.ToString()));
            #endregion

            Log.Information($"Xóa chỉ số sinh tồn ID {request.VitalSignId} thành công.");
            return Unit.Value;
        }
    }
}
