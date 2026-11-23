/**
 * Nghiệp vụ Lọc, tìm kiếm và phân trang danh sách phiếu kết quả chẩn đoán hình ảnh (GetFilterDiagnosticReportQuery)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;
using VnuaCare.Shared.Utils;

namespace VnuaCare.Business.Business.DiagnosticReports.DiagnosticReportQueries;

public class GetFilterDiagnosticReportQuery : IRequest<PaginationList<DiagnosticReportModel>>
{
    public DiagnosticReportFilterModel Filter { get; set; }

    public GetFilterDiagnosticReportQuery(DiagnosticReportFilterModel filter)
    {
        Filter = filter;
    }

    public class Handler : IRequestHandler<GetFilterDiagnosticReportQuery, PaginationList<DiagnosticReportModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareReadDataContext dataContext, IContextAccessor contextAccessor)
        {
            _dataContext = dataContext;
            _contextAccessor = contextAccessor;
        }

        public async Task<PaginationList<DiagnosticReportModel>> Handle(GetFilterDiagnosticReportQuery request, CancellationToken cancellationToken)
        {
            var filter = request.Filter;
            var currentUserRole = _contextAccessor.Role;

            var query = _dataContext.VcDiagnosticReports.AsNoTracking();

            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.DOCTOR.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem danh sách chẩn đoán hình ảnh.");
            }

            // Lọc theo RecordId
            if (filter.RecordId.HasValue && filter.RecordId.Value > 0)
            {
                query = query.Where(x => x.RecordId == filter.RecordId.Value);
            }

            // Lọc theo Loại chẩn đoán (ULTRASOUND, XRAY, ECG, ENDOSCOPY, CT, MRI)
            if (!string.IsNullOrWhiteSpace(filter.ModalityType) && filter.ModalityType != "string")
            {
                string normModality = filter.ModalityType.Trim().ToUpperInvariant();
                query = query.Where(x => x.ModalityType == normModality);
            }

            // Lọc theo Bác sĩ đọc
            if (filter.ReadingDoctorId.HasValue && filter.ReadingDoctorId.Value > 0)
            {
                query = query.Where(x => x.ReadingDoctorId == filter.ReadingDoctorId.Value);
            }

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrWhiteSpace(filter.TextSearch) && filter.TextSearch != "string")
            {
                string ts = filter.TextSearch.Trim().ToLower();
                query = query.Where(x =>
                    x.ExamTitle.ToLower().Contains(ts) ||
                    x.Conclusion.ToLower().Contains(ts) ||
                    (x.FindingsData != null && x.FindingsData.ToLower().Contains(ts)));
            }

            // Phân trang
            if (filter.PageSize <= 0) filter.PageSize = 10;
            if (filter.PageNumber <= 0) filter.PageNumber = 1;

            int totalCount = await query.CountAsync(cancellationToken);
            int excludedRows = (filter.PageNumber - 1) * filter.PageSize;

            var listData = await query
                .OrderByDescending(x => x.PerformedAt)
                .Skip(excludedRows)
                .Take(filter.PageSize)
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
                .ToListAsync(cancellationToken);

            return new PaginationList<DiagnosticReportModel>
            {
                Data = listData,
                DataCount = listData.Count,
                TotalCount = totalCount,
                PageSize = filter.PageSize,
                PageNumber = filter.PageNumber
            };
        }
    }
}
