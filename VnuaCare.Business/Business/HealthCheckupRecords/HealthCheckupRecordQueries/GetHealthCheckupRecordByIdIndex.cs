/**
 * Nghiệp vụ lấy thông tin chi tiết Hồ sơ khám sức khỏe theo ID:
 * Đọc từ Redis Cache trước, nếu chưa có thì truy vấn từ CSDL kèm thông tin Cán bộ, Đợt khám và Bác sĩ.
 * Phân quyền: Cán bộ (STAFF) chỉ có quyền xem hồ sơ của chính mình.
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordQueries;

/// <summary>
/// Truy vấn lấy thông tin chi tiết hồ sơ khám sức khỏe theo ID
/// </summary>
public class GetHealthCheckupRecordByIdIndex : IRequest<HealthCheckupRecordModel?>
{
    public int Id { get; set; }

    public GetHealthCheckupRecordByIdIndex(int id)
    {
        Id = id;
    }

    public class Handler : IRequestHandler<GetHealthCheckupRecordByIdIndex, HealthCheckupRecordModel?>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly ICacheService _cacheService;
        private readonly IContextAccessor _contextAccessor;

        public Handler(
            VnuaCareReadDataContext dataContext, 
            ICacheService cacheService,
            IContextAccessor contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor;
        }

        public async Task<HealthCheckupRecordModel?> Handle(GetHealthCheckupRecordByIdIndex request,
            CancellationToken cancellationToken)
        {
            var currentUserRole = _contextAccessor.Role;
            var currentUserId = _contextAccessor.UserId;

            string cacheKey = HealthCheckupRecordConstant.BuildCacheKey(request.Id.ToString());
            
            // Đọc từ Cache nếu có, nếu chưa có thì truy vấn từ CSDL và lưu vào Cache
            var item = await _cacheService.GetOrCreate<HealthCheckupRecordModel>(cacheKey, async () =>
            {
                var entity = await _dataContext.VcHealthCheckupRecords.AsNoTracking()
                    .Where(x => x.RecordId == request.Id)
                    .Select(x => new HealthCheckupRecordModel
                    {
                        RecordId = x.RecordId,
                        SidCode = x.SidCode,
                        CampaignId = x.CampaignId,
                        CampaignName = x.Campaign != null ? x.Campaign.CampaignName : null,
                        StaffId = x.StaffId,
                        StaffName = x.Staff != null ? x.Staff.FullName : null,
                        EmployeeCode = x.Staff != null ? x.Staff.EmployeeCode : null,
                        DepartmentName = x.Staff != null && x.Staff.Department != null ? x.Staff.Department.DepartmentName : null,
                        DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                        ConcludedByDoctorId = x.ConcludedByDoctorId ?? 0,
                        Gender = x.Staff != null ? x.Staff.Gender : null,
                        DateOfBirth = x.Staff != null ? x.Staff.DateOfBirth : default,
                        PhoneNumber = x.Staff != null ? x.Staff.PhoneNumber ?? string.Empty : string.Empty,
                        ActualCheckupDate = x.ActualCheckupDate,
                        MedicalHistoryPersonal = x.MedicalHistoryPersonal,
                        MedicalHistoryFamily = x.MedicalHistoryFamily,
                        HealthClassification = x.HealthClassification,
                        GeneralConclusion = x.GeneralConclusion,
                        Recommentdations = x.Recommendations,
                        ConcludedAt = x.ConcludedAt,
                        ReportPdfUrl = x.ReportPdfUrl,
                        Status = x.Status,
                    }).FirstOrDefaultAsync(cancellationToken);

                return entity;
            });

            // Nếu người dùng là Cán bộ (STAFF), đảm bảo không xem trộm hồ sơ của người khác
            if (item != null && currentUserRole == Role.STAFF.ToString())
            {
                var isOwnRecord = await _dataContext.VcStaffs.AsNoTracking()
                    .AnyAsync(s => s.StaffId == item.StaffId && s.UserId == currentUserId, cancellationToken);
                if (!isOwnRecord)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền xem hồ sơ khám sức khỏe của người khác.");
                }
            }

            return item;
        }
    }
}