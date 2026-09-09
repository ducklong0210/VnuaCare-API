using System.ComponentModel.DataAnnotations;

namespace VnuaCare.Business.Business.Doctors;

public abstract record DoctorBaseModel
{
    [Required(ErrorMessage = "Mã bác sĩ không được để trống")]
    public string Code { get; init; }
    
    [Required(ErrorMessage = "Họ và tên bác sĩ không được để trống")]
    public string FullName { get; init; }
    
    public string Specialty { get; init; }
    public string? LicenseNumber { get; init; }
    public string HospitalName { get; init; }
}

/// <summary>
/// Dùng để hiển thị chi tiết thông tin Bác sĩ
/// </summary>
public record DoctorModel : DoctorBaseModel
{
    public int UserId { get; init; }
    public int DoctorId { get; init; }
    public string? Email { get; init; }
    public bool IsActive { get; init; } = true;
    
}