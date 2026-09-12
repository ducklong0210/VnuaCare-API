using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;

[Table("vc_specialties")]
public class VcSpecialties
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("specialty_id",TypeName = "int")]
    public int SpecialtyId { get; set; }
    
    [Column("specialty_description",TypeName = "nvarchar(255)")]
    public string SpecialtyDescription { get; set; }
    
    [Column("specialty_name",TypeName = "nvarchar(150)")]
    public string SpecialtyName { get; set; }
    
    [Column("is_active",TypeName = "bit")]
    public bool IsActive { get; set; }
  
}