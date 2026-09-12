using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;

[Table("vc_staffs")]
public class VcStaffs
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("staff_id", TypeName = "int")]
    public int StaffId { get; set; }

    [Column("user_id", TypeName = "int")]
    public int UserId { get; set; }

    [Required]
    [Column("department_id", TypeName = "int")]
    public int DepartmentId { get; set; }

    [Required]
    [Column("employee_code", TypeName = "varchar(50)")]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required]
    [Column("full_name", TypeName = "nvarchar(150)")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Column("gender", TypeName = "varchar(10)")]
    public string Gender { get; set; } = string.Empty;

    [Column("date_of_birth", TypeName = "date")]
    public DateTime DateOfBirth { get; set; }

    [Column("academic_title", TypeName = "nvarchar(50)")]
    public string? AcademicTitle { get; set; }

    [Column("job_title", TypeName = "nvarchar(100)")]
    public string? JobTitle { get; set; }

    [Column("phone_number", TypeName = "varchar(20)")]
    public string? PhoneNumber { get; set; }

    [Column("address", TypeName = "nvarchar(255)")]
    public string? Address { get; set; }
    [Column("avatar_url", TypeName = "varchar(255)")]
    public string? AvatarUrl { get; set; }
    
    [ForeignKey("UserId")]
    public virtual VcUsers? User { get; set; }
    
    [ForeignKey("DepartmentId")]
    public virtual VcDepartments? Department { get; set; }
}
