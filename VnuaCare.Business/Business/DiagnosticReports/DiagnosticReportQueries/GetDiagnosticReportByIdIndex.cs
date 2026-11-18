/**
 * Nghiệp vụ Lấy chi tiết phiếu kết quả chẩn đoán hình ảnh theo ID (GetDiagnosticReportByIdIndex)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.DataContext;

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

        public Handler(VnuaCareReadDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<DiagnosticReportModel?> Handle(GetDiagnosticReportByIdIndex request, CancellationToken cancellationToken)
        {
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
                    ReadingDoctorName = x.ReadingDoctor != null ? x.ReadingDoctor.FullName : null,
                    ApprovingDoctorId = x.ApprovingDoctorId,
                    ApprovingDoctorName = x.ApprovingDoctor != null ? x.ApprovingDoctor.FullName : null,
                    PerformedAt = x.PerformedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            return report;
        }
    }
}
