using System.ComponentModel.DataAnnotations;
using VnuaCare.Data.Systems.Context;

namespace VnuaCare.Business.Business.Users;

public abstract record UserBaseModel
{
    public int UserId { get; init; }

    [Required(ErrorMessage = "user.username.required")]
    public string Username { get; init; }

    [Required(ErrorMessage = "user.email.required")]
    public string Email { get; init; }

    [Required(ErrorMessage = "user.role.required")]
    public string Role { get; init; }

    public bool IsActive { get; init; } = true;

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record UserModel : UserBaseModel
{
    // public string? DisplayName { get; init; }
    // Hồ sơ Cán bộ (nếu là STAFF)
    public StaffProfile? StaffProfile { get; set; }
    // Hồ sơ Bác sĩ (nếu là DOCTOR)
    public DoctorProfile? DoctorProfile { get; set; }
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

public class StaffProfile
{
    public int StaffId { get; init; }
    public string Code { get; init; }
    public string FullName { get; init; }
    public string Gender { get; init; }
    public DateTime DateOfBirth { get; init; }
    public int DepartmentId { get; init; }
    public string? DepartmentName { get; init; }
    public string? AcademicTitle { get; init; }
    public string? JobTitle { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Adress { get; init; }
}

public class DoctorProfile
{
    public int DoctorId { get; init; }
    public string Code { get; init; }
    public string FullName { get; init; }
    public string Specialty { get; init; }
    public string? LicenseNumber { get; init; }
    public string HospitalName { get; init; }
}
