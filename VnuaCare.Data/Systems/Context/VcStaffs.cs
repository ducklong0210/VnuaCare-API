using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VnuaCare.Shared.Model;

namespace VnuaCare.Data.Systems.Context;

public class VcStaffs : EntityBase.BaseEntity
{
    public VcStaffs()
    {
    }

    /// <summary>
    /// Khóa chính tự tăng của hồ sơ Cán bộ
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("staff_id", TypeName = "int")]
    public int StaffId { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết 1-1 với tài khoản đăng nhập
    /// </summary>
    [Column("user_id", TypeName = "int")]
    public int? UserId { get; set; }
    /// <summary>
    /// Mã định danh bác sĩ (Duy nhất - ví dụ: BS001, BS_MAT_01)
    /// </summary>
    [Required]
    [Column("staff_code", TypeName = "varchar(50)")]
    public string StaffCode { get; set; }
    /// <summary>
    /// Khóa ngoại thuộc Khoa / Phòng ban nào
    /// </summary>
    [Required]
    [Column("department_id", TypeName = "int")]
    public int DepartmentId { get; set; }

    /// <summary>
    /// Mã định danh cán bộ (Duy nhất - ví dụ: CB001, GV1023)
    /// </summary>
    [Required]
    [Column("employee_code", TypeName = "varchar(50)")]
    public string EmployeeCode { get; set; }

    /// <summary>
    /// Họ và tên đầy đủ của cán bộ
    /// </summary>
    [Required]
    [Column("full_name", TypeName = "nvarchar(150)")]
    public string FullName { get; set; }

    /// <summary>
    /// Giới tính (NAM / NU)
    /// </summary>
    [Required]
    [Column("gender", TypeName = "varchar(10)")]
    public string Gender { get; set; }

    /// <summary>
    /// Ngày tháng năm sinh
    /// </summary>
    [Column("date_of_birth", TypeName = "date")]
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Học hàm / Học vị (GS, PGS.TS, TS, ThS)
    /// </summary>
    [Column("academic_title", TypeName = "nvarchar(50)")]
    public string? AcademicTitle { get; set; }

    /// <summary>
    /// Chức vụ / Vị trí công tác (Giảng viên chính, Trưởng bộ môn...)
    /// </summary>
    [Column("job_title", TypeName = "nvarchar(100)")]
    public string? JobTitle { get; set; }

    /// <summary>
    /// Số điện thoại liên hệ
    /// </summary>
    [Column("phone_number", TypeName = "varchar(20)")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Địa chỉ cư trú
    /// </summary>
    [Column("address", TypeName = "nvarchar(255)")]
    public string? Address { get; set; }

   
}