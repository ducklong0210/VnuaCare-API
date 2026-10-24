/**
 * Nghiệp vụ lấy danh sách Tài khoản người dùng dạng danh mục chọn (Combobox / Dropdown)
 * Hỗ trợ lưu bộ nhớ đệm Cache và lọc theo Vai trò (Role), Trạng thái hoạt động nếu cần.
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.Users.UserQueries;

/// <summary>
/// Query lấy danh sách ngắn gọn các Tài khoản người dùng phục vụ dropdown/select trên giao diện
/// </summary>
public class GetComboxUserQuery : IRequest<List<UserSelectModel>>
{
    /// <summary>
    /// Vai trò người dùng cần lọc (SUPER_ADMIN, HEALTH_ADMIN, STAFF, DOCTOR) - Tùy chọn
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của tài khoản (Mặc định: true)
    /// </summary>
    public bool? IsActive { get; set; }

    public GetComboxUserQuery(string? role = null, bool? isActive = true)
    {
        Role = role;
        IsActive = isActive;
    }

    public class Handler : IRequestHandler<GetComboxUserQuery, List<UserSelectModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;    // DbContext Đọc để tối ưu hiệu năng
        private readonly ICacheService _cacheService;            // Dịch vụ Cache

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }

        public async Task<List<UserSelectModel>> Handle(GetComboxUserQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = UserConstant.BuildCacheKey(
                $"role-{(string.IsNullOrWhiteSpace(request.Role) ? "all" : request.Role.Trim())}-active-{(request.IsActive.HasValue ? request.IsActive.Value.ToString() : "all")}");

            var result = await _cacheService.GetOrCreate<List<UserSelectModel>>(cacheKey, async () =>
            {
                var query = _dataContext.VcUsers.AsNoTracking();

                if (request.IsActive.HasValue)
                {
                    query = query.Where(x => x.IsActive == request.IsActive.Value);
                }

                if (!string.IsNullOrWhiteSpace(request.Role))
                {
                    var roleFilter = request.Role.Trim();
                    query = query.Where(x => x.Role == roleFilter);
                }

                var userList = await query
                    .OrderBy(x => x.Username)
                    .Select(x => new UserSelectModel
                    {
                        UserId = x.UserId,
                        Username = x.Username,
                        Email = x.Email,
                        Role = x.Role,
                        IsActive = x.IsActive
                    })
                    .ToListAsync(cancellationToken);

                if (userList.Count > 0)
                {
                    var userIds = userList.Select(u => u.UserId).ToList();

                    // Lấy tên cán bộ nếu tài khoản thuộc nhân viên
                    var staffMap = await _dataContext.VcStaffs.AsNoTracking()
                        .Where(s => userIds.Contains(s.UserId))
                        .ToDictionaryAsync(s => s.UserId, s => s.FullName, cancellationToken);

                    // Lấy tên bác sĩ nếu tài khoản thuộc bác sĩ
                    var doctorMap = await _dataContext.VcDoctors.AsNoTracking()
                        .Where(d => userIds.Contains(d.UserId))
                        .ToDictionaryAsync(d => d.UserId, d => d.FullName, cancellationToken);

                    foreach (var u in userList)
                    {
                        if (staffMap.TryGetValue(u.UserId, out var staffName))
                        {
                            u.FullName = staffName;
                        }
                        else if (doctorMap.TryGetValue(u.UserId, out var doctorName))
                        {
                            u.FullName = doctorName;
                        }
                    }
                }

                return userList;
            });

            return result ?? new List<UserSelectModel>();
        }
    }
}
