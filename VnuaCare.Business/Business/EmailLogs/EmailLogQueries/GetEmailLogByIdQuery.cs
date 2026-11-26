/**
 * Nghiệp vụ lấy chi tiết một bản ghi nhật ký gửi Email theo ID (GetEmailLogByIdQuery)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.EmailLogs.EmailLogQueries;

/// <summary>
/// Query lấy chi tiết lịch sử gửi Email (kèm nội dung HTML và lỗi SMTP nếu có)
/// </summary>
public class GetEmailLogByIdQuery : IRequest<EmailLogModel>
{
    public int Id { get; set; }

    public GetEmailLogByIdQuery(int id)
    {
        Id = id;
    }

    public class Handler : IRequestHandler<GetEmailLogByIdQuery, EmailLogModel>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareReadDataContext dataContext, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _contextAccessor = contextAccessor();
        }

        public async Task<EmailLogModel> Handle(GetEmailLogByIdQuery request, CancellationToken cancellationToken)
        {
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem chi tiết nhật ký gửi Email.");
            }

            var log = await _dataContext.VcEmailLogs.AsNoTracking()
                .Where(x => x.EmailLogId == request.Id)
                .Select(x => new EmailLogModel
                {
                    EmailLogId = x.EmailLogId,
                    Code = x.Code,
                    ToEmail = x.ToEmail,
                    Subject = x.Subject,
                    Body = x.Body,
                    SentStatus = x.SentStatus,
                    ErrorMessage = x.ErrorMessage,
                    CreatedDate = x.CreatedDate,
                    ReferenceType = x.ReferenceType,
                    ReferenceId = x.ReferenceId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (log == null)
            {
                throw new ArgumentException($"Không tìm thấy nhật ký gửi Email với ID: {request.Id}");
            }

            return log;
        }
    }
}
