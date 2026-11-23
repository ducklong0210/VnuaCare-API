/**
 * Nghiệp vụ lọc, tìm kiếm và phân trang danh sách Hồ sơ khám sức khỏe
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;
using VnuaCare.Shared.Utils;

namespace VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordQueries;

public class GetFilterHealthCheckupRecordQuery : IRequest<PaginationList<HealthCheckupRecordModel>>
{
    public HealthCheckupRecordFilterModel Filter { get; set; }
    
    public GetFilterHealthCheckupRecordQuery(HealthCheckupRecordFilterModel filter)
    {
        Filter = filter;
    }

    public class Handler : IRequestHandler<GetFilterHealthCheckupRecordQuery, PaginationList<HealthCheckupRecordModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareReadDataContext dataContext, IContextAccessor contextAccessor)
        {
            _dataContext = dataContext;
            _contextAccessor = contextAccessor;
        }

        public async Task<PaginationList<HealthCheckupRecordModel>> Handle(
            GetFilterHealthCheckupRecordQuery request,
            CancellationToken cancellationToken)
        {
            var filter = request.Filter;
            var currentUserRole = _contextAccessor.Role;
            var currentUserId = _contextAccessor.UserId;

            var query = _dataContext.VcHealthCheckupRecords.AsNoTracking();

            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.STAFF.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập danh sách hồ sơ khám sức khỏe.");
            }

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

            // Lọc theo Trạng thái hồ sơ
            if (!string.IsNullOrWhiteSpace(filter.Status) && filter.Status != "string")
            {
                query = query.Where(x => x.Status == filter.Status.Trim());
            }

            // Lọc theo Phân loại sức khỏe
            if (!string.IsNullOrWhiteSpace(filter.HealthClassification) && filter.HealthClassification != "string")
            {
                query = query.Where(x => x.HealthClassification == filter.HealthClassification.Trim());
            }

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrWhiteSpace(filter.TextSearch) && filter.TextSearch != "string")
            {
                string ts = filter.TextSearch.Trim().ToLower();
                query = query.Where(x =>
                    (x.SidCode != null && x.SidCode.ToLower().Contains(ts)) ||
                    (x.Staff != null && x.Staff.User != null && x.Staff.User.FullName != null && x.Staff.User.FullName.ToLower().Contains(ts)) ||
                    (x.Staff != null && x.Staff.EmployeeCode.ToLower().Contains(ts)));
            }

            // Phân trang
            if (filter.PageSize <= 0) filter.PageSize = 10;
            if (filter.PageNumber <= 0) filter.PageNumber = 1;
            
            int totalCount = await query.CountAsync(cancellationToken);
            int excludedRows = (filter.PageNumber - 1) * filter.PageSize;

            var listData = await query
                .OrderBy(x => x.RecordId)
                .Skip(excludedRows)
                .Take(filter.PageSize)
                .Select(x => new HealthCheckupRecordModel
                {
                    RecordId = x.RecordId,
                    SidCode = x.SidCode,
                    CampaignId = x.CampaignId,
                    CampaignName = x.Campaign != null ? x.Campaign.CampaignName : null,
                    StaffId = x.StaffId,
                    StaffName = x.Staff != null && x.Staff.User != null ? x.Staff.User.FullName : null,
                    EmployeeCode = x.Staff != null ? x.Staff.EmployeeCode : null,
                    DateOfBirth = x.Staff != null ? x.Staff.DateOfBirth : default,
                    PhoneNumber = x.Staff != null ? x.Staff.PhoneNumber ?? string.Empty : string.Empty,
                    DepartmentName = x.Staff != null && x.Staff.Department != null ? x.Staff.Department.DepartmentName : null,
                    DoctorName = x.Doctor != null && x.Doctor.User != null ? x.Doctor.User.FullName : null,
                    ConcludedByDoctorId = x.ConcludedByDoctorId ?? 0,
                    ExaminationDate = x.ExaminationDate,
                    ActualCheckupDate = x.ActualCheckupDate,
                    MedicalHistoryPersonal = x.MedicalHistoryPersonal,
                    MedicalHistoryFamily = x.MedicalHistoryFamily,
                    HealthClassification = x.HealthClassification,
                    GeneralConclusion = x.GeneralConclusion,
                    Gender = x.Staff != null ? x.Staff.Gender : null,
                    Recommendations = x.Recommendations,
                    ConcludedAt = x.ConcludedAt,
                    Status = x.Status,
                    ReportPdfUrl = x.ReportPdfUrl,
                }).ToListAsync(cancellationToken);

            return new PaginationList<HealthCheckupRecordModel>
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
