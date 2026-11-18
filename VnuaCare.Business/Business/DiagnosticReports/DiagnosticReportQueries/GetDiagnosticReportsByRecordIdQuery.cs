/**
 * Nghiệp vụ Lấy danh sách phiếu kết quả chẩn đoán hình ảnh theo ID hồ sơ (GetDiagnosticReportsByRecordIdQuery)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.DiagnosticReports.DiagnosticReportQueries;

public class GetDiagnosticReportsByRecordIdQuery : IRequest<List<DiagnosticReportModel>>
{
    public int RecordId { get; set; }
    public string? ModalityType { get; set; }

    public GetDiagnosticReportsByRecordIdQuery(int recordId, string? modalityType = null)
    {
        RecordId = recordId;
        ModalityType = modalityType;
    }

    public class Handler : IRequestHandler<GetDiagnosticReportsByRecordIdQuery, List<DiagnosticReportModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly ICacheService _cacheService;

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }

        public async Task<List<DiagnosticReportModel>> Handle(GetDiagnosticReportsByRecordIdQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = string.IsNullOrWhiteSpace(request.ModalityType)
                ? DiagnosticReportConstant.BuildCacheKey(request.RecordId.ToString())
                : DiagnosticReportConstant.BuildCacheKey($"{request.RecordId}_{request.ModalityType.Trim().ToUpperInvariant()}");

            var item = await _cacheService.GetOrCreate<List<DiagnosticReportModel>>(cacheKey, async () =>
            {
                var query = _dataContext.VcDiagnosticReports.AsNoTracking()
                    .Include(x => x.ReadingDoctor)
                    .Include(x => x.ApprovingDoctor)
                    .Where(x => x.RecordId == request.RecordId);

                if (!string.IsNullOrWhiteSpace(request.ModalityType))
                {
                    var norm = request.ModalityType.Trim().ToUpperInvariant();
                    query = query.Where(x => x.ModalityType == norm);
                }

                var list = await query
                    .OrderByDescending(x => x.PerformedAt)
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
                    .ToListAsync(cancellationToken);

                return list;
            });

            return item ?? new List<DiagnosticReportModel>();
        }
    }
}
