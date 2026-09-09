namespace VnuaCare.Shared.ContextAccessor;

/// <summary>
/// Interface trừu tượng hóa ngữ cảnh của phiên làm việc hiện tại (Current Request Context).
/// Cho phép các tầng nghiệp vụ (Business, Data, Services) truy cập danh tính người dùng
/// mà không bị ràng buộc trực tiếp vào hạ tầng Web của ASP.NET Core.
/// </summary>
public interface IContextAccessor
{
    /// <summary>
    /// Mã tương quan liên kết chuỗi Request giữa Client và Server
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Mã định danh luồng xử lý phục vụ truy vết và giám sát hệ thống (Tracing/Logging)
    /// </summary>
    string TraceId { get; }

    /// <summary>
    /// Khóa chính (UserId) của tài khoản đang đăng nhập trong CSDL
    /// </summary>
    int? UserId { get; }

    /// <summary>
    /// Tên đăng nhập của tài khoản
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Quyền hạn / Vai trò của tài khoản (SUPER_ADMIN, HEALTH_ADMIN, DOCTOR, STAFF)
    /// </summary>
    string? Role { get; }

    /// <summary>
    /// Ngôn ngữ của giao diện (ví dụ: vi-VN, en-US)
    /// </summary>
    string Language { get; }
}
