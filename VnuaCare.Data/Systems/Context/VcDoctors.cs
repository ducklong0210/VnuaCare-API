using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;

[Table("vc_doctors")]
public class VcDoctors
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("doctor_id", TypeName = "int")]
    public int DoctorId { get; set; }

    [Column("user_id", TypeName = "int")]
    public int? UserId { get; set; }

    [Required]
    [Column("doctor_code", TypeName = "varchar(50)")]
    public string DoctorCode { get; set; } = string.Empty;

    [Required]
    [Column("full_name", TypeName = "nvarchar(150)")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Column("specialty", TypeName = "nvarchar(100)")]
    public string Specialty { get; set; } = string.Empty;

    [Column("license_number", TypeName = "varchar(50)")]
    public string? LicenseNumber { get; set; }

    [Column("hospital_name", TypeName = "nvarchar(255)")]
    public string HospitalName { get; set; } = "Bệnh viện Đa khoa MEDLATEC";

    [ForeignKey("UserId")]
    public virtual VcUsers? User { get; set; }
}
