using System.ComponentModel.DataAnnotations;

namespace VnuaCare.Business.Business.Staffs;

public abstract record StaffBaseModel
{
    public string Code { get; init; }
    public string FullName { get; init; }
    public string Gender { get; init; }
    public DateTime DateOfBirth { get; init; }
    public int DepartmentId { get; init; }
    public string? AcademicTitle { get; init; }
    public string? JobTitle { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Adress { get; init; }
}

/// <summary>
///  chi tiết Cán bộ 
/// </summary>
public record StaffModel : StaffBaseModel
{
    public int StaffId { get; init; }
    public int UserId { get; init; }
    public string? DepartmentName { get; init; }
    public string? Email { get; init; }
    public bool IsActive { get; init; } = true;
    // public List<>
}

/// <summary>
/// Model nhận từ Client khi Admin tạo mới một Cán bộ 
/// </summary>
public record CreateStaffModel :  StaffBaseModel
{
    [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
    public string Username { get; init; }
    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    public string Password { get; init; } 
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string? Email { get; init; }
    
    [Required(ErrorMessage = "Mã cán bộ không được để trống")]
    public string StaffCode { get; init; } 
    [Required(ErrorMessage = "Họ và tên không được để trống")]
    public string FullName { get; init; }
    public string Gender { get; init; } = "NAM";
    public DateTime? DateOfBirth { get; init; }
    [Required(ErrorMessage = "Vui lòng chọn Khoa/Phòng ban")]
    public int DepartmentId { get; init; } 
    public string? AcademicTitle { get; init; } 
    public string? JobTitle { get; init; }     
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
}