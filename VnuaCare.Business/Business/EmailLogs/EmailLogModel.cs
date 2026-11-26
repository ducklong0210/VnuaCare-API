/**
 * Định nghĩa DTO của Phân hệ Nhật ký gửi Email (EmailLogs)
 */

namespace VnuaCare.Business.Business.EmailLogs;

/// <summary>
/// DTO trả về thông tin chi tiết nhật ký gửi Email
/// </summary>
public record EmailLogModel
{
    public int EmailLogId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string ToEmail { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string? Body { get; init; }
    public string SentStatus { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public DateTime CreatedDate { get; init; }
    public string? ReferenceType { get; init; }
    public int ReferenceId { get; init; }
}

/// <summary>
/// DTO điều kiện lọc và phân trang danh sách nhật ký gửi Email
/// </summary>
public record EmailLogFilterModel : BaseQueryFilterModel
{
    public string? SentStatus { get; set; }      // Lọc theo trạng thái: Success / Failed
    public string? ReferenceType { get; set; }   // Lọc theo nghiệp vụ: HealthCheckupRecord, ForgotPassword, System
    public int? ReferenceId { get; set; }        // Lọc theo ID bản ghi liên kết (ví dụ RecordId)
    public DateTime? FromDate { get; set; }      // Lọc từ ngày gửi
    public DateTime? ToDate { get; set; }        // Lọc đến ngày gửi
}
