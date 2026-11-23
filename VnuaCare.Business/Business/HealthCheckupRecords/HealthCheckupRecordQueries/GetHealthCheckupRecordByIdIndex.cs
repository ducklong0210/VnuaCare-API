/**
 * Nghiệp vụ lấy thông tin chi tiết Hồ sơ khám sức khỏe theo ID (kèm Cache)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordQueries;

public class GetHealthCheckupRecordByIdIndex : IRequest<HealthCheckupRecordModel?>
{
    public int Id { get; init; }

    public GetHealthCheckupRecordByIdIndex(int id)
    {
        Id = id;
    }

    public class Handler : IRequestHandler<GetHealthCheckupRecordByIdIndex, HealthCheckupRecordModel?>
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

        public async Task<HealthCheckupRecordModel?> Handle(GetHealthCheckupRecordByIdIndex request,
            CancellationToken cancellationToken)
        {
            var id = request.Id;

            if (_contextAccessor.Role == Role.STAFF.ToString())
            {
                var currentUserId = _contextAccessor.UserId;
                var isOwner = currentUserId.HasValue && await _dataContext.VcHealthCheckupRecords.AsNoTracking()
                    .AnyAsync(r => r.RecordId == id && r.Staff != null && r.Staff.UserId == currentUserId.Value, cancellationToken);
                if (!isOwner)
                {
                    throw new UnauthorizedAccessException("Bạn chỉ có quyền xem hồ sơ khám sức khỏe của chính mình.");
                }
            }

            // Xây dựng khóa bộ nhớ đệm (Cache Key) theo ID hồ sơ khám sức khỏe
            string cacheKey = HealthCheckupRecordConstant.BuildCacheKey(id.ToString());
            
            // Đọc chi tiết hồ sơ khám từ Cache trước; nếu chưa có thì truy vấn CSDL bằng AsNoTracking,
            // ánh xạ sang HealthCheckupRecordModel (lấy tên cán bộ & bác sĩ kết luận từ vc_users) rồi lưu vào Cache
            var item = await _cacheService.GetOrCreate<HealthCheckupRecordModel?>(cacheKey, async () =>
            {
                var entity = await _dataContext.VcHealthCheckupRecords.AsNoTracking()
                    .Where(x => x.RecordId == id)
                    .Select(x => new HealthCheckupRecordModel
                    {
                        RecordId = x.RecordId,
                        SidCode = x.SidCode,
                        CampaignId = x.CampaignId,
                        CampaignName = x.Campaign != null ? x.Campaign.CampaignName : null,
                        StaffId = x.StaffId,
                        StaffName = x.Staff != null && x.Staff.User != null ? x.Staff.User.FullName : null,
                        EmployeeCode = x.Staff != null ? x.Staff.EmployeeCode : null,
                        DepartmentName = x.Staff != null && x.Staff.Department != null ? x.Staff.Department.DepartmentName : null,
                        DoctorName = x.Doctor != null && x.Doctor.User != null ? x.Doctor.User.FullName : null,
                        ConcludedByDoctorId = x.ConcludedByDoctorId ?? 0,
                        Gender = x.Staff != null ? x.Staff.Gender : null,
                        DateOfBirth = x.Staff != null ? x.Staff.DateOfBirth : default,
                        PhoneNumber = x.Staff != null ? x.Staff.PhoneNumber ?? string.Empty : string.Empty,
                        ActualCheckupDate = x.ActualCheckupDate,
                        ExaminationDate = x.ExaminationDate,
                        MedicalHistoryPersonal = x.MedicalHistoryPersonal,
                        MedicalHistoryFamily = x.MedicalHistoryFamily,
                        HealthClassification = x.HealthClassification,
                        GeneralConclusion = x.GeneralConclusion,
                        Recommendations = x.Recommendations,
                        ConcludedAt = x.ConcludedAt,
                        ReportPdfUrl = x.ReportPdfUrl,
                        Status = x.Status,
                    }).FirstOrDefaultAsync(cancellationToken);

                return entity;
            });

            return item;
        }
    }
}
