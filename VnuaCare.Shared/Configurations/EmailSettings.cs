namespace VnuaCare.Shared.Configurations;

/// <summary>
/// Cấu hình máy chủ SMTP dùng để gửi Email thông báo (ánh xạ từ mục EmailSettings trong appsettings.json)
/// </summary>
public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPass { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}