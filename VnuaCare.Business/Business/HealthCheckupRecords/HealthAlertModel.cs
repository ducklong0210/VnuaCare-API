/**
 * Định nghĩa DTO và hàm quy đổi xếp loại sức khỏe phục vụ tính năng Cảnh báo sức khỏe giảm qua Email
 */

namespace VnuaCare.Business.Business.HealthCheckupRecords;

/// <summary>
/// DTO biểu diễn hồ sơ cán bộ có sức khỏe suy giảm so với năm trước
/// </summary>
public class DeclinedHealthRecordModel
{
    public int RecordId { get; set; }
    public string SidCode { get; set; } = string.Empty;
    public int CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public int CurrentYear { get; set; }
    public int StaffId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? Email { get; set; }
    public string? PreviousClassification { get; set; }
    public int PreviousYear { get; set; }
    public string? CurrentClassification { get; set; }
    public string? GeneralConclusion { get; set; }
    public string? Recommendations { get; set; }
    public DateTime? AlertSentAt { get; set; }
}

/// <summary>
/// DTO yêu cầu gửi email cảnh báo sức khỏe (gửi lẻ theo RecordId hoặc gửi hàng loạt theo CampaignId)
/// </summary>
public class SendHealthAlertRequestModel
{
    public int? RecordId { get; set; }
    public int? CampaignId { get; set; }
    public bool ForceResend { get; set; } = false;
}

/// <summary>
/// DTO thống kê kết quả gửi email cảnh báo sức khỏe
/// </summary>
public class SendHealthAlertResultModel
{
    public int TotalDeclinedFound { get; set; }
    public int TotalSent { get; set; }
    public int TotalSkippedAlreadySent { get; set; }
    public int TotalFailed { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Tiện ích quy đổi xếp loại sức khỏe (Loại I -> V) sang bậc số nguyên để so sánh tăng/giảm
/// </summary>
public static class HealthClassificationHelper
{
    public static int? ParseRank(string? classification)
    {
        if (string.IsNullOrWhiteSpace(classification)) return null;
        string norm = classification.Trim().ToUpperInvariant();

        if (norm.Contains("IV") || norm.Contains("4")) return 4;
        if (norm.Contains("III") || norm.Contains("3")) return 3;
        if (norm.Contains("II") || norm.Contains("2")) return 2;
        if (norm.Contains("V") || norm.Contains("5")) return 5;
        if (norm.Contains("I") || norm.Contains("1")) return 1;

        return null;
    }
}
