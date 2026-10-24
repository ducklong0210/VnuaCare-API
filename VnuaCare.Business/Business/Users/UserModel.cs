/**
 * Định nghĩa DTO Tài khoản người dùng (UserModel, UserBaseModel, UpdatePasswordUserModel)
 */

using System.ComponentModel.DataAnnotations;
using VnuaCare.Business.Business.Doctors;
using VnuaCare.Business.Business.Staffs;
using VnuaCare.Business;
using VnuaCare.Data.Systems.Context;


namespace VnuaCare.Business.Business.Users;

public abstract record UserBaseModel
{
    public int UserId { get; init; }

    [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
    public string Username { get; init; }

    [Required(ErrorMessage = "Email không được để trống")]
    public string? Email { get; init; }

    [Required(ErrorMessage = "Role không được để trống")]
    public string Role { get; init; }

    public bool IsActive { get; init; } = true;

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record UserModel : UserBaseModel
{
    // public string? DisplayName { get; init; }
    // Hồ sơ Cán bộ (nếu là STAFF)
    public StaffModel? StaffProfile { get; set; }
    // Hồ sơ Bác sĩ (nếu là DOCTOR)
    public DoctorModel? DoctorProfile { get; set; }
}

public record UpdatePasswordUserModel
{
    public int? ModifiedUserId {get; init;}
    
    public string OldPassword {get; init;}
    
    public string NewPassword {get; init;}
    
    public string ConfirmPassword {get; init;}

    public void UpdatePassword(VcUsers entity)
    {
        entity.Password = this.NewPassword;
    }
}

/// <summary>
/// DTO nhận vào khi người dùng tự cập nhật thông tin hồ sơ cá nhân
/// </summary>
public record UpdateMyProfileModel
{
    [Required(ErrorMessage = "Họ và tên không được để trống")]
    public string FullName { get; init; }

    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string? Email { get; init; }

    public string? PhoneNumber { get; init; }

    public string? Address { get; init; }

    public string? AvatarUrl { get; init; }

    public string? Gender { get; init; }

    public DateTime? DateOfBirth { get; init; }
}

/// <summary>
/// DTO rút gọn cho danh mục chọn tài khoản người dùng (Combobox / Dropdown)
/// </summary>
public record UserSelectModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? FullName { get; set; }
}

/// <summary>
/// DTO cho danh mục chọn vai trò người dùng (Combobox / Dropdown)
/// </summary>
public record RoleSelectModel
{
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// DTO tạo mới tài khoản người dùng
/// </summary>
public record CreateUserModel
{
    [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
    public string Username { get; init; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vai trò không được để trống")]
    public string Role { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
}

/// <summary>
/// DTO cập nhật tài khoản người dùng
/// </summary>
public record UpdateUserModel
{
    [Required(ErrorMessage = "ID người dùng không được để trống")]
    public int UserId { get; init; }

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vai trò không được để trống")]
    public string Role { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;

    public void UpdateEntity(VcUsers entity)
    {
        entity.Email = this.Email.Trim();
        entity.Role = this.Role.Trim().ToUpper();
        entity.IsActive = this.IsActive;
        entity.UpdatedAt = DateTime.Now;
    }
}

/// <summary>
/// DTO lọc và phân trang danh sách tài khoản
/// </summary>
public record UserFilterModel : BaseQueryFilterModel
{
    public string? Role { get; set; }
}

/// <summary>
/// DTO đặt lại mật khẩu người dùng do Quản trị viên thực hiện
/// </summary>
public record ResetPasswordAdminModel
{
    [Required(ErrorMessage = "ID người dùng không được để trống")]
    public int UserId { get; init; }

    [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
    public string NewPassword { get; init; } = string.Empty;
}


