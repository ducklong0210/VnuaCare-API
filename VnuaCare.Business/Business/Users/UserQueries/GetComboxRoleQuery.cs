/**
 * Nghiệp vụ lấy danh mục Vai trò phân quyền người dùng dạng danh mục chọn (Combobox / Dropdown)
 */

using MediatR;
using VnuaCare.Business.Business.Services;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.Users.UserQueries;

/// <summary>
/// Query lấy danh sách các vai trò phân quyền người dùng trong hệ thống (Roles)
/// </summary>
public class GetComboxRoleQuery : IRequest<List<RoleSelectModel>>
{
    public class Handler : IRequestHandler<GetComboxRoleQuery, List<RoleSelectModel>>
    {
        private readonly ICacheService _cacheService;

        public Handler(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<List<RoleSelectModel>> Handle(GetComboxRoleQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"roles-{VnuaCareCacheConstant.LIST_SELECT}";

            var result = await _cacheService.GetOrCreate<List<RoleSelectModel>>(cacheKey, () =>
            {
                var roles = new List<RoleSelectModel>
                {
                    new RoleSelectModel
                    {
                        RoleCode = Role.SUPER_ADMIN.ToString(),
                        RoleName = "Quản trị viên cấp cao",
                        Description = "Toàn quyền quản trị hệ thống V-Care Health"
                    },
                    new RoleSelectModel
                    {
                        RoleCode = Role.HEALTH_ADMIN.ToString(),
                        RoleName = "Quản trị viên y tế",
                        Description = "Cán bộ phụ trách Trạm Y tế Học viện Nông nghiệp"
                    },
                    new RoleSelectModel
                    {
                        RoleCode = Role.DOCTOR.ToString(),
                        RoleName = "Bác sĩ",
                        Description = "Bác sĩ phụ trách khám thuộc BVĐK MEDLATEC hoặc Trạm Y tế"
                    },
                    new RoleSelectModel
                    {
                        RoleCode = Role.STAFF.ToString(),
                        RoleName = "Cán bộ / Giảng viên",
                        Description = "Cán bộ, giảng viên, nhân viên Học viện Nông nghiệp Việt Nam"
                    }
                };

                return Task.FromResult(roles);
            });

            return result ?? new List<RoleSelectModel>();
        }
    }
}
