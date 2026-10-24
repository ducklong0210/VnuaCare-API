/**
 * Nghiệp vụ lấy danh sách Cán bộ dạng danh mục chọn (Combobox / Dropdown)
 * Hỗ trợ lưu bộ nhớ đệm Cache và lọc theo Khoa / Phòng ban nếu cần.
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.Staffs.StaffQueries;

/// <summary>
/// Query lấy danh sách ngắn gọn các Cán bộ phục vụ việc chọn nhanh trên giao diện (Dropdown/Select)
/// </summary>
public class GetComboxStaffQuery : IRequest<List<StaffSelectModel>>
{
    /// <summary>
    /// ID Khoa / Phòng ban (tùy chọn lọc theo phòng ban)
    /// </summary>
    public int? DepartmentId { get; set; }

    public GetComboxStaffQuery(int? departmentId = null)
    {
        DepartmentId = departmentId;
    }

    public class Handler : IRequestHandler<GetComboxStaffQuery, List<StaffSelectModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;    // DbContext Đọc để tối ưu hiệu năng
        private readonly ICacheService _cacheService;            // Dịch vụ Cache

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }

        public async Task<List<StaffSelectModel>> Handle(GetComboxStaffQuery request, CancellationToken cancellationToken)
        {
            // Xây dựng cache key theo phòng ban hoặc key mặc định
            string cacheKey = request.DepartmentId.HasValue && request.DepartmentId.Value > 0
                ? StaffConstant.BuildCacheKey($"department-{request.DepartmentId.Value}")
                : StaffConstant.BuildCacheKey();

            // Đọc từ Cache trước, nếu chưa có thì truy vấn từ CSDL
            var result = await _cacheService.GetOrCreate<List<StaffSelectModel>>(cacheKey, async () =>
            {
                var query = _dataContext.VcStaffs.AsNoTracking();

                // Chỉ lấy các cán bộ có tài khoản đang hoạt động
                query = query.Where(x => x.User == null || x.User.IsActive);

                // Lọc theo Khoa / Phòng ban nếu có yêu cầu
                if (request.DepartmentId.HasValue && request.DepartmentId.Value > 0)
                {
                    query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);
                }

                var list = await query
                    .OrderBy(x => x.FullName)
                    .Select(x => new StaffSelectModel
                    {
                        StaffId = x.StaffId,
                        StaffCode = x.EmployeeCode,
                        FullName = x.FullName,
                        Gender = x.Gender,
                        DepartmentId = x.DepartmentId,
                        DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                        PhoneNumber = x.PhoneNumber
                    })
                    .ToListAsync(cancellationToken);

                return list;
            });

            return result ?? new List<StaffSelectModel>();
        }
    }
}
