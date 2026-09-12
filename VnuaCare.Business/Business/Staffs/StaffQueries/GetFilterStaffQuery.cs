/**
 * Nghiệp vụ lọc, tìm kiếm và phân trang danh sách Cán bộ theo Khoa/Phòng ban
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Utils;
using Role = VnuaCare.Shared.Enums.Role;

namespace VnuaCare.Business.Business.Staffs.StaffQueries;

public class GetFilterStaffQuery : IRequest<PaginationList<StaffModel>>
{
    public StaffFilterModel Filter { get; set; }

    public GetFilterStaffQuery(StaffFilterModel filter)
    {
        Filter = filter;
    }

    public class Handler : IRequestHandler<GetFilterStaffQuery, PaginationList<StaffModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext; // Kết nối CSDL đọc danh sách tối ưu hiệu năng
        private readonly IContextAccessor _contextAccessor;    // Dịch vụ kiểm tra Role của người dùng để phân quyền

        public Handler(VnuaCareReadDataContext dataContext, IContextAccessor contextAccessor)
        {
            _dataContext = dataContext;
            _contextAccessor = contextAccessor;
        }

        // Xử lý tìm kiếm, lọc theo Khoa/Phòng ban và phân trang danh sách Cán bộ
        public async Task<PaginationList<StaffModel>> Handle(GetFilterStaffQuery request,
            CancellationToken cancellationToken)
        {
            var filter = request.Filter;
            var currentUserRole = _contextAccessor.Role;

            var query = _dataContext.VcStaffs.AsNoTracking();
            
            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem danh sách cán bộ.");
            }

            if (filter.DepartmentId.HasValue && filter.DepartmentId.Value > 0)
            {
                query = query.Where(p => p.DepartmentId == filter.DepartmentId.Value);
            }
            // if(!string.IsNullOrEmpty(filter.g))
            if (!string.IsNullOrEmpty((filter.TextSearch)))
            {
                string ts = filter.TextSearch.Trim().ToLower();
                query = query.Where(x => x.FullName.ToLower().Contains(ts) ||
                                         x.EmployeeCode.ToLower().Contains(ts)
                                         || x.JobTitle.ToLower().Contains(ts)
                                         || x.PhoneNumber.ToLower().Contains(ts));
            }
            if (filter.IsActive.HasValue)
            {
                query = query.Where(x => x.User.IsActive == filter.IsActive.Value);
            }

            // sắp xếp
            // query = query.

            // phân trang
            if (filter.PageSize <= 0) filter.PageSize = 10;
            if (filter.PageNumber <= 0) filter.PageNumber = 1;

            int totalCout = await query.CountAsync(cancellationToken);
            int excludedRows = (filter.PageNumber - 1) * filter.PageSize;

            var listData = await query
                .OrderBy(x =>x.StaffId) // mặc định lọc theo id
                .Skip(excludedRows)
                .Take(filter.PageSize).
                Select(x => new StaffModel
            {
                UserId = x.UserId,
                StaffId = x.StaffId,
                StaffCode = x.EmployeeCode,
                FullName = x.FullName,
                Gender =  x.Gender,
                DateOfBirth =  x.DateOfBirth,
                // DepartmentId = x.DepartmentId,
                DepartmentName = x.Department.DepartmentName,
                AcademicTitle =   x.AcademicTitle,
                PhoneNumber = x.PhoneNumber,
                JobTitle = x.JobTitle,
                Adrress = x.Address,
                AvatarUrl = string.IsNullOrEmpty(x.AvatarUrl) ? "" : x.AvatarUrl,
                Email = x.User.Email
            }).ToListAsync(cancellationToken);

            return new PaginationList<StaffModel>()
            {
                DataCount = listData.Count,
                TotalCount = totalCout,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Data = listData
            };
        }
    }
}