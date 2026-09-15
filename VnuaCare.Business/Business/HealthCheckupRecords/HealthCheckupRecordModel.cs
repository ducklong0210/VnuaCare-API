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
    public string SidCode{ get; init; }
    
    public DateTime ActualCheckupDate { get; init; }
    
    public string? MedicalHistoryPersonal { get; init; }
    
    public string? MedicalHistoryFamily { get; init; }
    
    public string? HealthClassification { get; init; }
    
    public string? GeneralConclusion { get; init; }
    
    public string? Recommentdations { get; init; }
    
    public int ConcludedByDoctorId { get; init; }
    
    public DateTime ConcludedAt { get; init; }
    
    public string Status { get; init; }
    
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
    public string ? StaffName { get; init; }  
  
    public string? EmployeeCode { get; init; }
    
    public string? DepartmentName { get; init; }
    public string? DoctorName { get; init; }
    
    public string? Gender { get; init; }
    
    public DateTime DateOfBirth { get; init; }
    public string PhoneNumber { get; init; }
}

public record CreateHealthCheckupRecordModel : HealthCheckupRecordBaseModel
{
    
}
/// <summary>
/// DTO nhận vào khi bác sĩ cập nhật kết luận và tình trạng hồ sơ khám
/// </summary>
public record UpdateHealthCheckupRecordModel : HealthCheckupRecordBaseModel
{

    public void UpdateEntity(VcHealthCheckupRecords entity)
    {
        entity.SidCode = SidCode;
        entity.ActualCheckupDate = ActualCheckupDate;
        entity.MedicalHistoryPersonal = MedicalHistoryPersonal;
        entity.MedicalHistoryFamily = MedicalHistoryFamily;
        entity.HealthClassification = HealthClassification;
        entity.GeneralConclusion = GeneralConclusion;
        entity.Recommendations = Recommentdations;
        entity.ReportPdfUrl = ReportPdfUrl;
        entity.ConcludedByDoctorId = ConcludedByDoctorId;
        entity.ConcludedAt = ConcludedAt;
        entity.Status = Status;
    }
}

public record HealthCheckupRecordSelectModel : HealthCheckupRecordBaseModel
{
    
}
/// <summary>
/// DTO phục vụ lọc và phân trang danh sách hồ sơ khám
/// </summary>
public record HealthCheckupRecordFilterModel : HealthCheckupRecordBaseModel
{
    public int? CampaignId {get; set;}
    public int? DepartmentId {get; set;}
    
    public string? Status {get; set;}
    
    public string? HealthClassification {get; set;}
}