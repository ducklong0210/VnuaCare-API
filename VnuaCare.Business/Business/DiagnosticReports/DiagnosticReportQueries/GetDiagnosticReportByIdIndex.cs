/**
 * Nghiệp vụ Lấy chi tiết phiếu kết quả chẩn đoán hình ảnh theo ID (GetDiagnosticReportByIdIndex)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.DiagnosticReports.DiagnosticReportQueries;

public class GetDiagnosticReportByIdIndex : IRequest<DiagnosticReportModel?>
{
    public int Id { get; set; }

    public GetDiagnosticReportByIdIndex(int id)
    {
        Id = id;
    }

    public class Handler : IRequestHandler<GetDiagnosticReportByIdIndex, DiagnosticReportModel?>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareReadDataContext dataContext, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _contextAccessor = contextAccessor();
        }

        public async Task<DiagnosticReportModel?> Handle(GetDiagnosticReportByIdIndex request, CancellationToken cancellationToken)
        {
            if (_contextAccessor.Role == Role.STAFF.ToString())
            {
                var currentUserId = _contextAccessor.UserId;
                var isOwner = currentUserId.HasValue && await _dataContext.VcDiagnosticReports.AsNoTracking()
                    .AnyAsync(r => r.ReportId == request.Id && r.HealthCheckupRecord != null && r.HealthCheckupRecord.Staff != null && r.HealthCheckupRecord.Staff.UserId == currentUserId.Value, cancellationToken);
                if (!isOwner)
                {
                    throw new UnauthorizedAccessException("Bạn chỉ có quyền xem phiếu chẩn đoán hình ảnh của chính mình.");
                }
            }

            var report = await _dataContext.VcDiagnosticReports.AsNoTracking()
                .Include(x => x.ReadingDoctor)
                .Include(x => x.ApprovingDoctor)
                .Where(x => x.ReportId == request.Id)
                .Select(x => new DiagnosticReportModel
                {
                    ReportId = x.ReportId,
                    RecordId = x.RecordId,
                    ModalityType = x.ModalityType,
                    ExamTitle = x.ExamTitle,
                    TechniqueDescription = x.TechniqueDescription,
                    FindingsData = x.FindingsData,
                    Conclusion = x.Conclusion,
                    ImageUrls = x.ImageUrls,
                    ReadingDoctorId = x.ReadingDoctorId,
                    ReadingDoctorName = x.ReadingDoctor != null && x.ReadingDoctor.User != null ? x.ReadingDoctor.User.FullName : null,
                    ApprovingDoctorId = x.ApprovingDoctorId,
                    ApprovingDoctorName = x.ApprovingDoctor != null && x.ApprovingDoctor.User != null ? x.ApprovingDoctor.User.FullName : null,
                    PerformedAt = x.PerformedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            return report;
        }
    }
}
