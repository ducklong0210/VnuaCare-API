using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;

public class VcCampaignRegistrations
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("registration_id", TypeName = "int")]
    public int RegistrationId { get; set; }
    
    [Column("campaign_id", TypeName = "int")]
    public int CampaignId { get; set; }
    
    
}