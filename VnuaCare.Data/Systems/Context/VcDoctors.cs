using VnuaCare.Shared.Model;

namespace VnuaCare.Data.Systems.Context;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("vc_doctors")]
public class VcDoctors : EntityBase.BaseEntity
{
    /// <summary>
    /// Khóa chính tự tăng của hồ sơ Bác sĩ
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("doctor_id", TypeName = "int")]
    public int DoctorId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết 1-1 với tài khoản đăng nhập
    /// </summary>
    [Column("user_id", TypeName = "int")]
    public int? UserId { get; set; }

    /// <summary>
    /// Mã định danh bác sĩ (Duy nhất - ví dụ: BS001, BS_MAT_01)
    /// </summary>
    [Required]
    [Column("doctor_code", TypeName = "varchar(50)")]
    public string DoctorCode { get; set; }

    /// <summary>
    /// Họ và tên bác sĩ
    /// </summary>
    [Required]
    [Column("full_name", TypeName = "nvarchar(150)")]
    public string FullName { get; set; }

    /// <summary>
    /// Chuyên khoa phụ trách (Nội khoa, Mắt, Tai Mũi Họng, CĐHA, Xét nghiệm...)
    /// </summary>
    [Required]
    [Column("specialty", TypeName = "nvarchar(100)")]
    public string Specialty { get; set; }

    /// <summary>
    /// Số chứng chỉ hành nghề y
    /// </summary>
    [Column("license_number", TypeName = "varchar(50)")]
    public string? LicenseNumber { get; set; }

    /// <summary>
    /// Bệnh viện công tác (Mặc định: 'Bệnh viện Đa khoa MEDLATEC')
    /// </summary>
    [Column("hospital_name", TypeName = "nvarchar(255)")]
    public string HospitalName { get; set; } = "Bệnh viện Đa khoa MEDLATEC";

}