/**
 * Nghiệp vụ lấy danh sách Cán bộ có sức khỏe suy giảm so với năm trước (GetDeclinedHealthRecordsQuery)
 * So sánh HealthClassification giữa đợt khám hiện tại với hồ sơ COMPLETED gần nhất của năm trước đó
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordQueries;

public class GetDeclinedHealthRecordsQuery : IRequest<List<DeclinedHealthRecordModel>>
{
    public int? CampaignId { get; set; }

    public GetDeclinedHealthRecordsQuery(int? campaignId = null)
    {
        CampaignId = campaignId;
    }

    public class Handler : IRequestHandler<GetDeclinedHealthRecordsQuery, List<DeclinedHealthRecordModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareReadDataContext dataContext, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _contextAccessor = contextAccessor();
        }

        public async Task<List<DeclinedHealthRecordModel>> Handle(GetDeclinedHealthRecordsQuery request, CancellationToken cancellationToken)
        {
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.DOCTOR.ToString())
            {
                throw new UnauthorizedAccessException("Chỉ Quản trị viên y tế hoặc Bác sĩ mới có quyền xem danh sách cảnh báo sức khỏe.");
            }

            // Lấy các hồ sơ đã hoàn thành (COMPLETED) và có xếp loại sức khỏe
            var currentQuery = _dataContext.VcHealthCheckupRecords.AsNoTracking()
                .Where(x => x.Status == "COMPLETED" && x.HealthClassification != null && x.Campaign != null);

            if (request.CampaignId.HasValue && request.CampaignId.Value > 0)
            {
                currentQuery = currentQuery.Where(x => x.CampaignId == request.CampaignId.Value);
            }

            var currentRecords = await currentQuery
                .Select(x => new
                {
                    x.RecordId,
                    x.SidCode,
                    x.CampaignId,
                    CampaignName = x.Campaign != null ? x.Campaign.CampaignName : null,
                    Year = x.Campaign != null ? x.Campaign.Year : 0,
                    x.StaffId,
                    EmployeeCode = x.Staff != null ? x.Staff.EmployeeCode : string.Empty,
                    StaffName = x.Staff != null && x.Staff.User != null ? (x.Staff.User.FullName ?? string.Empty) : string.Empty,
                    DepartmentName = x.Staff != null && x.Staff.Department != null ? x.Staff.Department.DepartmentName : null,
                    Email = x.Staff != null && x.Staff.User != null ? x.Staff.User.Email : null,
                    x.HealthClassification,
                    x.GeneralConclusion,
                    x.Recommendations
                })
                .ToListAsync(cancellationToken);

            if (currentRecords.Count == 0)
            {
                return new List<DeclinedHealthRecordModel>();
            }

            var staffIds = currentRecords.Select(r => r.StaffId).Distinct().ToList();
            var recordIds = currentRecords.Select(r => r.RecordId).ToList();

            // Lấy toàn bộ hồ sơ COMPLETED của các cán bộ trên ở các năm trước để so sánh
            var historicalRecords = await _dataContext.VcHealthCheckupRecords.AsNoTracking()
                .Where(x => staffIds.Contains(x.StaffId) && x.Status == "COMPLETED" && x.HealthClassification != null && x.Campaign != null)
                .Select(x => new
                {
                    x.RecordId,
                    x.StaffId,
                    Year = x.Campaign != null ? x.Campaign.Year : 0,
                    x.HealthClassification
                })
                .ToListAsync(cancellationToken);

            // Tra cứu thời điểm đã gửi email cảnh báo thành công gần nhất từ vc_email_logs
            var sentLogs = await _dataContext.VcEmailLogs.AsNoTracking()
                .Where(l => l.ReferenceType == "HealthCheckupRecord" && l.SentStatus == "Success" && recordIds.Contains(l.ReferenceId))
                .GroupBy(l => l.ReferenceId)
                .Select(g => new { RecordId = g.Key, LastSentAt = g.Max(x => x.CreatedDate) })
                .ToDictionaryAsync(x => x.RecordId, x => x.LastSentAt, cancellationToken);

            var result = new List<DeclinedHealthRecordModel>();

            foreach (var cur in currentRecords)
            {
                var curRank = HealthClassificationHelper.ParseRank(cur.HealthClassification);
                if (!curRank.HasValue) continue;

                // Tìm hồ sơ năm gần nhất trước năm hiện tại của cùng cán bộ
                var prev = historicalRecords
                    .Where(h => h.StaffId == cur.StaffId && h.Year < cur.Year)
                    .OrderByDescending(h => h.Year)
                    .FirstOrDefault();

                if (prev == null) continue;

                var prevRank = HealthClassificationHelper.ParseRank(prev.HealthClassification);
                if (!prevRank.HasValue) continue;

                // Bậc số lớn hơn nghĩa là sức khỏe suy giảm (ví dụ Loại I = 1 -> Loại II = 2 hoặc Loại III = 3)
                if (curRank.Value > prevRank.Value)
                {
                    sentLogs.TryGetValue(cur.RecordId, out var lastSentAt);
                    result.Add(new DeclinedHealthRecordModel
                    {
                        RecordId = cur.RecordId,
                        SidCode = cur.SidCode,
                        CampaignId = cur.CampaignId,
                        CampaignName = cur.CampaignName,
                        CurrentYear = cur.Year,
                        StaffId = cur.StaffId,
                        EmployeeCode = cur.EmployeeCode,
                        StaffName = cur.StaffName,
                        DepartmentName = cur.DepartmentName,
                        Email = cur.Email,
                        PreviousYear = prev.Year,
                        PreviousClassification = prev.HealthClassification,
                        CurrentClassification = cur.HealthClassification,
                        GeneralConclusion = cur.GeneralConclusion,
                        Recommendations = cur.Recommendations,
                        AlertSentAt = lastSentAt == default ? null : lastSentAt
                    });
                }
            }

            return result.OrderByDescending(x => x.CurrentYear).ThenBy(x => x.StaffName).ToList();
        }
    }
}
