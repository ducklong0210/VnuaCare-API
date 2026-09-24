using System;
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
    public int CampaignId { get; set; } // Khóa chính: ID đợt khám sức khỏe
    
    [Column("campaign_name", TypeName = "nvarchar(255)")]
    public string CampaignName { get; set; } // Tên đợt khám 
    
    [Column("campaign_code", TypeName = "varchar(50)")]
    public string CampaignCode { get; set; } // Mã đợt khám 
    
    [Column("year", TypeName = "int")]
    public int Year { get; set; } // Năm tổ chức khám
    
    [Column("start_date", TypeName = "datetime")]
    public DateTime StartDate { get; set; } // Ngày bắt đầu tổ chức khám
    
    [Column("end_date", TypeName = "datetime")]
    public DateTime EndDate { get; set; } // Ngày kết thúc đợt khám
    
    [Column("hospital_partner", TypeName = "nvarchar(255)")]
    public string HospitalPartner { get; set; } // Đơn vị bệnh viện đối tác (Mặc định: BVĐK MEDLATEC)
    
    [Column("status", TypeName = "varchar(20)")]
    public string Status { get; set; } // Trạng thái đợt khám (PLANNING, IN_PROGRESS, COMPLETED)
}
