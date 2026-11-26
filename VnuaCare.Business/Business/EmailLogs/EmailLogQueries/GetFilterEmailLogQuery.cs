/**
 * Nghiệp vụ lọc, tìm kiếm và phân trang danh sách Email đã gửi (GetFilterEmailLogQuery)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;
using VnuaCare.Shared.Utils;

namespace VnuaCare.Business.Business.EmailLogs.EmailLogQueries;

/// <summary>
/// Lấy danh sách lịch sử Email đã gửi có lọc và phân trang
/// </summary>
public class GetFilterEmailLogQuery : IRequest<PaginationList<EmailLogModel>>
{
    public EmailLogFilterModel Filter { get; set; }

    public GetFilterEmailLogQuery(EmailLogFilterModel filter)
    {
        Filter = filter;
    }

    public class Handler : IRequestHandler<GetFilterEmailLogQuery, PaginationList<EmailLogModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareReadDataContext dataContext, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _contextAccessor = contextAccessor();
        }

        public async Task<PaginationList<EmailLogModel>> Handle(GetFilterEmailLogQuery request, CancellationToken cancellationToken)
        {
            var filter = request.Filter;
            var currentUserRole = _contextAccessor.Role;

            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem danh sách nhật ký gửi Email.");
            }

            // Đọc dữ liệu bảng vc_email_logs bằng AsNoTracking tối ưu hiệu năng
            var query = _dataContext.VcEmailLogs.AsNoTracking();

            // Lọc theo trạng thái gửi (Success / Failed)
            if (!string.IsNullOrWhiteSpace(filter.SentStatus) && filter.SentStatus != "string")
            {
                string status = filter.SentStatus.Trim().ToLower();
                query = query.Where(x => x.SentStatus.ToLower() == status);
            }

            // Lọc theo loại đối tượng tham chiếu (HealthCheckupRecord, ForgotPassword, System)
            if (!string.IsNullOrWhiteSpace(filter.ReferenceType) && filter.ReferenceType != "string")
            {
                string refType = filter.ReferenceType.Trim().ToLower();
                query = query.Where(x => x.ReferenceType != null && x.ReferenceType.ToLower() == refType);
            }

            // Lọc theo ID bản ghi liên kết
            if (filter.ReferenceId.HasValue && filter.ReferenceId.Value > 0)
            {
                query = query.Where(x => x.ReferenceId == filter.ReferenceId.Value);
            }

            // Lọc theo khoảng thời gian gửi
            if (filter.FromDate.HasValue)
            {
                query = query.Where(x => x.CreatedDate >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(x => x.CreatedDate <= filter.ToDate.Value);
            }

            // Tìm kiếm theo mã log, email người nhận hoặc tiêu đề
            if (!string.IsNullOrWhiteSpace(filter.TextSearch) && filter.TextSearch != "string")
            {
                string ts = filter.TextSearch.Trim().ToLower();
                query = query.Where(x =>
                    x.Code.ToLower().Contains(ts) ||
                    x.ToEmail.ToLower().Contains(ts) ||
                    x.Subject.ToLower().Contains(ts));
            }

            // Phân trang
            if (filter.PageSize <= 0) filter.PageSize = 10;
            if (filter.PageNumber <= 0) filter.PageNumber = 1;

            int totalCount = await query.CountAsync(cancellationToken);
            int excludedRows = (filter.PageNumber - 1) * filter.PageSize;

            var listData = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip(excludedRows)
                .Take(filter.PageSize)
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
                .ToListAsync(cancellationToken);

            return new PaginationList<EmailLogModel>
            {
                Data = listData,
                DataCount = listData.Count,
                TotalCount = totalCount,
                PageSize = filter.PageSize,
                PageNumber = filter.PageNumber
            };
        }
    }
}
