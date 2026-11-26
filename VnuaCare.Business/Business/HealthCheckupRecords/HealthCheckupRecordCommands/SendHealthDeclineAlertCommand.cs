/**
 * Nghiệp vụ gửi Email cảnh báo cho cán bộ có tình trạng sức khỏe suy giảm so với năm trước
 * Hỗ trợ gửi lẻ theo RecordId hoặc gửi hàng loạt theo CampaignId, tự động chống gửi trùng qua vc_email_logs
 */

using System.Net;
using MediatR;
using Serilog;
using VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordQueries;
using VnuaCare.Business.Services;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordCommands;

public class SendHealthDeclineAlertCommand : IRequest<SendHealthAlertResultModel>
{
    public SendHealthAlertRequestModel Model { get; }

    public SendHealthDeclineAlertCommand(SendHealthAlertRequestModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<SendHealthDeclineAlertCommand, SendHealthAlertResultModel>
    {
        private readonly IMediator _mediator;
        private readonly IEmailService _emailService;
        private readonly IContextAccessor _contextAccessor;

        public Handler(IMediator mediator, IEmailService emailService, Func<IContextAccessor> contextAccessor)
        {
            _mediator = mediator;
            _emailService = emailService;
            _contextAccessor = contextAccessor();
        }

        public async Task<SendHealthAlertResultModel> Handle(SendHealthDeclineAlertCommand request, CancellationToken cancellationToken)
        {
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.DOCTOR.ToString())
            {
                throw new UnauthorizedAccessException("Chỉ Quản trị viên y tế hoặc Bác sĩ mới có quyền gửi email cảnh báo sức khỏe.");
            }

            var model = request.Model;
            var declinedList = await _mediator.Send(new GetDeclinedHealthRecordsQuery(model.CampaignId), cancellationToken);

            if (model.RecordId.HasValue && model.RecordId.Value > 0)
            {
                declinedList = declinedList.Where(x => x.RecordId == model.RecordId.Value).ToList();
            }

            var result = new SendHealthAlertResultModel
            {
                TotalDeclinedFound = declinedList.Count
            };

            foreach (var item in declinedList)
            {
                if (item.AlertSentAt.HasValue && !model.ForceResend)
                {
                    result.TotalSkippedAlreadySent++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Email))
                {
                    result.TotalFailed++;
                    result.Errors.Add($"Cán bộ {item.StaffName} ({item.EmployeeCode}) chưa có địa chỉ Email.");
                    continue;
                }

                try
                {
                    string subject = $"[VnuaCare] Cảnh báo theo dõi sức khỏe định kỳ năm {item.CurrentYear}";
                    string htmlBody = BuildAlertHtmlBody(item);

                    await _emailService.SendEmailAsync(
                        to: item.Email.Trim(),
                        subject: subject,
                        htmlContent: htmlBody,
                        referenceType: "HealthCheckupRecord",
                        referenceId: item.RecordId);

                    result.TotalSent++;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Gửi email cảnh báo thất bại cho hồ sơ ID {item.RecordId} ({item.Email})");
                    result.TotalFailed++;
                    result.Errors.Add($"{item.StaffName} ({item.Email}): {ex.Message}");
                }
            }

            return result;
        }

        private static string BuildAlertHtmlBody(DeclinedHealthRecordModel item)
        {
            string name = WebUtility.HtmlEncode(item.StaffName);
            string code = WebUtility.HtmlEncode(item.EmployeeCode);
            string dept = WebUtility.HtmlEncode(item.DepartmentName ?? "Không xác định");
            string prevClass = WebUtility.HtmlEncode(item.PreviousClassification ?? "Chưa xác định");
            string curClass = WebUtility.HtmlEncode(item.CurrentClassification ?? "Chưa xác định");
            string conclusion = WebUtility.HtmlEncode(item.GeneralConclusion ?? "Vui lòng đăng nhập hệ thống VnuaCare để xem chi tiết.");
            string recommendation = WebUtility.HtmlEncode(item.Recommendations ?? "Nên tái khám chuyên khoa và điều chỉnh chế độ sinh hoạt, dinh dưỡng hợp lý.");

            return $@"
<div style=""font-family: Arial, sans-serif; max-width: 640px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden;"">
  <div style=""background-color: #0f766e; color: #ffffff; padding: 20px 24px;"">
    <h2 style=""margin: 0; font-size: 18px;"">THÔNG BÁO CẢNH BÁO SỨC KHỎE ĐỊNH KỲ</h2>
    <p style=""margin: 6px 0 0; font-size: 13px; opacity: 0.9;"">Hệ thống Quản lý Sức khỏe Cán bộ - Học viện Nông nghiệp Việt Nam (VnuaCare)</p>
  </div>
  <div style=""padding: 24px; color: #1e293b; line-height: 1.6; font-size: 14px;"">
    <p>Kính gửi cán bộ <strong>{name}</strong> (Mã CB: <strong>{code}</strong> - {dept}),</p>
    <p>Qua kết quả tổng hợp khám sức khỏe định kỳ năm <strong>{item.CurrentYear}</strong>, hệ thống ghi nhận phân loại sức khỏe của bạn có <strong>dấu hiệu suy giảm so với năm {item.PreviousYear}</strong>:</p>
    <table style=""width: 100%; border-collapse: collapse; margin: 16px 0; font-size: 14px;"">
      <tr style=""background-color: #f8fafc;"">
        <th style=""border: 1px solid #cbd5e1; padding: 10px; text-align: left;"">Kỳ khám</th>
        <th style=""border: 1px solid #cbd5e1; padding: 10px; text-align: left;"">Phân loại sức khỏe</th>
      </tr>
      <tr>
        <td style=""border: 1px solid #cbd5e1; padding: 10px;"">Năm {item.PreviousYear}</td>
        <td style=""border: 1px solid #cbd5e1; padding: 10px; color: #0f766e; font-weight: bold;"">{prevClass}</td>
      </tr>
      <tr>
        <td style=""border: 1px solid #cbd5e1; padding: 10px;"">Năm {item.CurrentYear}</td>
        <td style=""border: 1px solid #cbd5e1; padding: 10px; color: #dc2626; font-weight: bold;"">{curClass}</td>
      </tr>
    </table>
    <p><strong>Kết luận của Hội đồng Bác sĩ:</strong><br/>{conclusion}</p>
    <p><strong>Khuyến nghị &amp; Lời dặn dò:</strong><br/>{recommendation}</p>
    <p style=""margin-top: 20px; font-size: 13px; color: #64748b;"">Vui lòng đăng nhập vào cổng thông tin VnuaCare để xem đầy đủ chỉ số sinh tồn, kết quả khám chuyên khoa và chẩn đoán hình ảnh.</p>
  </div>
</div>";
        }
    }
}
