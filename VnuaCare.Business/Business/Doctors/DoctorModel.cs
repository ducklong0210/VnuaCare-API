/**
 * Định nghĩa cấu trúc dữ liệu và DTO cho Bác sĩ (DoctorModel, CreateDoctorModel, DoctorFilterModel)
 */

using System.ComponentModel.DataAnnotations;
using VnuaCare.Data.Systems.Context;

namespace VnuaCare.Business.Business.Doctors;

public abstract record DoctorBaseModel
{
    public int DoctorId { get; init; }
    
    [Required(ErrorMessage = "Mã bác sĩ không được để trống")]
    public string DoctorCode { get; init; }
    
    [Required(ErrorMessage = "Họ và tên bác sĩ không được để trống")]
    public string FullName { get; init; }
    
    public string? Email { get; init; }
    public bool IsActive { get; init; } = true;
    public int SpecialtyId { get; init; }
    public string? SpesialtyName { get; init; }
    public string? LicenseNumber { get; init; }
    public string HospitalName { get; init; }
}

/// <summary>
/// Dùng để hiển thị chi tiết thông tin Bác sĩ
/// </summary>
public record DoctorModel : DoctorBaseModel
{
    public int UserId { get; init; }
}

public record CreateDoctorModel : DoctorBaseModel
{
    
    [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
    public string Username { get; init; }
    
    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    public string Password { get; init; } 
}

public record DoctorFilterModel : BaseQueryFilterModel
{
    public int? Specialty { get; set; }
}

public record UpdateDoctorModel : DoctorBaseModel
{
    public int? ModifiedByDoctorId { get; init; }
    public void UpdateEntity(VcDoctors vcDoctor){
        vcDoctor.FullName =  FullName;
        vcDoctor.DoctorCode = DoctorCode;
        vcDoctor.HospitalName = HospitalName;
        vcDoctor.SpecialtyId = SpecialtyId;
        vcDoctor.LicenseNumber = LicenseNumber;
    }
}

public record DeleteDoctorModel : DoctorBaseModel
{
    public int UserId { get; init; }

    public  DeleteDoctorModel(int id)
    {
        UserId = id;
    }
}