/**
 * Giao diện dịch vụ băm và xác thực mật khẩu
 */

namespace VnuaCare.Business.Services;

/// <summary>
/// Dịch vụ xử lý mã hóa và xác thực mật khẩu người dùng
/// </summary>
public interface IBcryptPasswordHasher
{
    /// <summary>
    /// Băm mật khẩu dạng văn bản thô sang chuỗi hash an toàn
    /// </summary>
    /// <param name="password">Mật khẩu gốc</param>
    /// <returns>Chuỗi mật khẩu đã băm</returns>
    string HashPassword(string password);

    /// <summary>
    /// Kiểm tra đối chiếu mật khẩu thô với mật khẩu đã băm
    /// </summary>
    /// <param name="password">Mật khẩu người dùng nhập</param>
    /// <param name="hashedPassword">Mật khẩu đã băm lưu trong CSDL</param>
    /// <returns>True nếu trùng khớp, ngược lại False</returns>
    bool VerifyPassword(string password, string hashedPassword);
}
