/**
 * Định nghĩa DTO của Phân hệ Khám Chuyên khoa lẻ (Specialist Exams: Mắt, Tai Mũi Họng, Răng Hàm Mặt, Da liễu, Ngoại, Sản)
 */

using System.ComponentModel.DataAnnotations;
using VnuaCare.Data.Systems.Context;

namespace VnuaCare.Business.Business.SpecialistExams;

/// <summary>
/// Lớp cơ sở chứa các trường khám chuyên khoa lâm sàng lẻ
/// </summary>
public abstract record SpecialistExamBaseModel
{
    // Khám Mắt (EYE)
    public string? VisionLeftNoGlass { get; init; }
    public string? VisionRightNoGlass { get; init; }
    public string? VisionLeftGlass { get; init; }
    public string? VisionRightGlass { get; init; }

    // Khám Răng Hàm Mặt (DENTAL)
    public string? UpperJaw { get; init; }
    public string? LowerJaw { get; init; }
    public string? TeethFormula { get; init; }

    // Khám Tai Mũi Họng (ENT)
    public string? EarLeftDesc { get; init; }
    public string? EarRightDesc { get; init; }
    public string? NoseDesc { get; init; }
    public string? ThroatDesc { get; init; }

    // Khám Da liễu, Ngoại khoa, Sản phụ khoa
    public string? SkinDesc { get; init; }
    public string? SurgicalDesc { get; init; }
    public string? ObstetricDesc { get; init; }

    // Đánh giá chung
    public string? Conclusion { get; init; }
    public string? HealthClass { get; init; }
    public int? ExaminingDoctorId { get; init; }
}

/// <summary>
/// DTO thêm mới kết quả khám chuyên khoa lẻ (Create)
/// </summary>
public record CreateSpecialistExamModel : SpecialistExamBaseModel
{
    [Required(ErrorMessage = "ID hồ sơ khám sức khỏe không được để trống")]
    public int RecordId { get; init; }

    [Required(ErrorMessage = "Vui lòng chỉ định loại chuyên khoa: EYE, ENT, DENTAL, DERMATOLOGY, SURGERY, OBSTETRICS")]
    public string SpecialtyType { get; init; } = string.Empty;
}

/// <summary>
/// DTO cập nhật kết quả khám chuyên khoa lẻ (Update)
/// </summary>
public record UpdateSpecialistExamModel : SpecialistExamBaseModel
{
    [Required(ErrorMessage = "ID kết quả khám chuyên khoa không được để trống")]
    public int SpecialistExamId { get; init; }

    public void UpdateEntity(VcSpecialistExams entity)
    {
        entity.VisionLeftNoGlass = VisionLeftNoGlass;
        entity.VisionRightNoGlass = VisionRightNoGlass;
        entity.VisionLeftGlass = VisionLeftGlass;
        entity.VisionRightGlass = VisionRightGlass;
        entity.UpperJaw = UpperJaw;
        entity.LowerJaw = LowerJaw;
        entity.TeethFormula = TeethFormula;
        entity.EarLeftDesc = EarLeftDesc;
        entity.EarRightDesc = EarRightDesc;
        entity.NoseDesc = NoseDesc;
        entity.ThroatDesc = ThroatDesc;
        entity.SkinDesc = SkinDesc;
        entity.SurgicalDesc = SurgicalDesc;
        entity.ObstetricDesc = ObstetricDesc;
        entity.Conclusion = Conclusion;
        entity.HealthClass = HealthClass;
        if (ExaminingDoctorId.HasValue && ExaminingDoctorId.Value > 0)
        {
            entity.ExaminingDoctorId = ExaminingDoctorId.Value;
        }
    }
}

/// <summary>
/// DTO trả về chi tiết kết quả khám chuyên khoa lẻ (Read)
/// </summary>
public record SpecialistExamModel : SpecialistExamBaseModel
{
    public int SpecialistExamId { get; init; }
    public int RecordId { get; init; }
    public string SpecialtyType { get; init; } = string.Empty;
    public string? ExaminingDoctorName { get; init; }
    public DateTime CreatedAt { get; init; }
}
