/**
 * Nghiệp vụ lấy danh sách Bác sĩ dạng danh mục chọn (Combobox / Dropdown)
 * Hỗ trợ lưu bộ nhớ đệm Cache và lọc theo Chuyên khoa y tế nếu cần.
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.Doctors.DoctorQueries;

/// <summary>
/// Query lấy danh sách ngắn gọn các Bác sĩ phục vụ việc chọn nhanh trên giao diện (Dropdown/Select)
/// </summary>
public class GetComboxDocTorQuery : IRequest<List<DoctorSelectModel>>
{
    /// <summary>
    /// ID Chuyên khoa y tế (tùy chọn lọc theo chuyên khoa)
    /// </summary>
    public int? SpecialtyId { get; set; }

    public GetComboxDocTorQuery(int? specialtyId = null)
    {
        SpecialtyId = specialtyId;
    }

    public class Handler : IRequestHandler<GetComboxDocTorQuery, List<DoctorSelectModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;    // DbContext Đọc để tối ưu hiệu năng
        private readonly ICacheService _cacheService;            // Dịch vụ Cache

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }

        public async Task<List<DoctorSelectModel>> Handle(GetComboxDocTorQuery request, CancellationToken cancellationToken)
        {
            // Xây dựng cache key theo chuyên khoa hoặc key mặc định
            string cacheKey = request.SpecialtyId.HasValue && request.SpecialtyId.Value > 0
                ? DoctorConstant.BuildCacheKey($"specialty-{request.SpecialtyId.Value}")
                : DoctorConstant.BuildCacheKey();

            // Đọc từ Cache trước, nếu chưa có thì truy vấn từ CSDL
            var result = await _cacheService.GetOrCreate<List<DoctorSelectModel>>(cacheKey, async () =>
            {
                var query = _dataContext.VcDoctors.AsNoTracking();

                // Chỉ lấy các bác sĩ có tài khoản đang hoạt động
                query = query.Where(x => x.User == null || x.User.IsActive);

                // Lọc theo chuyên khoa nếu có yêu cầu
                if (request.SpecialtyId.HasValue && request.SpecialtyId.Value > 0)
                {
                    query = query.Where(x => x.SpecialtyId == request.SpecialtyId.Value);
                }

                var list = await query
                    .OrderBy(x => x.FullName)
                    .Select(x => new DoctorSelectModel
                    {
                        DoctorId = x.DoctorId,
                        DoctorCode = x.DoctorCode,
                        FullName = x.FullName,
                        SpecialtyId = x.SpecialtyId,
                        SpecialtyName = x.Specialty != null ? x.Specialty.SpecialtyName : null,
                        HospitalName = x.HospitalName
                    })
                    .ToListAsync(cancellationToken);

                return list;
            });

            return result ?? new List<DoctorSelectModel>();
        }
    }
}
