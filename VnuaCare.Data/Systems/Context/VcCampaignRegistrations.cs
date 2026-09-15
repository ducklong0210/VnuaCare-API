using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;

/// <summary>
/// Bảng danh sách cán bộ đăng ký tham gia đợt khám (vc_campaign_registrations)
/// </summary>
[Table("vc_campaign_registrations")]
public class VcCampaignRegistrations
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("registration_id", TypeName = "int")]
    public int RegistrationId { get; set; } // Khóa chính: ID lượt đăng ký khám
    
    [Column("campaign_id", TypeName = "int")]
    public int CampaignId { get; set; } // ID đợt khám sức khỏe tham gia
}
