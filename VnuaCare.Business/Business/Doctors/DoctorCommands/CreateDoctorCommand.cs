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

/// <summary>
/// Lệnh thêm mới Bác sĩ và tự động tạo tài khoản đăng nhập
/// </summary>
public class CreateDoctorCommand : IRequest<Unit>
{
    public CreateDoctorModel Model { get; set; }

    public CreateDoctorCommand(CreateDoctorModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<CreateDoctorCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;                // Kết nối CSDL để lưu trữ User và Doctor
        private readonly ICacheService _cacheService;                    // Dịch vụ xóa Cache sau khi tạo mới
        private readonly IContextAccessor _contextAccessor;              // Dịch vụ trích xuất Role của người thực hiện
        private readonly IBcryptPasswordHasher _bcryptPasswordHasher;    // Dịch vụ băm mật khẩu bằng BCrypt

        public Handler(
            VnuaCareDataContext dataContext, 
            ICacheService cacheService,
            Func<IContextAccessor> contextAccessor, 
            IBcryptPasswordHasher bcryptPasswordHasher)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
            _bcryptPasswordHasher = bcryptPasswordHasher;
        }

        // Xử lý kiểm tra dữ liệu, tạo tài khoản User và lưu hồ sơ Bác sĩ vào CSDL
        public async Task<Unit> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu thêm bác sĩ mới: " + model.FullName);

            // Kiểm tra phân quyền người thực hiện
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("role?");
            }

            #region Kiểm tra tính hợp lệ (Validation)
            // Kiểm tra định dạng tên đăng nhập
            if (string.IsNullOrWhiteSpace(model.Username) || !ValidationUtils.IsUsername(model.Username))
            {
                throw new ArgumentException("Tên đăng nhập không hợp lệ (không chứa khoảng trắng và tối thiểu 3 ký tự).");
            }

            // Kiểm tra độ mạnh mật khẩu
            if (string.IsNullOrWhiteSpace(model.Password) || !ValidationUtils.IsPassword(model.Password))
            {
                throw new ArgumentException("Mật khẩu phải có tối thiểu 6 ký tự, bao gồm ít nhất 1 chữ cái và 1 chữ số.");
            }

            // Kiểm tra định dạng Email
            if (string.IsNullOrWhiteSpace(model.Email) || !ValidationUtils.IsEmail(model.Email))
            {
                throw new ArgumentException("Địa chỉ Email không đúng định dạng.");
            }

            // Kiểm tra Chuyên khoa
            if (model.SpecialtyId <= 0)
            {
                throw new ArgumentException("Vui long dien chuyen khoa");
            }

            // Kiểm tra trùng Mã bác sĩ trong CSDL
            var checkCode = await _dataContext.VcDoctors
                .AnyAsync(x => x.DoctorCode == model.DoctorCode.Trim(), cancellationToken);
            if (checkCode)
            {
                throw new ArgumentException($"Mã bac si '{model.DoctorCode}' đã tồn tại trên hệ thống.");
            }

            // Kiểm tra trùng Tên đăng nhập trong CSDL
            var checkUsername = await _dataContext.VcUsers
                .AnyAsync(x => x.Username == model.Username.Trim(), cancellationToken);
            if (checkUsername)
            {
                throw new ArgumentException($"Tên đăng nhập '{model.Username}' đã tồn tại trên hệ thống. Vui lòng chọn tên khác.");
            }

            // Kiểm tra trùng Email trong CSDL
            var checkEmail = await _dataContext.VcUsers
                .AnyAsync(x => x.Email == model.Email.Trim(), cancellationToken);
            if (checkEmail)
            {
                throw new ArgumentException($"Email '{model.Email}' đã được sử dụng bởi một tài khoản khác trong hệ thống.");
            }
            #endregion

            #region Lưu dữ liệu vào CSDL
            // Tạo tài khoản User trước để SQL Server cấp UserId
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
            await _dataContext.VcUsers.AddAsync(userEntity, cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken);

            // Tạo hồ sơ Bác sĩ gắn với UserId vừa sinh
            var doctorEntity = new VcDoctors
            {
                UserId = userEntity.UserId,
                FullName = model.FullName,
                DoctorCode = model.DoctorCode,
                LicenseNumber = model.LicenseNumber,
                SpecialtyId = model.SpecialtyId,
                HospitalName = model.HospitalName,
            };
            await _dataContext.VcDoctors.AddAsync(doctorEntity, cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            // Xóa Cache danh sách bác sĩ để cập nhật dữ liệu mới nhất
            _cacheService.Remove(VnuaCareCacheConstant.DOCTOR);
            _cacheService.Remove(VnuaCareCacheConstant.LIST_SELECT);
            
            Log.Information("Thêm bác sĩ thành công ID: " + doctorEntity.DoctorId);
            return Unit.Value;
        }
    }
}
