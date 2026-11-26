namespace VnuaCare.Business.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlContent, string referenceType = "System", int referenceId = 0);
}