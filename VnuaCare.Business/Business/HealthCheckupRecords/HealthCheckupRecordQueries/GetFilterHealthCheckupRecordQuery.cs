/**
 * Nghiệp vụ lọc và phân trang danh sách Hồ sơ khám sức khỏe:
 * Hỗ trợ tìm kiếm theo từ khóa (Mã SID, Tên/Mã cán bộ, Bác sĩ), lọc theo Đợt khám, Khoa/Phòng ban,
 * Cán bộ, Bác sĩ kết luận, Xếp loại sức khỏe và Trạng thái hồ sơ.
 * Phân quyền: ADMIN/DOCTOR xem danh sách theo bộ lọc; STAFF chỉ xem được hồ sơ của chính mình.
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;
using VnuaCare.Shared.Utils;

namespace VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordQueries;

/// <summary>
/// Truy vấn lọc và phân trang hồ sơ khám sức khỏe
/// </summary>
public class GetFilterHealthCheckupRecordQuery : IRequest<PaginationList<HealthCheckupRecordModel>>
{
    public HealthCheckupRecordFilterModel Filter { get; set; }
    
    public GetFilterHealthCheckupRecordQuery(HealthCheckupRecordFilterModel filter)
    {
        Filter = filter;
    }

    public class Handler : IRequestHandler<GetFilterHealthCheckupRecordQuery, PaginationList<HealthCheckupRecordModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;    // DbContext Đọc để tối ưu hiệu năng truy vấn
        private readonly ICacheService _cacheService;            // Dịch vụ Cache
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất thông tin người dùng

        public Handler(
            VnuaCareReadDataContext dataContext, 
            ICacheService cacheService, 
            IContextAccessor contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor;
        }

        public async Task<PaginationList<HealthCheckupRecordModel>> Handle(
            GetFilterHealthCheckupRecordQuery request,
            CancellationToken cancellationToken)
        {
            var filter = request.Filter;
            var currentUserRole = _contextAccessor.Role;
            var currentUserId = _contextAccessor.UserId;

            #region 1. Phân quyền người thực hiện (Authorization)
            // Kiểm tra vai trò hợp lệ: SUPER_ADMIN, HEALTH_ADMIN, DOCTOR hoặc STAFF
            if (currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.STAFF.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập danh sách hồ sơ khám sức khỏe.");
            }
            #endregion

            #region 2. Xây dựng điều kiện lọc (Filter Conditions)
            var query = _dataContext.VcHealthCheckupRecords.AsNoTracking();

            // Nếu người dùng là Cán bộ (STAFF), chỉ cho phép xem các hồ sơ của chính mình
            if (currentUserRole == Role.STAFF.ToString())
            {
                var staff = await _dataContext.VcStaffs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.UserId == currentUserId, cancellationToken);

                if (staff == null)
                {
                    return new PaginationList<HealthCheckupRecordModel>
                    {
                        DataCount = 0,
                        TotalCount = 0,
                        PageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber,
                        PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize,
                        Data = new List<HealthCheckupRecordModel>()
                    };
                }

                query = query.Where(x => x.StaffId == staff.StaffId);
            }
            else
            {
                // Đối với Admin/Doctor: Có thể lọc theo Cán bộ cụ thể
                if (filter.StaffId.HasValue && filter.StaffId.Value > 0)
                {
                    query = query.Where(x => x.StaffId == filter.StaffId.Value);
                }

                // Lọc theo Khoa / Phòng ban của cán bộ
                if (filter.DepartmentId.HasValue && filter.DepartmentId.Value > 0)
                {
                    query = query.Where(x => x.Staff != null && x.Staff.DepartmentId == filter.DepartmentId.Value);
                }
            }

            // Lọc theo Đợt khám sức khỏe
            if (filter.CampaignId.HasValue && filter.CampaignId.Value > 0)
            {
                query = query.Where(x => x.CampaignId == filter.CampaignId.Value);
            }

            // Lọc theo Bác sĩ ký kết luận
            if (filter.DoctorId.HasValue && filter.DoctorId.Value > 0)
            {
                query = query.Where(x => x.ConcludedByDoctorId == filter.DoctorId.Value);
            }

            // Lọc theo Trạng thái hồ sơ (PENDING, COMPLETED, ...)
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(x => x.Status == filter.Status.Trim());
            }

            // Lọc theo Phân loại sức khỏe (LOAI_I, LOAI_II, ...)
            if (!string.IsNullOrWhiteSpace(filter.HealthClassification))
            {
                query = query.Where(x => x.HealthClassification == filter.HealthClassification.Trim());
            }

            // Tìm kiếm tự do theo từ khóa (TextSearch): Mã SID, Mã/Tên cán bộ, Bác sĩ kết luận, Kết luận, Lời dặn
            if (!string.IsNullOrWhiteSpace(filter.TextSearch))
            {
                string ts = filter.TextSearch.Trim().ToLower();
                query = query.Where(x =>
                    (x.SidCode != null && x.SidCode.ToLower().Contains(ts)) ||
                    (x.Staff != null && x.Staff.FullName != null && x.Staff.FullName.ToLower().Contains(ts)) ||
                    (x.Staff != null && x.Staff.EmployeeCode != null && x.Staff.EmployeeCode.ToLower().Contains(ts)) ||
                    (x.Doctor != null && x.Doctor.FullName != null && x.Doctor.FullName.ToLower().Contains(ts)) ||
                    (x.HealthClassification != null && x.HealthClassification.ToLower().Contains(ts)) ||
                    (x.GeneralConclusion != null && x.GeneralConclusion.ToLower().Contains(ts)) ||
                    (x.Recommendations != null && x.Recommendations.ToLower().Contains(ts)) ||
                    (x.MedicalHistoryPersonal != null && x.MedicalHistoryPersonal.ToLower().Contains(ts)) ||
                    (x.MedicalHistoryFamily != null && x.MedicalHistoryFamily.ToLower().Contains(ts)));
            }
            #endregion

            #region 3. Sắp xếp động và phân trang dữ liệu
            if (filter.PageNumber <= 0) filter.PageNumber = 1;
            if (filter.PageSize <= 0) filter.PageSize = 10;
            
            int totalCount = await query.CountAsync(cancellationToken);
            int excludedRows = (filter.PageNumber - 1) * filter.PageSize;

            bool isAsc = string.Equals(filter.Ascending, "asc", StringComparison.OrdinalIgnoreCase);
            query = (filter.PropertyName?.ToLower(), isAsc) switch
            {
                ("actualcheckupdate", true) => query.OrderBy(x => x.ActualCheckupDate),
                ("actualcheckupdate", false) => query.OrderByDescending(x => x.ActualCheckupDate),
                ("concludedat", true) => query.OrderBy(x => x.ConcludedAt),
                ("concludedat", false) => query.OrderByDescending(x => x.ConcludedAt),
                ("sidcode", true) => query.OrderBy(x => x.SidCode),
                ("sidcode", false) => query.OrderByDescending(x => x.SidCode),
                (_, true) => query.OrderBy(x => x.RecordId),
                _ => query.OrderByDescending(x => x.RecordId)
            };

            var listData = await query
                .Skip(excludedRows)
                .Take(filter.PageSize)
                .Select(x => new HealthCheckupRecordModel
                {
                    RecordId = x.RecordId,
                    SidCode = x.SidCode,
                    CampaignId = x.CampaignId,
                    CampaignName = x.Campaign != null ? x.Campaign.CampaignName : null,
                    StaffId = x.StaffId,
                    StaffName = x.Staff != null ? x.Staff.FullName : null,
                    EmployeeCode = x.Staff != null ? x.Staff.EmployeeCode : null,
                    DateOfBirth = x.Staff != null ? x.Staff.DateOfBirth : default,
                    PhoneNumber = x.Staff != null ? x.Staff.PhoneNumber ?? string.Empty : string.Empty,
                    DepartmentName = x.Staff != null && x.Staff.Department != null ? x.Staff.Department.DepartmentName : null,
                    DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                    ConcludedByDoctorId = x.ConcludedByDoctorId ?? 0,
                    ActualCheckupDate = x.ActualCheckupDate,
                    MedicalHistoryPersonal = x.MedicalHistoryPersonal,
                    MedicalHistoryFamily = x.MedicalHistoryFamily,
                    HealthClassification = x.HealthClassification,
                    GeneralConclusion = x.GeneralConclusion,
                    Gender = x.Staff != null ? x.Staff.Gender : null,
                    Recommentdations = x.Recommendations,
                    ConcludedAt = x.ConcludedAt,
                    Status = x.Status,
                    ReportPdfUrl = x.ReportPdfUrl,
                }).ToListAsync(cancellationToken);

            return new PaginationList<HealthCheckupRecordModel>
            {
                DataCount = listData.Count,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Data = listData
            };
            #endregion
        }
    }
}