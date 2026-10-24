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
        entity.Recommendations = Recommentdations;
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
    public int? CampaignId {get; set;}
    public int? DepartmentId {get; set;}
    
    public string? Status {get; set;}
    
    public string? HealthClassification {get; set;}
}

/// <summary>
/// DTO danh mục Đợt khám sức khỏe phục vụ hiển thị combobox / dropdown
/// </summary>
public record CampaignSelectModel
{
    public int CampaignId { get; init; }
    public string CampaignName { get; init; }
    public string CampaignCode { get; init; }
    public int Year { get; init; }
    public string Status { get; init; }
}

/// <summary>
/// DTO danh mục Phân loại sức khỏe (Theo tiêu chuẩn Bộ Y tế: Loại I đến Loại V)
/// </summary>
public record HealthClassificationSelectModel
{
    public string ClassificationCode { get; init; } = string.Empty;
    public string ClassificationName { get; init; } = string.Empty;
    public string? Description { get; init; }
}

/// <summary>
/// DTO danh mục Trạng thái hồ sơ khám sức khỏe
/// </summary>
public record RecordStatusSelectModel
{
    public string StatusCode { get; init; } = string.Empty;
    public string StatusName { get; init; } = string.Empty;
    public string? Description { get; init; }
}

/// <summary>
/// DTO nhận vào khi bác sĩ nhập hoặc cập nhật kết quả khám lâm sàng nội khoa tổng quát
/// </summary>
public record SaveInternalMedicineExamModel
{
    public int? InternalExamId { get; init; }

    [Required(ErrorMessage = "ID hồ sơ khám sức khỏe không được để trống")]
    public int RecordId { get; init; }

    public string? CirculatoryDesc { get; init; } // Tuần hoàn / Tim mạch
    public string? RespiratoryDesc { get; init; } // Hô hấp / Phổi
    public string? DigestiveDesc { get; init; }   // Tiêu hóa / Ổ bụng
    public string? UrinaryDesc { get; init; }     // Thận - tiết niệu
    public string? MusculoskeletalDesc { get; init; } // Cơ xương khớp
    public string? NeurologicalDesc { get; init; }    // Thần kinh - tâm thần
    public string? Conclusion { get; init; }      // Kết luận khám nội khoa
    public string? HealthClass { get; init; }     // Xếp loại sức khỏe nội khoa (Loại I - Loại V)
    public int? ExaminingDoctorId { get; init; }  // Bác sĩ khám
}

/// <summary>
/// DTO chi tiết kết quả khám lâm sàng nội khoa
/// </summary>
public record InternalMedicineExamModel : SaveInternalMedicineExamModel
{
    public string? ExaminingDoctorName { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// DTO thêm mới Đợt khám sức khỏe
/// </summary>
public record CreateCampaignModel
{
    [Required(ErrorMessage = "Tên đợt khám không được để trống")]
    public string CampaignName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Mã đợt khám không được để trống")]
    public string CampaignCode { get; init; } = string.Empty;

    public int Year { get; init; } = DateTime.Now.Year;

    public DateTime StartDate { get; init; } = DateTime.Today;

    public DateTime EndDate { get; init; } = DateTime.Today.AddDays(7);

    public string HospitalPartner { get; init; } = "Bệnh viện Đa khoa MEDLATEC";

    public string Status { get; init; } = "PLANNING";
}

/// <summary>
/// DTO cập nhật Đợt khám sức khỏe
/// </summary>
public record UpdateCampaignModel : CreateCampaignModel
{
    [Required(ErrorMessage = "ID đợt khám không được để trống")]
    public int CampaignId { get; init; }
}
