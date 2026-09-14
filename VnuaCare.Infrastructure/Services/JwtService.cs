/**
 * Dịch vụ xử lý JWT: Tạo Access Token, tạo Refresh Token ngẫu nhiên và giải mã Token
 */

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using VnuaCare.Business.Services;

namespace VnuaCare.Infrastructure.Services;

/// <summary>
/// Dịch vụ quản lý và sinh mã JWT (JSON Web Token) cho hệ thống V-Care Health
/// </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Sinh JWT Access Token chứa danh sách quyền hạn và danh tính người dùng (Claims)
    /// </summary>
    /// <param name="claims">Danh sách thông tin định danh (UserId, Username, Email, Role)</param>
    /// <returns>Chuỗi JWT Token mã hóa HMAC-SHA256</returns>
    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var secretKey = _configuration["JwtSettings:Secret"] ?? "VnuaCareHealthSecretKey2026SuperSecureKeyDefault123456!";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "VnuaCare";
        var audience = _configuration["JwtSettings:Audience"] ?? "VnuaCareClient";
        var expirationMinutes = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "120");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Sinh chuỗi Refresh Token ngẫu nhiên 32 bytes an toàn bằng thuật toán mật mã học
    /// </summary>
    /// <returns>Chuỗi Base64 Refresh Token</returns>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Trích xuất thông tin ClaimsPrincipal từ Access Token đã hết hạn để phục vụ cấp mới Token
    /// </summary>
    /// <param name="token">Access Token cũ đã hết hạn</param>
    /// <returns>Đối tượng ClaimsPrincipal chứa thông tin người dùng</returns>
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var secretKey = _configuration["JwtSettings:Secret"] ?? "VnuaCareHealthSecretKey2026SuperSecureKeyDefault123456!";
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Token không hợp lệ.");
        }

        return principal;
    }
}
