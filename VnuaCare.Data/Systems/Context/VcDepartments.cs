/**
 * Thực thể ánh xạ bảng danh mục Khoa / Phòng ban Học viện Nông nghiệp (vc_departments)
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;

/// <summary>
/// Bảng danh mục các Khoa, Viện, Phòng ban thuộc Học viện Nông nghiệp Việt Nam
/// </summary>
[Table("vc_departments")]
public class VcDepartments
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("department_id", TypeName = "int")]
    public int DepartmentId { get; set; } // Khóa chính: ID khoa / phòng ban
    
    [Required]
    [Column("department_name", TypeName = "nvarchar(255)")]
    public string DepartmentName { get; set; } // Tên khoa / phòng ban (VD: Khoa Công nghệ thông tin)
    
    [Required]
    [Column("department_code", TypeName = "varchar(50)")]
    public string DepartmentCode { get; set; } // Mã khoa / phòng ban (VD: FITA, AGRI, TCHC)

    [Required]
    [Column("is_active", TypeName = "bit")]
    public bool IsActive { get; set; } = true; // Trạng thái hoạt động (true: Hoạt động, false: Khóa)
}
