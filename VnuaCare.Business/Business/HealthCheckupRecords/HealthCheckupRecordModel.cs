/**
 * Định nghĩa các Model / DTO của Phân hệ Hồ sơ khám sức khỏe tổng hợp (HealthCheckupRecords)
 */

using System.ComponentModel.DataAnnotations;
using VnuaCare.Data.Systems.Context;

namespace VnuaCare.Business.Business.HealthCheckupRecords;

/// <summary>
/// Lớp cơ sở chứa các thuộc tính chung của hồ sơ khám sức khỏe
/// </summary>
public abstract record HealthCheckupRecordBaseModel
{
    public int RecordId { get; init; }

    [Required(ErrorMessage = "Vui lòng nhập hoặc quét mã vạch hồ sơ (SID)")]
    public string SidCode { get; init; } = string.Empty;
    
    public DateTime ActualCheckupDate { get; init; }
    
    public string? MedicalHistoryPersonal { get; init; }
    
    public string? MedicalHistoryFamily { get; init; }
    
    public string? HealthClassification { get; init; }
    
    public string? GeneralConclusion { get; init; }
    
    public string? Recommendations { get; init; }
    public string? Recommentdations { get => Recommendations; init => Recommendations = value; } // Hỗ trợ tương thích ngược với code cũ
    
    public int ConcludedByDoctorId { get; init; }
    
    public DateTime ConcludedAt { get; init; }
    
    public string Status { get; init; } = string.Empty;
    
    public string? ReportPdfUrl { get; init; }
}

/// <summary>
/// DTO trả về thông tin chi tiết hồ sơ khám kèm thông tin cán bộ, đợt khám
/// </summary>
public record HealthCheckupRecordModel : HealthCheckupRecordBaseModel
{
    public int CampaignId { get; init; }
    public string? CampaignName { get; init; }
    
    public int StaffId { get; init; }
    public string? StaffName { get; init; }  
  
    public string? EmployeeCode { get; init; }
    
    public string? DepartmentName { get; init; }
    public string? DoctorName { get; init; }
    
    public string? Gender { get; init; }
    
    public DateTime DateOfBirth { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
}

/// <summary>
/// DTO nhận vào khi tạo mới hồ sơ khám sức khỏe
/// </summary>
public record CreateHealthCheckupRecordModel : HealthCheckupRecordBaseModel
{
    [Required(ErrorMessage = "Vui lòng chọn đợt khám sức khỏe (CampaignId).")]
    public int CampaignId { get; init; }

    [Required(ErrorMessage = "Vui lòng chọn cán bộ khám sức khỏe (StaffId).")]
    public int StaffId { get; init; }
}

/// <summary>
/// DTO nhận vào khi bác sĩ cập nhật kết luận và tình trạng hồ sơ khám
/// </summary>
public record UpdateHealthCheckupRecordModel : HealthCheckupRecordBaseModel
{
    public void UpdateEntity(VcHealthCheckupRecords entity)
    {
        if (!string.IsNullOrWhiteSpace(SidCode)) entity.SidCode = SidCode.Trim();
        if (ActualCheckupDate != default) entity.ActualCheckupDate = ActualCheckupDate;
        entity.MedicalHistoryPersonal = MedicalHistoryPersonal;
        entity.MedicalHistoryFamily = MedicalHistoryFamily;
        entity.HealthClassification = HealthClassification;
        entity.GeneralConclusion = GeneralConclusion;
        entity.Recommendations = Recommendations;
        entity.ReportPdfUrl = ReportPdfUrl;
        entity.ConcludedByDoctorId = ConcludedByDoctorId > 0 ? ConcludedByDoctorId : null;
        entity.ConcludedAt = ConcludedAt == default ? DateTime.Now : ConcludedAt;
        if (!string.IsNullOrWhiteSpace(Status))
        {
            entity.Status = Status.Trim();
        }
    }
}

public record HealthCheckupRecordSelectModel : HealthCheckupRecordBaseModel
{
}

/// <summary>
/// DTO phục vụ lọc và phân trang danh sách hồ sơ khám
/// </summary>
public record HealthCheckupRecordFilterModel : BaseQueryFilterModel
{
    public int? CampaignId { get; set; }
    public int? DepartmentId { get; set; }
    public string? Status { get; set; }
    public string? HealthClassification { get; set; }
}
