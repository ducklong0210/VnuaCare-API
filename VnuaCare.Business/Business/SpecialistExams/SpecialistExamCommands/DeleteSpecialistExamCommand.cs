/**
 * Nghiệp vụ Xóa kết quả khám chuyên khoa lẻ (DeleteSpecialistExamCommand)
 * Chuẩn CRUD: Xóa bản ghi kết quả khám chuyên khoa
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.SpecialistExams.SpecialistExamCommands;

public class DeleteSpecialistExamCommand : IRequest<Unit>
{
    public int SpecialistExamId { get; }

    public DeleteSpecialistExamCommand(int specialistExamId)
    {
        SpecialistExamId = specialistExamId;
    }

    public class Handler : IRequestHandler<DeleteSpecialistExamCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để xóa kết quả khám chuyên khoa
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi xóa dữ liệu
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu và xóa bản ghi kết quả khám chuyên khoa khỏi CSDL
        public async Task<Unit> Handle(DeleteSpecialistExamCommand request, CancellationToken cancellationToken)
        {
            Log.Information($"Bắt đầu xóa kết quả khám chuyên khoa ID: {request.SpecialistExamId}");

            #region Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Chỉ Bác sĩ chuyên khoa hoặc Quản trị viên y tế mới có quyền xóa kết quả khám chuyên khoa.");
            }
            #endregion

            #region Kiểm tra tồn tại
            var exam = await _dataContext.VcSpecialistExams
                .FirstOrDefaultAsync(x => x.SpecialistExamId == request.SpecialistExamId, cancellationToken);
            if (exam == null)
            {
                throw new ArgumentException($"Không tìm thấy kết quả khám chuyên khoa với ID: {request.SpecialistExamId}");
            }
            #endregion

            #region Xóa khỏi CSDL
            int recordId = exam.RecordId;
            _dataContext.VcSpecialistExams.Remove(exam);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region Xóa Cache
            _cacheService.Remove(SpecialistExamConstant.BuildCacheKey(recordId.ToString()));
            #endregion

            Log.Information($"Xóa kết quả khám chuyên khoa ID {request.SpecialistExamId} thành công.");
            return Unit.Value;
        }
    }
}
