using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace VnuaCare.Shared.ContextAccessor;

/// <summary>
/// Class trung gian (Wrapper) bọc quanh IHttpContextAccessor của ASP.NET Core.
/// Mục đích: Giúp các tầng nghiệp vụ (Business, DataContext, Audit Log) lấy được thông tin
/// của người dùng đang đăng nhập (UserId, Username, Role...) từ JWT Bearer Token
/// mà không làm tầng Business bị phụ thuộc trực tiếp vào thư viện Web (Microsoft.AspNetCore.Http).
/// </summary>
public class HttpContextAccessorWrapper : IContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _traceId;

    public HttpContextAccessorWrapper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        // Khởi tạo một mã Trace ID duy nhất cho mỗi vòng đời request
        _traceId = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Mã tương quan (Correlation ID) dùng để liên kết các luồng log của cùng 1 request từ Client gửi lên.
    /// Nếu Client có truyền Header 'X-Correlation-Id' thì lấy mã đó, ngược lại dùng TraceId tự sinh.
    /// </summary>
    public string CorrelationId
    {
        get
        {
            var correlationId = _httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-Id"].ToString();
            return string.IsNullOrEmpty(correlationId) ? _traceId : correlationId;
        }
    }

    /// <summary>
    /// Mã định danh vết (Trace ID) theo dõi hành trình của request qua các Controller, Handler, Database.
    /// </summary>
    public string TraceId => _traceId;

    /// <summary>
    /// Lấy ID của người dùng đang thực hiện request từ Claims trong JWT Token (NameIdentifier / sub / nameid).
    /// Trả về null nếu request là của khách vãng lai (chưa đăng nhập) hoặc Token không hợp lệ.
    /// </summary>
    public int? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }

    /// <summary>
    /// Tên đăng nhập của tài khoản hiện tại được trích xuất từ Token
    /// </summary>
    public string UserName
    {
        get => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Vai trò của người dùng (SUPER_ADMIN, HEALTH_ADMIN, DOCTOR, STAFF) lấy từ Claims trong Token
    /// </summary>
    public string Role
    {
        get => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
    }

    /// <summary>
    /// Ngôn ngữ yêu cầu của Client lấy từ Header 'Accept-Language' (Mặc định là 'vi-VN')
    /// </summary>
    public string Language => _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString() ?? "vi-VN";
}
