using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;

/// <summary>
/// Bảng quản lý các đợt khám sức khỏe định kỳ (vc_checkup_campaigns)
/// </summary>
[Table("vc_checkup_campaigns")] 
public class VcCheckupCampaigns
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("campaign_id", TypeName = "int")]
    public int CampaignId { get; set; }
    
    [Column("campaign_name", TypeName = "nvarchar(255)")]
    public string CampaignName { get; set; }
    
    [Column("campaign_code", TypeName = "varchar(50)")]
    public string CampaignCode { get; set; }
    
    [Column("year", TypeName = "int")]
    public int Year { get; set; }
    
    [Column("start_date", TypeName = "datetime")]
    public DateTime StartDate { get; set; }
    
    [Column("end_date", TypeName = "datetime")]
    public DateTime EndDate { get; set; }
    
    [Column("hospital_partner", TypeName = "nvarchar(255)")]
    public string HospitalPartner { get; set; }
    
    [Column("status", TypeName = "varchar(20)")]
    public string Status { get; set; }
    
}