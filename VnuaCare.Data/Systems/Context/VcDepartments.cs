using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;
[Table("vc_departments")]
public class VcDepartments
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("department_id", TypeName = "int")]
    public int DepartmentId { get; set; }
    
    [Required]
    [Column("department_name", TypeName = "nvarchar(255)")]
    public string DepartmentName { get; set; }
    
    [Required]
    [Column("department_code", TypeName = "varchar(50)")]
    public string DepartmentCode { get; set; }

    [Required]
    [Column("is_active", TypeName = "bit")]
    public bool IsActive { get; set; } = true;
}