/**
 * Giao diện dịch vụ phát hành và xác thực JSON Web Token (JWT)
 */

using System.Security.Claims;

namespace VnuaCare.Business.Services;

/// <summary>
/// Dịch vụ quản lý JWT Access Token và Refresh Token cho hệ thống
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Tạo chuỗi JWT Access Token từ danh sách Claims
    /// </summary>
    /// <param name="claims">Danh sách thông tin định danh người dùng</param>
    /// <returns>Chuỗi JWT Token mã hóa</returns>
    string GenerateAccessToken(IEnumerable<Claim> claims);

    /// <summary>
    /// Sinh chuỗi Refresh Token ngẫu nhiên an toàn (64 bytes Base64)
    /// </summary>
    /// <returns>Chuỗi Refresh Token</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Giải mã và trích xuất thông tin Claims từ một Token đã hết hạn
    /// </summary>
    /// <param name="token">Chuỗi JWT Access Token đã hết hạn</param>
    /// <returns>Đối tượng ClaimsPrincipal chứa thông tin người dùng</returns>
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
