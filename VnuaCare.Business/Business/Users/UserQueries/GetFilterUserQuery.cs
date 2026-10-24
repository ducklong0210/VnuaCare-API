/**
 * Nghiệp vụ Lọc và phân trang danh sách Tài khoản người dùng (GetFilterUserQuery)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;
using VnuaCare.Shared.Utils;

namespace VnuaCare.Business.Business.Users.UserQueries;

public class GetFilterUserQuery : IRequest<PaginationList<UserModel>>
{
    public UserFilterModel Filter { get; set; }

    public GetFilterUserQuery(UserFilterModel filter)
    {
        Filter = filter;
    }

    public class Handler : IRequestHandler<GetFilterUserQuery, PaginationList<UserModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareReadDataContext dataContext, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _contextAccessor = contextAccessor();
        }

        public async Task<PaginationList<UserModel>> Handle(GetFilterUserQuery request, CancellationToken cancellationToken)
        {
            var filter = request.Filter;
            var currentUserRole = _contextAccessor.Role;

            if (currentUserRole != Role.SUPER_ADMIN.ToString() && currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập danh sách tài khoản người dùng.");
            }

            var query = _dataContext.VcUsers.AsNoTracking();

            // Lọc theo Role nếu có
            if (!string.IsNullOrWhiteSpace(filter.Role))
            {
                var roleTrim = filter.Role.Trim().ToUpper();
                query = query.Where(x => x.Role == roleTrim);
            }

            // Lọc theo trạng thái hoạt động
            if (filter.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == filter.IsActive.Value);
            }

            // Tìm kiếm tự do theo Username hoặc Email
            if (!string.IsNullOrWhiteSpace(filter.TextSearch))
            {
                var ts = filter.TextSearch.Trim().ToLower();
                query = query.Where(x => x.Username.ToLower().Contains(ts) || (x.Email != null && x.Email.ToLower().Contains(ts)));
            }

            // Sắp xếp
            bool isAsc = string.Equals(filter.Ascending, "asc", StringComparison.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(filter.PropertyName) && filter.PropertyName.Equals("Username", StringComparison.OrdinalIgnoreCase))
            {
                query = isAsc ? query.OrderBy(x => x.Username) : query.OrderByDescending(x => x.Username);
            }
            else if (!string.IsNullOrWhiteSpace(filter.PropertyName) && filter.PropertyName.Equals("Email", StringComparison.OrdinalIgnoreCase))
            {
                query = isAsc ? query.OrderBy(x => x.Email) : query.OrderByDescending(x => x.Email);
            }
            else
            {
                query = query.OrderByDescending(x => x.UserId);
            }

            // Phân trang
            int pageNumber = filter.PageNumber > 0 ? filter.PageNumber : 1;
            int pageSize = filter.PageSize > 0 ? filter.PageSize : 10;

            int totalCount = await query.CountAsync(cancellationToken);
            int skip = (pageNumber - 1) * pageSize;

            var items = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new UserModel
                {
                    UserId = x.UserId,
                    Username = x.Username,
                    Email = x.Email,
                    Role = x.Role,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return new PaginationList<UserModel>
            {
                Data = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                DataCount = items.Count
            };
        }
    }
}
