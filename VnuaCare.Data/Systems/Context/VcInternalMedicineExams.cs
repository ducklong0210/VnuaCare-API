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
    public int InternalExamId { get; set; }

    [Required]
    [Column("record_id", TypeName = "int")]
    public int RecordId { get; set; }

    [Column("circulatory_desc", TypeName = "nvarchar(max)")]
    public string? CirculatoryDesc { get; set; }

    [Column("respiratory_desc", TypeName = "nvarchar(max)")]
    public string? RespiratoryDesc { get; set; }

    [Column("digestive_desc", TypeName = "nvarchar(max)")]
    public string? DigestiveDesc { get; set; }

    [Column("urinary_desc", TypeName = "nvarchar(max)")]
    public string? UrinaryDesc { get; set; }

    [Column("musculoskeletal_desc", TypeName = "nvarchar(max)")]
    public string? MusculoskeletalDesc { get; set; }

    [Column("neurological_desc", TypeName = "nvarchar(max)")]
    public string? NeurologicalDesc { get; set; }

    [Column("conclusion", TypeName = "nvarchar(max)")]
    public string? Conclusion { get; set; }

    [Column("health_class", TypeName = "nvarchar(10)")]
    public string? HealthClass { get; set; }

    [Column("examining_doctor_id", TypeName = "int")]
    public int? ExaminingDoctorId { get; set; }

    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("RecordId")]
    public virtual VcHealthCheckupRecords? HealthCheckupRecord { get; set; }

    [ForeignKey("ExaminingDoctorId")]
    public virtual VcDoctors? ExaminingDoctor { get; set; }
}