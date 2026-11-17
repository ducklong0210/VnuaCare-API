using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;

/// <summary>
/// Bảng đo lường thể lực và các chỉ số sinh tồn tại bàn đón tiếp ban đầu (vc_vital_signs)
/// </summary>
[Table("vc_vital_signs")]
public class VcVitalSigns
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("vital_sign_id", TypeName = "int")]
    public int VitalSignId { get; set; } // Khóa chính: ID chỉ số sinh tồn

    [Required]
    [Column("record_id", TypeName = "int")]
    public int RecordId { get; set; } // Khóa ngoại 1-1: ID hồ sơ khám sức khỏe liên kết

    [Column("height_cm", TypeName = "numeric(5,2)")]
    public decimal? HeightCm { get; set; } // Chiều cao cơ thể (cm)

    [Column("weight_kg", TypeName = "numeric(5,2)")]
    public decimal? WeightKg { get; set; } // Cân nặng cơ thể (kg)

    [Column("bmi", TypeName = "numeric(4,2)")]
    public decimal? Bmi { get; set; } // Chỉ số khối cơ thể BMI: Cân nặng / (Chiều cao/100)^2

    [Column("waist_cm", TypeName = "numeric(5,2)")]
    public decimal? WaistCm { get; set; } // Vòng bụng / vòng eo (cm)

    [Column("systolic_bp", TypeName = "int")]
    public int? SystolicBp { get; set; } // Huyết áp tâm thu (Huyết áp tối đa, mmHg)

    [Column("diastolic_bp", TypeName = "int")]
    public int? DiastolicBp { get; set; } // Huyết áp tâm trương (Huyết áp tối thiểu, mmHg)

    [Column("heart_rate_bpm", TypeName = "int")]
    public int? HeartRateBpm { get; set; } // Nhịp tim / mạch (lần/phút)

    [Column("respiratory_rate", TypeName = "int")]
    public int? RespiratoryRate { get; set; } // Nhịp thở (lần/phút)

    [Column("spo2_percent", TypeName = "numeric(4,1)")]
    public decimal? Spo2Percent { get; set; } // Độ bão hòa oxy trong máu ngoại vi SpO2 (%)

    [Column("measured_by", TypeName = "int")]
    public int? MeasuredBy { get; set; } // ID điều dưỡng / bác sĩ trực tiếp đo

    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; } = DateTime.Now; // Thời điểm đo

    [ForeignKey("RecordId")]
    public virtual VcHealthCheckupRecords? HealthCheckupRecord { get; set; } // Điều hướng: Hồ sơ khám tương ứng

    [ForeignKey("MeasuredBy")]
    public virtual VcDoctors? MeasuredByDoctor { get; set; } // Điều hướng: Bác sĩ / điều dưỡng đo
}
