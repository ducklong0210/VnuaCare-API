/**
 * Thực thể ánh xạ bảng hồ sơ bác sĩ BVĐK MEDLATEC (vc_doctors)
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;

/// <summary>
/// Bảng hồ sơ chuyên môn của Bác sĩ tham gia khám sức khỏe
/// </summary>
[Table("vc_doctors")]
public class VcDoctors
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("doctor_id", TypeName = "int")]
    public int DoctorId { get; set; } // Khóa chính: ID bác sĩ

    [Column("user_id", TypeName = "int")]
    public int UserId { get; set; } // ID tài khoản người dùng đăng nhập

    [Required]
    [Column("doctor_code", TypeName = "varchar(50)")]
    public string DoctorCode { get; set; } = string.Empty; 

    [Required]
    [Column("full_name", TypeName = "nvarchar(150)")]
    public string FullName { get; set; } = string.Empty; 

    [Required]
    [Column("specialty_id", TypeName = "int")]
    public int SpecialtyId { get; set; } // ID chuyên khoa phụ trách

    [Column("license_number", TypeName = "varchar(50)")]
    public string? LicenseNumber { get; set; } // Số chứng chỉ hành nghề khám chữa bệnh

    [Column("hospital_name", TypeName = "nvarchar(255)")]
    public string HospitalName { get; set; } = "Bệnh viện Đa khoa MEDLATEC"; // Tên bệnh viện đối tác

    [ForeignKey("UserId")]
    public virtual VcUsers? User { get; set; } // Tài khoản người dùng

    [ForeignKey("SpecialtyId")]
    public virtual VcSpecialties? Specialty { get; set; } // Chuyên khoa phụ trách
}
