/**
 * Nghiệp vụ Lấy chi tiết thông tin Tài khoản người dùng theo ID (GetUserByIdIndex)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Doctors;
using VnuaCare.Business.Business.Services;
using VnuaCare.Business.Business.Staffs;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.Users.UserQueries;

public class GetUserByIdIndex : IRequest<UserModel>
{
    public int UserId { get; set; }

    public GetUserByIdIndex(int userId)
    {
        UserId = userId;
    }

    public class Handler : IRequestHandler<GetUserByIdIndex, UserModel>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly ICacheService _cacheService;
        private readonly IContextAccessor _contextAccessor;

        public Handler(
            VnuaCareReadDataContext dataContext,
            ICacheService cacheService,
            Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        public async Task<UserModel> Handle(GetUserByIdIndex request, CancellationToken cancellationToken)
        {
            var currentUserRole = _contextAccessor.Role;
            var currentUserId = _contextAccessor.UserId;

            // Người dùng chỉ được xem chính mình, trừ khi là Quản trị viên
            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserId != request.UserId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập thông tin tài khoản này.");
            }

            string cacheKey = UserConstant.BuildCacheKey(request.UserId.ToString());

            var result = await _cacheService.GetOrCreate<UserModel>(cacheKey, async () =>
            {
                var user = await _dataContext.VcUsers.AsNoTracking()
                    .Where(x => x.UserId == request.UserId)
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

                if (user == null)
                {
                    throw new ArgumentException($"Không tìm thấy tài khoản với ID: {request.UserId}");
                }

                if (user.Role == Role.STAFF.ToString())
                {
                    user.StaffProfile = await _dataContext.VcStaffs.AsNoTracking()
                        .Include(s => s.Department)
                        .Where(s => s.UserId == user.UserId)
                        .Select(s => new StaffModel
                        {
                            StaffId = s.StaffId,
                            UserId = s.UserId,
                            StaffCode = s.EmployeeCode,
                            FullName = s.FullName,
                            Gender = s.Gender,
                            DateOfBirth = s.DateOfBirth,
                            DepartmentId = s.DepartmentId,
                            DepartmentName = s.Department != null ? s.Department.DepartmentName : null,
                            JobTitle = s.JobTitle,
                            AcademicTitle = s.AcademicTitle,
                            PhoneNumber = s.PhoneNumber,
                            Adrress = s.Address,
                            AvatarUrl = s.AvatarUrl,
                            Email = user.Email
                        })
                        .FirstOrDefaultAsync(cancellationToken);
                }
                else if (user.Role == Role.DOCTOR.ToString())
                {
                    user.DoctorProfile = await _dataContext.VcDoctors.AsNoTracking()
                        .Include(d => d.Specialty)
                        .Where(d => d.UserId == user.UserId)
                        .Select(d => new DoctorModel
                        {
                            DoctorId = d.DoctorId,
                            UserId = d.UserId,
                            DoctorCode = d.DoctorCode,
                            FullName = d.FullName,
                            SpecialtyId = d.SpecialtyId,
                            SpesialtyName = d.Specialty != null ? d.Specialty.SpecialtyName : string.Empty,
                            LicenseNumber = d.LicenseNumber,
                            HospitalName = d.HospitalName,
                            Email = user.Email
                        })
                        .FirstOrDefaultAsync(cancellationToken);
                }

                return user;
            });

            return result!;
        }
    }
}
