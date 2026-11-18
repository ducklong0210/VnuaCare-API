using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;

/// <summary>
/// Bảng kết quả khám các chuyên khoa lẻ lâm sàng (Mắt, Tai Mũi Họng, Răng Hàm Mặt, Da liễu, Ngoại khoa, Sản phụ khoa)
/// Bảng CSDL tương ứng: vc_specialist_exams
/// </summary>
[Table("vc_specialist_exams")]
public class VcSpecialistExams
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("specialist_exam_id", TypeName = "int")]
    public int SpecialistExamId { get; set; } // Khóa chính: ID kết quả khám chuyên khoa

    [Required]
    [Column("record_id", TypeName = "int")]
    public int RecordId { get; set; } // Khóa ngoại: ID hồ sơ khám sức khỏe liên kết

    [Required]
    [Column("specialty_type", TypeName = "varchar(30)")]
    public string SpecialtyType { get; set; } = string.Empty; // Mã loại chuyên khoa: EYE (Mắt), ENT (Tai Mũi Họng), DENTAL (Răng Hàm Mặt), DERMATOLOGY (Da liễu), SURGERY (Ngoại khoa), OBSTETRICS (Sản phụ khoa)

    // Khám Mắt (EYE)
    [Column("vision_left_no_glass", TypeName = "varchar(10)")]
    public string? VisionLeftNoGlass { get; set; } // Thị lực mắt trái không kính (ví dụ: 10/10)

    [Column("vision_right_no_glass", TypeName = "varchar(10)")]
    public string? VisionRightNoGlass { get; set; } // Thị lực mắt phải không kính (ví dụ: 10/10)

    [Column("vision_left_glass", TypeName = "varchar(10)")]
    public string? VisionLeftGlass { get; set; } // Thị lực mắt trái có kính (nếu cận/viễn/loạn)

    [Column("vision_right_glass", TypeName = "varchar(10)")]
    public string? VisionRightGlass { get; set; } // Thị lực mắt phải có kính (nếu cận/viễn/loạn)

    // Khám Răng Hàm Mặt (DENTAL)
    [Column("upper_jaw", TypeName = "nvarchar(255)")]
    public string? UpperJaw { get; set; } // Khám hàm trên (sâu răng, mất răng, vôi răng...)

    [Column("lower_jaw", TypeName = "nvarchar(255)")]
    public string? LowerJaw { get; set; } // Khám hàm dưới (sâu răng, mất răng...)

    [Column("teeth_formula", TypeName = "varchar(100)")]
    public string? TeethFormula { get; set; } // Sơ đồ / công thức răng bất thường

    // Khám Tai Mũi Họng (ENT)
    [Column("ear_left_desc", TypeName = "nvarchar(255)")]
    public string? EarLeftDesc { get; set; } // Khám tai trái (màng nhĩ, ống tai...)

    [Column("ear_right_desc", TypeName = "nvarchar(255)")]
    public string? EarRightDesc { get; set; } // Khám tai phải (màng nhĩ, thính lực...)

    [Column("nose_desc", TypeName = "nvarchar(255)")]
    public string? NoseDesc { get; set; } // Khám mũi (vách ngăn, niêm mạc, cuốn mũi...)

    [Column("throat_desc", TypeName = "nvarchar(255)")]
    public string? ThroatDesc { get; set; } // Khám họng, thanh quản (amidan, vòm...)

    // Khám Da liễu & Ngoại khoa & Sản phụ khoa
    [Column("skin_desc", TypeName = "nvarchar(500)")]
    public string? SkinDesc { get; set; } // Khám da liễu / tổn thương ngoài da

    [Column("surgical_desc", TypeName = "nvarchar(500)")]
    public string? SurgicalDesc { get; set; } // Khám ngoại khoa (vận động, chi, trĩ, thoát vị bẹn...)

    [Column("obstetric_desc", TypeName = "nvarchar(500)")]
    public string? ObstetricDesc { get; set; } // Khám sản phụ khoa (phụ khoa, cổ tử cung, vú - đối với nữ)

    // Đánh giá chung của chuyên khoa
    [Column("conclusion", TypeName = "nvarchar(max)")]
    public string? Conclusion { get; set; } // Kết luận chẩn đoán của chuyên khoa

    [Column("health_class", TypeName = "nvarchar(10)")]
    public string? HealthClass { get; set; } // Phân loại sức khỏe chuyên khoa: Loại I, II, III, IV, V

    [Column("examining_doctor_id", TypeName = "int")]
    public int? ExaminingDoctorId { get; set; } // ID bác sĩ thực hiện khám chuyên khoa

    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; } = DateTime.Now; // Thời điểm thực hiện khám

    [ForeignKey("RecordId")]
    public virtual VcHealthCheckupRecords? HealthCheckupRecord { get; set; } // Điều hướng: Hồ sơ khám liên kết

    [ForeignKey("ExaminingDoctorId")]
    public virtual VcDoctors? ExaminingDoctor { get; set; } // Điều hướng: Bác sĩ khám
}
