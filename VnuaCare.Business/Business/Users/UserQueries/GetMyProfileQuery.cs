/**
 * Nghiệp vụ lấy thông tin cá nhân và hồ sơ Cán bộ/Bác sĩ của tài khoản đang đăng nhập
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Doctors;
using VnuaCare.Business.Business.Services;
using VnuaCare.Business.Business.Staffs;
using VnuaCare.Business.Business.Users;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;

namespace VnuaCare.Business.Business.Users.UserQueries;

/// <summary>
/// Query lấy thông tin hồ sơ của người dùng đang đăng nhập
/// </summary>
public class GetMyProfileQuery : IRequest<UserModel>
{
    public class Handler : IRequestHandler<GetMyProfileQuery, UserModel>
    {
        private readonly VnuaCareReadDataContext _dataContext; // Kết nối CSDL đọc dữ liệu tối ưu hiệu năng
        private readonly IContextAccessor _contextAccessor;    // Dịch vụ trích xuất UserId từ Token người đang đăng nhập
        private readonly ICacheService _cacheService;          // Dịch vụ lưu và đọc dữ liệu từ Cache Redis

        public Handler(
            VnuaCareReadDataContext context, 
            IContextAccessor contextAccessor,
            ICacheService cacheService)
        {
            _dataContext = context;
            _contextAccessor = contextAccessor;
            _cacheService = cacheService;
        }

        // Xử lý lấy thông tin tài khoản và nạp kèm hồ sơ Cán bộ hoặc Bác sĩ
        public async Task<UserModel> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
        {
            // Lấy ID người dùng từ Token
            var id = _contextAccessor.UserId;
            if (!id.HasValue)
            {
                throw new UnauthorizedAccessException("Người dùng chưa đăng nhập hoặc Token không hợp lệ.");
            }

            string cacheKey = UserConstant.BuildCacheKey(id.Value.ToString());

            var result = await _cacheService.GetOrCreate<UserModel>(cacheKey, async () =>
            {
                var entity = await _dataContext.VcUsers.AsNoTracking()
                    .Where(x => x.UserId == id.Value && x.IsActive)
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
                    .FirstOrDefaultAsync(cancellationToken);

                if (entity == null)
                {
                    throw new ArgumentException("Tài khoản không tồn tại hoặc đã bị khóa.");
                }

                switch (entity.Role)
                {
                    case "STAFF":
                        var staff = await _dataContext.VcStaffs.AsNoTracking()
                            .Include(x => x.Department) // thêm để tải dữ liệu sang
                            .FirstOrDefaultAsync(x => x.UserId == entity.UserId, cancellationToken);
                        if (staff != null)
                        {
                            entity.StaffProfile = new StaffModel
                            {
                                StaffId = staff.StaffId,
                                StaffCode = staff.EmployeeCode,
                                FullName = staff.FullName,
                                Gender = staff.Gender,
                                DateOfBirth = staff.DateOfBirth,
                                DepartmentId = staff.DepartmentId,
                                AcademicTitle = staff.AcademicTitle,
                                JobTitle = staff.JobTitle,
                                PhoneNumber = staff.PhoneNumber,
                                Adrress = staff.Address,
                                Email = entity.Email,
                                DepartmentName = staff.Department?.DepartmentName
                            };
                        }
                        break;

                    case "DOCTOR":
                        var doctor = await _dataContext.VcDoctors.AsNoTracking()
                            .FirstOrDefaultAsync(x => x.UserId == entity.UserId, cancellationToken);
                        if (doctor != null)
                        {
                            entity.DoctorProfile = new DoctorModel
                            {
                                DoctorId = doctor.DoctorId,
                                Code = doctor.DoctorCode,
                                FullName = doctor.FullName,
                                Specialty = doctor.Specialty,
                                LicenseNumber = doctor.LicenseNumber,
                                Email =  entity.Email,
                                HospitalName = doctor.HospitalName
                            };
                        }
                        break;
                }

                return entity;
            });

            return result;
        }
    }
}
