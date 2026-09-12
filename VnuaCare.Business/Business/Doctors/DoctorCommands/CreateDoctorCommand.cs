/**
 * Nghiệp vụ thêm mới Bác sĩ: Lưu thông tin chuyên khoa và tạo tài khoản DOCTOR
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Business.Services;
using VnuaCare.Data.Systems.Context;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;
using VnuaCare.Shared.Utils;

namespace VnuaCare.Business.Business.Doctors.DoctorCommands;

public class CreateDoctorCommand : IRequest<Unit>
{
    public CreateDoctorModel Model { get; set; }

    public CreateDoctorCommand(CreateDoctorModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<CreateDoctorCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;
        private readonly ICacheService _cacheService;
        private readonly IContextAccessor _contextAccessor;
        private readonly IBcryptPasswordHasher _bcryptPasswordHasher;

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService,
            Func<IContextAccessor> contextAccessor, IBcryptPasswordHasher bcryptPasswordHasher)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
            _bcryptPasswordHasher = bcryptPasswordHasher;
        }

        public async Task<Unit> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Thêm bác sĩ mới");

            var currentUserRole = _contextAccessor.Role;

            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("role?");
            }

            #region  validation
            
            if (string.IsNullOrWhiteSpace(model.Username) || !ValidationUtils.IsUsername(model.Username))
            {
                throw new ArgumentException("Tên đăng nhập không hợp lệ (không chứa khoảng trắng và tối thiểu 3 ký tự).");
            }

            if (string.IsNullOrWhiteSpace(model.Password) || !ValidationUtils.IsPassword(model.Password))
            {
                throw new ArgumentException("Mật khẩu phải có tối thiểu 6 ký tự, bao gồm ít nhất 1 chữ cái và 1 chữ số.");
            }

            if (string.IsNullOrWhiteSpace(model.Email) || !ValidationUtils.IsEmail(model.Email))
            {
                throw new ArgumentException("Địa chỉ Email không đúng định dạng.");
            }

            if (model.SpecialtyId <= 0)
            {
                throw new ArgumentException("Vui long dien chuyen khoa");
            }

            var checkCode = await _dataContext.VcDoctors
                .AnyAsync(x => x.DoctorCode == model.DoctorCode.Trim(), cancellationToken);
            if (checkCode)
            {
                throw new ArgumentException($"Mã bac si '{model.DoctorCode}' đã tồn tại trên hệ thống.");
            }

            // Kiểm tra trùng Tên đăng nhập trong bảng 
            var checkUsername = await _dataContext.VcUsers
                .AnyAsync(x => x.Username == model.Username.Trim(), cancellationToken);
            if (checkUsername)
            {
                throw new ArgumentException($"Tên đăng nhập '{model.Username}' đã tồn tại trên hệ thống. Vui lòng chọn tên khác.");
            }

            // Kiểm tra trùng Email trong bảng
            var checkEmail = await _dataContext.VcUsers
                .AnyAsync(x => x.Email == model.Email.Trim(), cancellationToken);
            if (checkEmail)
            {
                throw new ArgumentException($"Email '{model.Email}' đã được sử dụng bởi một tài khoản khác trong hệ thống.");
            }

            var userEntity = new VcUsers
            {
                Username = model.Username,
                Email = model.Email,
                Password = _bcryptPasswordHasher.HashPassword(model.Password),
                IsActive = true,
                Role = Role.DOCTOR.ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            await _dataContext.VcUsers.AddAsync(userEntity,cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken);

            var doctorEntity = new VcDoctors
            {
                UserId =  userEntity.UserId,
                FullName = model.FullName,
                DoctorCode = model.DoctorCode,
                LicenseNumber = model.LicenseNumber,
                SpecialtyId = model.SpecialtyId,
                HospitalName = model.HospitalName,
            };
            
            await _dataContext.VcDoctors.AddAsync(doctorEntity,cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken);
            
            _cacheService.Remove(VnuaCareCacheConstant.DOCTOR);
            _cacheService.Remove(VnuaCareCacheConstant.LIST_SELECT);
            Log.Information("Them doctor thah cong");
            return Unit.Value;
            #endregion
        }
        
    }
}