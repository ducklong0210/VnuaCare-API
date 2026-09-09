using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
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
        private readonly VnuaCareDataContext _dataContext;
        private readonly IContextAccessor _contextAccessor;
        private readonly ICacheService _cacheService;

        public Handler(
            VnuaCareDataContext context, 
            IContextAccessor contextAccessor,
            ICacheService cacheService)
        {
            _dataContext = context;
            _contextAccessor = contextAccessor;
            _cacheService = cacheService;
        }

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
                            .FirstOrDefaultAsync(x => x.UserId == entity.UserId, cancellationToken);
                        if (staff != null)
                        {
                            entity.StaffProfile = new StaffProfile
                            {
                                StaffId = staff.StaffId,
                                Code = staff.EmployeeCode,
                                FullName = staff.FullName,
                                Gender = staff.Gender,
                                DateOfBirth = staff.DateOfBirth,
                                DepartmentId = staff.DepartmentId,
                                AcademicTitle = staff.AcademicTitle,
                                JobTitle = staff.JobTitle,
                                PhoneNumber = staff.PhoneNumber,
                                Adress = staff.Address
                            };
                        }
                        break;

                    case "DOCTOR":
                        var doctor = await _dataContext.VcDoctors.AsNoTracking()
                            .FirstOrDefaultAsync(x => x.UserId == entity.UserId, cancellationToken);
                        if (doctor != null)
                        {
                            entity.DoctorProfile = new DoctorProfile
                            {
                                DoctorId = doctor.DoctorId,
                                Code = doctor.DoctorCode,
                                FullName = doctor.FullName,
                                Specialty = doctor.Specialty,
                                LicenseNumber = doctor.LicenseNumber,
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
