using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Data.Systems.Context;

/// <summary>
/// Bảng lưu lịch sử gửi Email thông báo / cảnh báo sức khỏe của hệ thống (vc_email_logs)
/// </summary>
[Table("vc_email_logs")]
public class VcEmailLogs
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("email_log_id", TypeName = "int")]
    public int EmailLogId { get; set; } // Khóa chính: ID nhật ký gửi email

    [Required]
    [Column("code", TypeName = "varchar(50)")]
    public string Code { get; set; } = string.Empty; // Mã tra cứu nhật ký (ví dụ: LOG-A1B2C3D4)

    [Required]
    [Column("to_email", TypeName = "varchar(255)")]
    public string ToEmail { get; set; } = string.Empty; // Địa chỉ email người nhận

    [Required]
    [Column("subject", TypeName = "nvarchar(500)")]
    public string Subject { get; set; } = string.Empty; // Tiêu đề email

    [Column("body", TypeName = "nvarchar(max)")]
    public string? Body { get; set; } // Nội dung HTML của email đã gửi

    [Required]
    [Column("sent_status", TypeName = "varchar(30)")]
    public string SentStatus { get; set; } = string.Empty; // Trạng thái gửi: Success / Failed

    [Column("error_message", TypeName = "nvarchar(max)")]
    public string? ErrorMessage { get; set; } // Chi tiết lỗi SMTP nếu gửi thất bại

    [Column("created_date", TypeName = "datetime2")]
    public DateTime CreatedDate { get; set; } = DateTime.Now; // Thời điểm gửi email

    [Column("reference_type", TypeName = "varchar(50)")]
    public string? ReferenceType { get; set; } // Loại nghiệp vụ liên kết (System, HealthCheckupRecord, ForgotPassword)

    [Column("reference_id", TypeName = "int")]
    public int ReferenceId { get; set; } // ID bản ghi liên kết (ví dụ: RecordId của hồ sơ khám)
}
