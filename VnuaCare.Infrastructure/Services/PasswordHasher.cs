/**
 * Dịch vụ mã hóa và xác thực mật khẩu người dùng bằng thuật toán BCrypt
 */

using VnuaCare.Business.Services;
using BCrypt.Net;

namespace VnuaCare.Infrastructure.Services;

/// <summary>
/// Triển khai dịch vụ băm mật khẩu chuẩn bảo mật quốc tế bằng thuật toán BCrypt
/// </summary>
public class PasswordHasher : IBcryptPasswordHasher
{
    /// <summary>
    /// Băm mật khẩu dạng văn bản thô (Plain Text) sang chuỗi Hash BCrypt an toàn
    /// </summary>
    /// <param name="password">Mật khẩu gốc do người dùng nhập</param>
    /// <returns>Chuỗi hash BCrypt kèm Salt tự sinh</returns>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Đối chiếu mật khẩu người dùng nhập lúc đăng nhập với chuỗi Hash đã lưu trong CSDL
    /// </summary>
    /// <param name="password">Mật khẩu thô do người dùng nhập</param>
    /// <param name="hashedPassword">Chuỗi hash lưu trong bảng vc_users</param>
    /// <returns>True nếu mật khẩu khớp, False nếu không khớp</returns>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch
        {
            return false;
        }
    }
}
