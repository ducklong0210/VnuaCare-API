/**
 * Nghiệp vụ lấy thông tin chi tiết Cán bộ theo ID (kèm Cache)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.Staffs.StaffQueries;

/// <summary>
/// Query lấy thông tin chi tiết Cán bộ theo ID
/// </summary>
public class GetStaffByIdIndex : IRequest<StaffModel>
{
    public int Id { get; set; }

    public GetStaffByIdIndex(int id)
    {
        Id = id;
    }

    public class Handler : IRequestHandler<GetStaffByIdIndex, StaffModel>
    {
        private readonly VnuaCareReadDataContext _dataContext;    // Kết nối CSDL đọc dữ liệu tối ưu hiệu năng
        private readonly ICacheService _cacheService;            // Dịch vụ lưu và đọc dữ liệu từ Cache

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }

        // Xử lý lấy thông tin cán bộ theo ID kèm thông tin phòng ban qua Cache
        public async Task<StaffModel> Handle(GetStaffByIdIndex request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            string cacheKey = StaffConstant.BuildCacheKey(id.ToString());

            // Đọc từ Cache nếu có, nếu chưa có thì truy vấn từ CSDL và lưu vào Cache
            var item = await _cacheService.GetOrCreate<StaffModel>(cacheKey, async () =>
            {
                var entity = await _dataContext.VcStaffs.AsNoTracking()
                    .Where(x => x.StaffId == id)
                    .Select(x => new StaffModel
                    {
                        StaffId = x.StaffId,
                        StaffCode = x.EmployeeCode,
                        FullName = x.FullName,
                        JobTitle = x.JobTitle,
                        PhoneNumber = x.PhoneNumber,
                        Gender = x.Gender,
                        DateOfBirth = x.DateOfBirth,
                        Email = x.User.Email,
                        Adrress = x.Address,
                        AvatarUrl = string.IsNullOrEmpty(x.AvatarUrl) ? "" : x.AvatarUrl,
                        DepartmentName = x.Department.DepartmentName,
                        AcademicTitle = x.AcademicTitle,
                    }).FirstOrDefaultAsync(cancellationToken);
                return entity;
            });
            return item;
        }
    }
}
