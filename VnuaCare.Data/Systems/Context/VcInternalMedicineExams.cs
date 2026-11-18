using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;

/// <summary>
/// Bảng kết quả khám lâm sàng Nội khoa tổng quát (vc_internal_medicine_exams)
/// </summary>
[Table("vc_internal_medicine_exams")]
public class VcInternalMedicineExams
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("internal_exam_id", TypeName = "int")]
    public int InternalExamId { get; set; } // Khóa chính: ID kết quả khám nội khoa

    [Required]
    [Column("record_id", TypeName = "int")]
    public int RecordId { get; set; } // Khóa ngoại: ID hồ sơ khám sức khỏe liên kết

    [Column("circulatory_desc", TypeName = "nvarchar(max)")]
    public string? CirculatoryDesc { get; set; } // Khám hệ tuần hoàn / Tim mạch (tiếng tim T1 T2, nhịp xoang...)

    [Column("respiratory_desc", TypeName = "nvarchar(max)")]
    public string? RespiratoryDesc { get; set; } // Khám hệ hô hấp / Phổi (thông khí phổi, tiếng rale...)

    [Column("digestive_desc", TypeName = "nvarchar(max)")]
    public string? DigestiveDesc { get; set; } // Khám hệ tiêu hóa / Ổ bụng (bụng mềm, gan lách...)

    [Column("urinary_desc", TypeName = "nvarchar(max)")]
    public string? UrinaryDesc { get; set; } // Khám hệ thận - tiết niệu (hố thắt lưng, phản hồi thận...)

    [Column("musculoskeletal_desc", TypeName = "nvarchar(max)")]
    public string? MusculoskeletalDesc { get; set; } // Khám hệ cơ xương khớp (khớp, cột sống, vận động...)

    [Column("neurological_desc", TypeName = "nvarchar(max)")]
    public string? NeurologicalDesc { get; set; } // Khám hệ thần kinh - tâm thần (tri giác, phản xạ gân xương...)

    [Column("conclusion", TypeName = "nvarchar(max)")]
    public string? Conclusion { get; set; } // Kết luận khám lâm sàng nội khoa

    [Column("health_class", TypeName = "nvarchar(10)")]
    public string? HealthClass { get; set; } // Phân loại sức khỏe nội khoa (LOẠI I, LOẠI II...)

    [Column("examining_doctor_id", TypeName = "int")]
    public int? ExaminingDoctorId { get; set; } // ID bác sĩ trực tiếp khám nội khoa

    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; } // Thời điểm thực hiện khám nội khoa

    [ForeignKey("RecordId")]
    public virtual VcHealthCheckupRecords? HealthCheckupRecord { get; set; } // Điều hướng: Hồ sơ khám sức khỏe

    [ForeignKey("ExaminingDoctorId")]
    public virtual VcDoctors? ExaminingDoctor { get; set; } // Điều hướng: Bác sĩ khám nội khoa
}
