/**
 * Nghiệp vụ lấy thông tin chi tiết Bác sĩ theo ID (kèm Cache)
 */

using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;

namespace VnuaCare.Business.Business.Doctors.DoctorQueries;

/// <summary>
/// Query lấy thông tin chi tiết Bác sĩ theo ID
/// </summary>
public class GetDoctorByIdIndex : IRequest<DoctorModel>
{
    public int Id { get; init; }
    
    public GetDoctorByIdIndex(int id)
    {
        Id = id;
    }

    public class Handler : IRequestHandler<GetDoctorByIdIndex, DoctorModel>
    {
        private readonly VnuaCareReadDataContext _dataContext;    // Kết nối CSDL đọc dữ liệu tối ưu hiệu năng
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất thông tin người dùng từ Token
        private readonly ICacheService _cacheService;            // Dịch vụ lưu và đọc dữ liệu từ Cache

        public Handler(
            VnuaCareReadDataContext dataContext, 
            IContextAccessor contextAccessor, 
            ICacheService cacheService)
        {
            _dataContext = dataContext;
            _contextAccessor = contextAccessor;
            _cacheService = cacheService;
        }

        // Xử lý lấy thông tin chi tiết bác sĩ kèm thông tin chuyên khoa qua Cache
        public async Task<DoctorModel> Handle(GetDoctorByIdIndex request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            string cacheKey = DoctorConstant.BuildCacheKey(id.ToString());

            // Đọc từ Cache nếu có, nếu chưa có thì truy vấn từ CSDL và lưu vào Cache
            var item = await _cacheService.GetOrCreate<DoctorModel>(cacheKey, async () =>
            {
                var entity = await _dataContext.VcDoctors.AsNoTracking()
                    .Include(x => x.Specialty)
                    .Where(x => x.DoctorId == id)
                    .Select(x => new DoctorModel
                    {
                        UserId = x.UserId,
                        DoctorId = x.DoctorId,
                        Email = x.User.Email,
                        DoctorCode = x.DoctorCode,
                        FullName = x.FullName,
                        SpecialtyId = x.SpecialtyId,
                        SpesialtyName = x.Specialty != null ? x.Specialty.SpecialtyName : "",
                        LicenseNumber = x.LicenseNumber,
                        HospitalName = x.HospitalName
                    }).FirstOrDefaultAsync(cancellationToken);
                return entity;
            });
            return item;
        }
    }
}
