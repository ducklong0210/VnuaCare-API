/**
 * Nghiệp vụ Xóa kết quả khám lâm sàng Nội khoa tổng quát (DeleteInternalMedicineExamCommand)
 * Chuẩn CRUD: Xóa bản ghi kết quả khám nội khoa
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.InternalMedicine.InternalMedicineCommands;

public class DeleteInternalMedicineExamCommand : IRequest<Unit>
{
    public int InternalExamId { get; }

    public DeleteInternalMedicineExamCommand(int internalExamId)
    {
        InternalExamId = internalExamId;
    }

    public class Handler : IRequestHandler<DeleteInternalMedicineExamCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để xóa kết quả khám nội khoa
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi xóa dữ liệu
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu và xóa bản ghi kết quả khám nội khoa khỏi CSDL
        public async Task<Unit> Handle(DeleteInternalMedicineExamCommand request, CancellationToken cancellationToken)
        {
            Log.Information($"Bắt đầu xóa kết quả khám nội khoa ID: {request.InternalExamId}");

            #region Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Chỉ Bác sĩ hoặc Quản trị viên y tế mới có quyền xóa kết quả khám nội khoa.");
            }
            #endregion

            #region Kiểm tra tồn tại
            var exam = await _dataContext.VcInternalMedicineExams
                .FirstOrDefaultAsync(x => x.InternalExamId == request.InternalExamId, cancellationToken);
            if (exam == null)
            {
                throw new ArgumentException($"Không tìm thấy kết quả khám nội khoa với ID: {request.InternalExamId}");
            }
            #endregion

            #region Xóa khỏi CSDL
            int recordId = exam.RecordId;
            _dataContext.VcInternalMedicineExams.Remove(exam);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region Xóa Cache
            _cacheService.Remove(InternalMedicineConstant.BuildCacheKey(recordId.ToString()));
            #endregion

            Log.Information($"Xóa kết quả khám nội khoa ID {request.InternalExamId} thành công.");
            return Unit.Value;
        }
    }
}
