/**
 * Nghiệp vụ Xóa phiếu kết quả chẩn đoán hình ảnh (DeleteDiagnosticReportCommand)
 * Chuẩn CRUD: Xóa bản ghi kết quả chẩn đoán hình ảnh
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.DiagnosticReports.DiagnosticReportCommands;

public class DeleteDiagnosticReportCommand : IRequest<Unit>
{
    public int ReportId { get; }

    public DeleteDiagnosticReportCommand(int reportId)
    {
        ReportId = reportId;
    }

    public class Handler : IRequestHandler<DeleteDiagnosticReportCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để xóa phiếu kết quả chẩn đoán hình ảnh
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi xóa dữ liệu
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu và xóa bản ghi khỏi CSDL
        public async Task<Unit> Handle(DeleteDiagnosticReportCommand request, CancellationToken cancellationToken)
        {
            Log.Information($"Bắt đầu xóa phiếu kết quả chẩn đoán hình ảnh ID: {request.ReportId}");

            #region Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Chỉ Bác sĩ hoặc Quản trị viên y tế mới có quyền xóa phiếu chẩn đoán hình ảnh.");
            }
            #endregion

            #region Kiểm tra tồn tại
            var report = await _dataContext.VcDiagnosticReports
                .FirstOrDefaultAsync(x => x.ReportId == request.ReportId, cancellationToken);
            if (report == null)
            {
                throw new ArgumentException($"Không tìm thấy phiếu kết quả chẩn đoán hình ảnh với ID: {request.ReportId}");
            }
            #endregion

            #region Xóa khỏi CSDL
            int recordId = report.RecordId;
            _dataContext.VcDiagnosticReports.Remove(report);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region Xóa Cache
            _cacheService.Remove(DiagnosticReportConstant.BuildCacheKey(recordId.ToString()));
            #endregion

            Log.Information($"Xóa phiếu kết quả chẩn đoán hình ảnh ID {request.ReportId} thành công.");
            return Unit.Value;
        }
    }
}
