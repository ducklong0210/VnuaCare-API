/**
 * Nghiệp vụ lọc, tìm kiếm và phân trang danh sách Bác sĩ BVĐK MEDLATEC
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;
using VnuaCare.Shared.Utils;

namespace VnuaCare.Business.Business.Doctors.DoctorQueries;

public class GetFilterDoctorQuery : IRequest<PaginationList<DoctorModel>>
{
    public DoctorFilterModel Filter { get; init; }

    public GetFilterDoctorQuery(DoctorFilterModel filter)
    {
        Filter = filter;
    }
    
    public class Handler : IRequestHandler<GetFilterDoctorQuery, PaginationList<DoctorModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareReadDataContext context, IContextAccessor contextAccessor)
        {
            _dataContext = context;
            _contextAccessor = contextAccessor;
        }

        public async Task<PaginationList<DoctorModel>> Handle(GetFilterDoctorQuery request,
            CancellationToken cancellationToken)
        {
            var filter = request.Filter;
            var currentUserRole = _contextAccessor.Role;

            var query = _dataContext.VcDoctors.AsNoTracking();
    
            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem danh sách bác sĩ.");
            }

            //Lọc theo Chuyên khoa
            if (filter.Specialty > 0)
            {
                query = query.Where(q => q.SpecialtyId == filter.Specialty.Value);
            }

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrWhiteSpace(filter.TextSearch) && filter.TextSearch != "string")
            {
                string ts = filter.TextSearch.Trim().ToLower();
                query = query.Where(q => q.FullName.ToLower().Contains(ts) ||
                                         q.DoctorCode.ToLower().Contains(ts) ||
                                         q.HospitalName.ToLower().Contains(ts));
            }

            // Lọc theo trạng thái hoạt động
            if (filter.IsActive.HasValue)
            {
                query = query.Where(x => x.User == null || x.User.IsActive == filter.IsActive.Value);
            }
            
            // Phân trang
            if (filter.PageSize <= 0) filter.PageSize = 10;
            if (filter.PageNumber <= 0) filter.PageNumber = 1;
            
            int totalCount = await query.CountAsync(cancellationToken);
            int excludedRows = (filter.PageNumber - 1) * filter.PageSize;
            
            var listData = await query
                .Include(x => x.Specialty)
                .OrderBy(x => x.DoctorId)
                .Skip(excludedRows)
                .Take(filter.PageSize)
                .Select(x => new DoctorModel
                {
                    UserId = x.UserId,
                    DoctorId = x.DoctorId,
                    DoctorCode = x.DoctorCode,
                    FullName = x.FullName,
                    SpecialtyId = x.SpecialtyId,
                    SpesialtyName = x.Specialty != null ? x.Specialty.SpecialtyName : "",
                    HospitalName = x.HospitalName,
                    LicenseNumber = x.LicenseNumber,
                    Email = x.User != null ? x.User.Email : ""
                }).ToListAsync(cancellationToken);

            return new PaginationList<DoctorModel>
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
