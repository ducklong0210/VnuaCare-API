/**
 * Định nghĩa cấu trúc dữ liệu và DTO cho Cán bộ (StaffModel, CreateStaffModel, StaffFilterModel)
 */

using System.ComponentModel.DataAnnotations;
using VnuaCare.Business;
using VnuaCare.Data.Systems.Context;

namespace VnuaCare.Business.Business.Staffs;

public abstract record StaffBaseModel
{
    public int StaffId { get; init; }
    
    [Required(ErrorMessage = "Mã cán bộ không được để trống")]
    public string StaffCode { get; init; } 
    
    [Required(ErrorMessage = "Họ và tên không được để trống")]
    public string FullName { get; init; }
    
    public string Gender { get; init; }
    
    public DateTime? DateOfBirth { get; init; }
    
    [Required(ErrorMessage = "Vui lòng chọn Khoa/Phòng ban")]
    public int DepartmentId { get; init; }
    public string? DepartmentName { get; init; }

    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string? Email { get; init; }
    public bool IsActive { get; init; } = true;
    
    public string? AcademicTitle { get; init; }
    public string? JobTitle { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Adrress { get; init; }
    public string? AvatarUrl { get; init; }
}

/// <summary>
///  chi tiết Cán bộ 
/// </summary>
public record StaffModel : StaffBaseModel
{
    public int UserId { get; init; }
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
    
}

public record StaffFilterModel : BaseQueryFilterModel
{
    public int? DepartmentId { get; set; }
}

public record UpdateStaffModel : StaffBaseModel
{
    public void UpdateEntity(VcStaffs vcStaffs)
    {
        vcStaffs.FullName = this.FullName;
        vcStaffs.Gender = this.Gender;
        vcStaffs.DateOfBirth = this.DateOfBirth ?? DateTime.Now;
        if (this.DepartmentId > 0)
        {
            vcStaffs.DepartmentId = this.DepartmentId;
        }
        vcStaffs.AcademicTitle = this.AcademicTitle;
        vcStaffs.JobTitle = this.JobTitle;
        vcStaffs.PhoneNumber = this.PhoneNumber;
        vcStaffs.Address = this.Adrress;
        vcStaffs.AvatarUrl = string.IsNullOrWhiteSpace(this.AvatarUrl) ? "" : this.AvatarUrl.Trim();
    }
}
public record DeleteStaffModel : StaffBaseModel
{
    public int UserId { get; init; }
    public DeleteStaffModel(int userId)
    {
        UserId = userId;
    }
}

/// <summary>
/// DTO danh mục Cán bộ phục vụ hiển thị combobox / dropdown
/// </summary>
public record StaffSelectModel
{
    public int StaffId { get; init; }
    public string StaffCode { get; init; }
    public string FullName { get; init; }
    public string? Gender { get; init; }
    public int DepartmentId { get; init; }
    public string? DepartmentName { get; init; }
    public string? PhoneNumber { get; init; }
}

/// <summary>
/// DTO danh mục Khoa / Phòng ban phục vụ hiển thị combobox / dropdown
/// </summary>
public record DepartmentSelectModel
{
    public int DepartmentId { get; init; }
    public string DepartmentName { get; init; }
    public string DepartmentCode { get; init; }
}