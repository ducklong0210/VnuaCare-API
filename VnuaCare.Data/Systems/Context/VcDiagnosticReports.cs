using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;

/// <summary>
/// Bảng kết quả chẩn đoán hình ảnh & thăm dò chức năng (Siêu âm, X-quang, Điện tim, Nội soi, CT, MRI...)
/// Bảng CSDL tương ứng: vc_diagnostic_reports
/// </summary>
[Table("vc_diagnostic_reports")]
public class VcDiagnosticReports
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("report_id", TypeName = "int")]
    public int ReportId { get; set; } // Khóa chính: ID phiếu kết quả chẩn đoán hình ảnh

    [Required]
    [Column("record_id", TypeName = "int")]
    public int RecordId { get; set; } // Khóa ngoại: ID hồ sơ khám sức khỏe liên kết

    [Required]
    [Column("modality_type", TypeName = "varchar(30)")]
    public string ModalityType { get; set; } = string.Empty; // Loại chẩn đoán hình ảnh: ULTRASOUND (Siêu âm), XRAY (X-quang), ECG (Điện tim), ENDOSCOPY (Nội soi), CT (Cắt lớp), MRI (Cộng hưởng từ)...

    [Required]
    [Column("exam_title", TypeName = "nvarchar(255)")]
    public string ExamTitle { get; set; } = string.Empty; // Tiêu đề kỹ thuật / chỉ định khám (ví dụ: Siêu âm ổ bụng tổng quát, Chụp X-quang tim phổi thẳng)

    [Column("technique_description", TypeName = "nvarchar(max)")]
    public string? TechniqueDescription { get; set; } // Mô tả kỹ thuật thực hiện / thiết bị máy móc

    [Column("findings_data", TypeName = "nvarchar(max)")]
    public string? FindingsData { get; set; } // Mô tả hình ảnh tổn thương / kết quả giải phẫu quan sát được

    [Required]
    [Column("conclusion", TypeName = "nvarchar(max)")]
    public string Conclusion { get; set; } = string.Empty; // Kết luận chẩn đoán hình ảnh

    [Column("image_urls", TypeName = "nvarchar(max)")]
    public string? ImageUrls { get; set; } // Danh sách đường dẫn ảnh kết quả / PACS / file đính kèm

    [Column("reading_doctor_id", TypeName = "int")]
    public int? ReadingDoctorId { get; set; } // ID bác sĩ thực hiện và đọc kết quả chẩn đoán

    [Column("approving_doctor_id", TypeName = "int")]
    public int? ApprovingDoctorId { get; set; } // ID bác sĩ trưởng khoa duyệt kết quả

    [Column("performed_at", TypeName = "datetime2")]
    public DateTime? PerformedAt { get; set; } = DateTime.Now; // Thời điểm thực hiện chẩn đoán hình ảnh

    [ForeignKey("RecordId")]
    public virtual VcHealthCheckupRecords? HealthCheckupRecord { get; set; } // Điều hướng: Hồ sơ khám sức khỏe liên kết

    [ForeignKey("ReadingDoctorId")]
    public virtual VcDoctors? ReadingDoctor { get; set; } // Điều hướng: Bác sĩ đọc kết quả

    [ForeignKey("ApprovingDoctorId")]
    public virtual VcDoctors? ApprovingDoctor { get; set; } // Điều hướng: Bác sĩ duyệt kết quả
}
