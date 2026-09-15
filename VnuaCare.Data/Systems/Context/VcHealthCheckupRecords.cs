using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;

/// <summary>
/// Bảng hồ sơ khám sức khỏe tổng hợp của cán bộ (vc_health_checkup_records)
/// </summary>
[Table("vc_health_checkup_records")]
public class VcHealthCheckupRecords
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("record_id", TypeName = "int")]
    public int RecordId { get; set; } // Khóa chính: ID hồ sơ khám sức khỏe
    
    [Required]
    [Column("campaign_id", TypeName = "int")]
    public int CampaignId { get; set; } // ID đợt khám sức khỏe tham gia
    
    [Required]
    [Column("staff_id", TypeName = "int")]
    public int StaffId { get; set; } // ID cán bộ được khám
    
    [Column("sid_code", TypeName = "varchar(50)")]
    public string SidCode { get; set; } // Mã vạch hồ sơ / Mã tiếp nhận SID của 
    
    [Column("actual_checkup_date", TypeName = "date")]
    public DateTime ActualCheckupDate { get; set; } // Ngày cán bộ thực tế đến khám
    
    [Column("medical_history_personal", TypeName = "nvarchar(max)")]
    public string? MedicalHistoryPersonal { get; set; } // Tiền sử bệnh bản thân 
    
    [Column("medical_history_family", TypeName = "nvarchar(max)")]
    public string? MedicalHistoryFamily { get; set; } // Tiền sử bệnh gia đình 
    
    [Column("health_classification", TypeName = "nvarchar(10)")]
    public string? HealthClassification { get; set; } // Xếp loại sức khỏe tổng thể 
    
    [Column("general_conclusion", TypeName = "nvarchar(max)")]
    public string? GeneralConclusion { get; set; } // Kết luận sức khỏe tổng quát của hội đồng bác sĩ
    
    [Column("recommendations", TypeName = "nvarchar(max)")]
    public string? Recommendations { get; set; } // Lời dặn dò, chế độ ăn uống, theo dõi và hướng điều trị
    
    [Column("concluded_by_doctor_id", TypeName = "int")]
    public int? ConcludedByDoctorId { get; set; } // ID bác sĩ chủ trì ký duyệt kết luận
    
    [Column("concluded_at", TypeName = "datetime2")]
    public DateTime ConcludedAt { get; set; } // Thời điểm bác sĩ ký duyệt kết luận hồ sơ
    
    [Column("status", TypeName = "varchar(20)")]
    public string Status { get; set; } // Trạng thái hồ sơ 
    
    [Column("report_pdf_url", TypeName = "varchar(500)")]
    public string? ReportPdfUrl { get; set; } // Đường dẫn lưu file PDF sổ khám sức khỏe tổng hợp
    
    [ForeignKey("CampaignId")]
    public virtual VcCheckupCampaigns? Campaign { get; set; } // Thông tin chi tiết đợt khám
    
    [ForeignKey("StaffId")]
    public virtual VcStaffs? Staff { get; set; } // Thông tin chi tiết cán bộ
    
    [ForeignKey("ConcludedByDoctorId")]
    public virtual VcDoctors? Doctor { get; set; } //Thông tin bác sĩ ký kết luận
}
