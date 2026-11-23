/**
 * Nghiệp vụ Lấy danh sách phiếu kết quả chẩn đoán hình ảnh theo ID hồ sơ (GetDiagnosticReportsByRecordIdQuery)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

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
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        public async Task<List<DiagnosticReportModel>> Handle(GetDiagnosticReportsByRecordIdQuery request, CancellationToken cancellationToken)
        {
            if (_contextAccessor.Role == Role.STAFF.ToString())
            {
                var currentUserId = _contextAccessor.UserId;
                var isOwner = currentUserId.HasValue && await _dataContext.VcHealthCheckupRecords.AsNoTracking()
                    .AnyAsync(r => r.RecordId == request.RecordId && r.Staff != null && r.Staff.UserId == currentUserId.Value, cancellationToken);
                if (!isOwner)
                {
                    throw new UnauthorizedAccessException("Bạn chỉ có quyền xem kết quả chẩn đoán hình ảnh trong hồ sơ của chính mình.");
                }
            }

            // Xây dựng khóa bộ nhớ đệm (Cache Key) theo ID hồ sơ và loại kỹ thuật CĐHA (nếu có lọc)
            string cacheKey = string.IsNullOrWhiteSpace(request.ModalityType)
                ? DiagnosticReportConstant.BuildCacheKey(request.RecordId.ToString())
                : DiagnosticReportConstant.BuildCacheKey($"{request.RecordId}_{request.ModalityType.Trim().ToUpperInvariant()}");

            // Đọc danh sách phiếu chẩn đoán hình ảnh từ Cache trước; nếu chưa có thì truy vấn CSDL bằng AsNoTracking,
            // sắp xếp phiếu mới nhất lên đầu, chiếu sang DiagnosticReportModel (lấy tên bác sĩ đọc/duyệt từ vc_users) rồi lưu Cache
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
                        ReadingDoctorName = x.ReadingDoctor != null && x.ReadingDoctor.User != null ? x.ReadingDoctor.User.FullName : null,
                        ApprovingDoctorId = x.ApprovingDoctorId,
                        ApprovingDoctorName = x.ApprovingDoctor != null && x.ApprovingDoctor.User != null ? x.ApprovingDoctor.User.FullName : null,
                        PerformedAt = x.PerformedAt
                    })
                    .ToListAsync(cancellationToken);

                return list;
            });

            return item ?? new List<DiagnosticReportModel>();
        }
    }
}
