/**
 * Định nghĩa DTO của Phân hệ Khám Lâm sàng Nội khoa tổng quát (Internal Medicine)
 * Chuẩn CRUD: Create, Update, Read Model tách bạch
 */

using System.ComponentModel.DataAnnotations;
using VnuaCare.Data.Systems.Context;

namespace VnuaCare.Business.Business.InternalMedicine;

/// <summary>
/// Lớp cơ sở chứa các trường khám nội khoa 6 hệ cơ quan
/// </summary>
public abstract record InternalMedicineExamBaseModel
{
    public string? CirculatoryDesc { get; init; }      // Tuần hoàn / Tim mạch (tiếng tim T1 T2, nhịp xoang...)
    public string? RespiratoryDesc { get; init; }      // Hô hấp / Phổi (thông khí phổi, tiếng rale...)
    public string? DigestiveDesc { get; init; }        // Tiêu hóa / Ổ bụng (bụng mềm, gan lách...)
    public string? UrinaryDesc { get; init; }          // Thận - tiết niệu (hố thắt lưng, phản hồi thận...)
    public string? MusculoskeletalDesc { get; init; }  // Cơ xương khớp (tầm vận động, biến dạng...)
    public string? NeurologicalDesc { get; init; }     // Thần kinh - tâm thần (tri giác, phản xạ gân xương...)
    public string? Conclusion { get; init; }           // Kết luận khám nội khoa
    public string? HealthClass { get; init; }          // Xếp loại sức khỏe nội khoa (Loại I - Loại V)
    public int? ExaminingDoctorId { get; init; }       // ID bác sĩ khám
}

/// <summary>
/// DTO thêm mới kết quả khám nội khoa (Create)
/// </summary>
public record CreateInternalMedicineExamModel : InternalMedicineExamBaseModel
{
    [Required(ErrorMessage = "ID hồ sơ khám sức khỏe không được để trống")]
    public int RecordId { get; init; }
}

/// <summary>
/// DTO cập nhật kết quả khám nội khoa (Update)
/// </summary>
public record UpdateInternalMedicineExamModel : InternalMedicineExamBaseModel
{
    [Required(ErrorMessage = "ID kết quả khám nội khoa không được để trống")]
    public int InternalExamId { get; init; }

    public void UpdateEntity(VcInternalMedicineExams entity)
    {
        entity.CirculatoryDesc = CirculatoryDesc;
        entity.RespiratoryDesc = RespiratoryDesc;
        entity.DigestiveDesc = DigestiveDesc;
        entity.UrinaryDesc = UrinaryDesc;
        entity.MusculoskeletalDesc = MusculoskeletalDesc;
        entity.NeurologicalDesc = NeurologicalDesc;
        entity.Conclusion = Conclusion;
        entity.HealthClass = HealthClass;
        if (ExaminingDoctorId.HasValue && ExaminingDoctorId.Value > 0)
        {
            entity.ExaminingDoctorId = ExaminingDoctorId.Value;
        }
    }
}

/// <summary>
/// DTO trả về chi tiết kết quả khám nội khoa (Read)
/// </summary>
public record InternalMedicineExamModel : InternalMedicineExamBaseModel
{
    public int InternalExamId { get; init; }
    public int RecordId { get; init; }
    public string? ExaminingDoctorName { get; init; }
    public DateTime CreatedAt { get; init; }
}
