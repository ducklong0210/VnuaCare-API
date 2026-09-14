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
    public int RecordId { get; set; }
    
    [Required]
    [Column("campaign_id", TypeName = "int")]
    public int CampaignId { get; set; }
    
    [Required]
    [Column("staff_id", TypeName = "int")]
    public int StaffId { get; set; }
    
    [Column("sid_code", TypeName = "varchar(50)")]
    public string SidCode { get; set; }
    
    [Column("actual_checkup_date", TypeName = "date")]
    public DateTime ActualCheckupDate { get; set; }
    
    [Column("medical_history_personal", TypeName = "nvarchar(max)")]
    public string? MedicalHistoryPersonal { get; set; }
    
    [Column("medical_history_family", TypeName = "nvarchar(max)")]
    public string? MedicalHistoryFamily { get; set; }
    
    [Column("health_classification", TypeName = "nvarchar(10)")]
    public string? HealthClassification { get; set; }
    
    [Column("general_conclusion", TypeName = "nvarchar(max)")]
    public string? GeneralConclusion { get; set; }
    
    [Column("recommendations", TypeName = "nvarchar(max)")]
    public string? Recommendations { get; set; }
    
    [Column("concluded_by_doctor_id", TypeName = "int")]
    public int? ConcludedByDoctorId { get; set; }
    
    [Column("concluded_at", TypeName = "datetime2")]
    public DateTime ConcludedAt { get; set; }
    
    [Column("status", TypeName = "varchar(20)")]
    public string Status { get; set; }
    
    [Column("report_pdf_url", TypeName = "varchar(500)")]
    public string? ReportPdfUrl { get; set; }
    
    [ForeignKey("CampaignId")]
    public virtual VcCheckupCampaigns? Campaign { get; set; }
    
    [ForeignKey("StaffId")]
    public virtual VcStaffs? Staff { get; set; }

    [ForeignKey("ConcludedByDoctorId")]
    public virtual VcDoctors? Doctor { get; set; }
}
