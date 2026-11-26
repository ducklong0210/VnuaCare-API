using MailKit.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MimeKit;
using Serilog;
using VnuaCare.Business.Services;
using VnuaCare.Data.Systems.Context;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.Configurations;

namespace VnuaCare.Infrastructure.Services;

/// <summary>
/// Dịch vụ gửi Email qua giao thức SMTP (thư viện MailKit) và tự động lưu nhật ký vào bảng vc_email_logs
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EmailService(IOptions<EmailSettings> emailSettings, IServiceScopeFactory serviceScopeFactory)
    {
        _emailSettings = emailSettings.Value;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlContent, string referenceType = "System", int referenceId = 0)
    {
        string errorMessage = string.Empty;
        string status = "Success";

        Log.Information("Sending email to {Email} with subject {Subject}", to, subject);
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(string.Empty, to));
            message.Subject = subject;
            message.Body = new TextPart("html")
            {
                Text = htmlContent
            };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            client.CheckCertificateRevocation = false;

            var socketOption = _emailSettings.EnableSsl
                ? SecureSocketOptions.StartTlsWhenAvailable
                : SecureSocketOptions.None;

            Log.Information("[EmailService] Connecting to SMTP server {Host}:{Port}...", _emailSettings.SmtpServer, _emailSettings.SmtpPort);
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, socketOption);

            Log.Information("[EmailService] Authenticating...");
            await client.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPass);

            Log.Information("[EmailService] Sending...");
            await client.SendAsync(new MailKit.SendRequest(message));

            Log.Information("[EmailService] Disconnecting...");
            await client.DisconnectAsync(true);

            Log.Information($"[EmailService] Email sent successfully to {to}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"[EmailService] Failed to send email to {to}");
            status = "Failed";
            errorMessage = ex.Message;

            throw new Exception($"Email sending failed: {ex.Message}");
        }
        finally
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<VnuaCareDataContext>();

                var emailLog = new VcEmailLogs
                {
                    Code = $"LOG-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    ToEmail = to,
                    Subject = subject,
                    Body = htmlContent,
                    SentStatus = status,
                    ErrorMessage = errorMessage,
                    CreatedDate = DateTime.Now,
                    ReferenceType = referenceType,
                    ReferenceId = referenceId
                };

                await dbContext.VcEmailLogs.AddAsync(emailLog);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                Log.Error(dbEx, "[EmailService] Failed to save email log to database");
            }
        }
    }
}
