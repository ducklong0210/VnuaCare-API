/**
 * Định nghĩa DTO của Phân hệ Chẩn đoán hình ảnh & Thăm dò chức năng (Diagnostic Reports)
 * Bao gồm Siêu âm, X-quang, Điện tim, Nội soi, CT, MRI...
 */

using System.ComponentModel.DataAnnotations;
using VnuaCare.Data.Systems.Context;

namespace VnuaCare.Business.Business.DiagnosticReports;

/// <summary>
/// Lớp cơ sở chứa thông tin kết quả chẩn đoán hình ảnh & thăm dò chức năng
/// </summary>
public abstract record DiagnosticReportBaseModel
{
    [Required(ErrorMessage = "Loại chẩn đoán hình ảnh không được để trống (ví dụ: ULTRASOUND, XRAY, ECG, ENDOSCOPY, CT, MRI)")]
    public string ModalityType { get; init; } = string.Empty;

    [Required(ErrorMessage = "Tiêu đề chỉ định / kỹ thuật khám không được để trống")]
    public string ExamTitle { get; init; } = string.Empty;

    public string? TechniqueDescription { get; init; }
    public string? FindingsData { get; init; }

    [Required(ErrorMessage = "Kết luận chẩn đoán hình ảnh không được để trống")]
    public string Conclusion { get; init; } = string.Empty;

    public string? ImageUrls { get; init; }
    public int? ReadingDoctorId { get; init; }
    public int? ApprovingDoctorId { get; init; }
    public DateTime? PerformedAt { get; init; }
}

/// <summary>
/// DTO thêm mới phiếu kết quả chẩn đoán hình ảnh (Create)
/// </summary>
public record CreateDiagnosticReportModel : DiagnosticReportBaseModel
{
    [Required(ErrorMessage = "ID hồ sơ khám sức khỏe không được để trống")]
    public int RecordId { get; init; }
}

/// <summary>
/// DTO cập nhật phiếu kết quả chẩn đoán hình ảnh (Update)
/// </summary>
public record UpdateDiagnosticReportModel : DiagnosticReportBaseModel
{
    [Required(ErrorMessage = "ID phiếu kết quả chẩn đoán hình ảnh không được để trống")]
    public int ReportId { get; init; }

    public void UpdateEntity(VcDiagnosticReports entity)
    {
        entity.ModalityType = ModalityType.Trim().ToUpperInvariant();
        entity.ExamTitle = ExamTitle.Trim();
        entity.TechniqueDescription = TechniqueDescription;
        entity.FindingsData = FindingsData;
        entity.Conclusion = Conclusion.Trim();
        entity.ImageUrls = ImageUrls;
        if (ReadingDoctorId.HasValue && ReadingDoctorId.Value > 0)
        {
            entity.ReadingDoctorId = ReadingDoctorId.Value;
        }
        if (ApprovingDoctorId.HasValue && ApprovingDoctorId.Value > 0)
        {
            entity.ApprovingDoctorId = ApprovingDoctorId.Value;
        }
        if (PerformedAt.HasValue)
        {
            entity.PerformedAt = PerformedAt.Value;
        }
    }
}

/// <summary>
/// DTO trả về chi tiết kết quả chẩn đoán hình ảnh (Read)
/// </summary>
public record DiagnosticReportModel : DiagnosticReportBaseModel
{
    public int ReportId { get; init; }
    public int RecordId { get; init; }
    public string? ReadingDoctorName { get; init; }
    public string? ApprovingDoctorName { get; init; }
}

/// <summary>
/// DTO lọc và phân trang danh sách kết quả chẩn đoán hình ảnh
/// </summary>
public class DiagnosticReportFilterModel
{
    public int? RecordId { get; set; }
    public string? ModalityType { get; set; }
    public string? TextSearch { get; set; }
    public int? ReadingDoctorId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
