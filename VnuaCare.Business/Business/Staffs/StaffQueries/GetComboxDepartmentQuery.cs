/**
 * Nghiệp vụ lấy danh sách Khoa / Phòng ban dạng danh mục chọn (Combobox / Dropdown)
 * Hỗ trợ lưu bộ nhớ đệm Cache và chỉ lấy các phòng ban đang hoạt động.
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.Staffs.StaffQueries;

/// <summary>
/// Query lấy danh sách Khoa / Phòng ban phục vụ hiển thị combobox / dropdown trên giao diện
/// </summary>
public class GetComboxDepartmentQuery : IRequest<List<DepartmentSelectModel>>
{
    public class Handler : IRequestHandler<GetComboxDepartmentQuery, List<DepartmentSelectModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly ICacheService _cacheService;

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }

        public async Task<List<DepartmentSelectModel>> Handle(GetComboxDepartmentQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"departments-{VnuaCareCacheConstant.LIST_SELECT}";

            var result = await _cacheService.GetOrCreate<List<DepartmentSelectModel>>(cacheKey, async () =>
            {
                var list = await _dataContext.VcDepartments.AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.DepartmentName)
                    .Select(x => new DepartmentSelectModel
                    {
                        DepartmentId = x.DepartmentId,
                        DepartmentName = x.DepartmentName,
                        DepartmentCode = x.DepartmentCode
                    })
                    .ToListAsync(cancellationToken);

                return list;
            });

            return result ?? new List<DepartmentSelectModel>();
        }
    }
}
