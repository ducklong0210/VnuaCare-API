/**
 * Nghiệp vụ thêm mới Cán bộ: Lưu hồ sơ và tự động tạo tài khoản đăng nhập
 */

using System.Text.Json;
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

namespace VnuaCare.Business.Business.Staffs.StaffCommands;

public class CreateStaffCommand : IRequest<Unit>
{
    public CreateStaffModel Model { get; set; }
    
    public CreateStaffCommand(CreateStaffModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<CreateStaffCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để ghi dữ liệu User và Staff
        private readonly ICacheService _cacheService;            // Dịch vụ xóa cache sau khi thêm mới
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện
        private readonly IBcryptPasswordHasher _passwordHasher;  // Dịch vụ băm mật khẩu BCrypt

        public Handler(
            VnuaCareDataContext dataContext, 
            ICacheService cacheService, 
            Func<IContextAccessor> contextAccessor,
            IBcryptPasswordHasher passwordHasher)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
            _passwordHasher = passwordHasher;
        }

        // Xử lý kiểm tra dữ liệu, tạo tài khoản đăng nhập và lưu hồ sơ Cán bộ
        public async Task<Unit> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu thêm cán bộ mới: " + JsonSerializer.Serialize(model));
           
            #region 1. Phân quyền người thực hiện
            var currentUserRole = _contextAccessor.Role;

            // Chấp nhận SUPER_ADMIN, hoặc HEALTH_ADMIN
            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new ArgumentException("Bạn không có quyền thực hiện chức năng này (Yêu cầu quyền Quản trị viên).");
            }
            #endregion

            #region Validation

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

            if (model.DepartmentId <= 0)
            {
                throw new ArgumentException("Vui lòng chọn Khoa / Phòng ban hợp lệ.");
            }

            // Kiểm tra xem Khoa/Phòng ban có tồn tại và đang hoạt động trong CSDL không
            var departmentExists = await _dataContext.VcDepartments
                .AnyAsync(x => x.DepartmentId == model.DepartmentId && x.IsActive, cancellationToken);
            if (!departmentExists)
            {
                throw new ArgumentException("Khoa / Phòng ban được chọn không tồn tại hoặc đã bị vô hiệu hóa.");
            }
            #endregion

            #region Kiểm tra trùng lặp thông tin định danh (Check Duplicate)
            //  Kiểm tra trùng Mã cán bộ trong bảng vc_staffs
            var checkCode = await _dataContext.VcStaffs
                .AnyAsync(x => x.EmployeeCode == model.StaffCode.Trim(), cancellationToken);
            if (checkCode)
            {
                throw new ArgumentException($"Mã cán bộ '{model.StaffCode}' đã tồn tại trên hệ thống.");
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

            // Kiểm tra trùng Số điện thoại trong bảng 
            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                var checkPhone = await _dataContext.VcStaffs
                    .AnyAsync(x => x.PhoneNumber == model.PhoneNumber.Trim(), cancellationToken);
                if (checkPhone)
                {
                    throw new ArgumentException($"Số điện thoại '{model.PhoneNumber}' đã được đăng ký bởi cán bộ khác.");
                }
            }
            #endregion

            #region Lưu tk vào CSDL
            // Tạo tài khoản đăng nhập 
            var userEntity = new VcUsers
            {
                Username = model.Username.Trim(),
                Email = model.Email.Trim(),
                Password = _passwordHasher.HashPassword(model.Password),
                Role = Role.STAFF.ToString(), //  Role là STAFF
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _dataContext.VcUsers.AddAsync(userEntity, cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken); 

            //Tạo hồ sơ cán bộ VcStaffs gắn liền với UserId vừa sinh
            var staffEntity = new VcStaffs
            {
                UserId = userEntity.UserId,
                DepartmentId = model.DepartmentId,
                EmployeeCode = model.StaffCode.Trim(),
                FullName = model.FullName.Trim(),
                Gender = string.IsNullOrWhiteSpace(model.Gender) ? "NAM" : model.Gender.Trim().ToUpper(),
                PhoneNumber = model.PhoneNumber?.Trim(),
                DateOfBirth = model.DateOfBirth ?? DateTime.Now,
                AcademicTitle = model.AcademicTitle?.Trim(),
                JobTitle = model.JobTitle?.Trim(),
                Address = string.IsNullOrWhiteSpace(model.Adrress) ? "" : model.Adrress.Trim(),
                AvatarUrl = string.IsNullOrWhiteSpace(model.AvatarUrl) ? "" : model.AvatarUrl.Trim(),
            };

            await _dataContext.VcStaffs.AddAsync(staffEntity, cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region 5. Xóa Cache danh sách cán bộ để cập nhật dữ liệu mới
            _cacheService.Remove(VnuaCareCacheConstant.STAFF);
            _cacheService.Remove(VnuaCareCacheConstant.LIST_SELECT);
            #endregion

            Log.Information($"Thêm cán bộ thành công: {staffEntity.FullName} ({staffEntity.EmployeeCode}) - UserId: {userEntity.UserId}");

            return Unit.Value;
        }
    }
}
