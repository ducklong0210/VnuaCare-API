/**
 * Nghiệp vụ lấy danh sách Chuyên khoa y tế dạng danh mục chọn (Combobox / Dropdown)
 * Hỗ trợ lưu bộ nhớ đệm Cache.
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.Doctors.DoctorQueries;

/// <summary>
/// Query lấy danh sách Chuyên khoa y tế phục vụ hiển thị combobox / dropdown trên giao diện
/// </summary>
public class GetComboxSpecialtyQuery : IRequest<List<SpecialtySelectModel>>
{
    public class Handler : IRequestHandler<GetComboxSpecialtyQuery, List<SpecialtySelectModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly ICacheService _cacheService;

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }

        public async Task<List<SpecialtySelectModel>> Handle(GetComboxSpecialtyQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"specialties-{VnuaCareCacheConstant.LIST_SELECT}";

            var result = await _cacheService.GetOrCreate<List<SpecialtySelectModel>>(cacheKey, async () =>
            {
                var list = await _dataContext.VcSpecialties.AsNoTracking()
                    .OrderBy(x => x.SpecialtyName)
                    .Select(x => new SpecialtySelectModel
                    {
                        SpecialtyId = x.SpecialtyId,
                        SpecialtyName = x.SpecialtyName
                    })
                    .ToListAsync(cancellationToken);

                return list;
            });

            return result ?? new List<SpecialtySelectModel>();
        }
    }
}
