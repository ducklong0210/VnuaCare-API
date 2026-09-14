/**
 * Tiện ích kiểm tra tính hợp lệ của dữ liệu (Validation Helpers)
 */

using System.Text.RegularExpressions;

namespace VnuaCare.Shared.Utils;

/// <summary>
/// Các hàm tiện ích kiểm tra tính hợp lệ của Email, Username và Mật khẩu
/// </summary>
public class ValidationUtils
{
    // Biểu thức Regex kiểm tra định dạng Email chuẩn
    private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    // Biểu thức Regex kiểm tra mật khẩu: tối thiểu 6 ký tự, gồm ít nhất 1 chữ cái và 1 chữ số
    private const string PasswordPattern = @"^(?=.*[A-Za-z])(?=.*\d).{6,}$";
    
    /// <summary>
    /// Kiểm tra chuỗi có phải là địa chỉ Email đúng chuẩn không
    /// </summary>
    /// <param name="email">Chuỗi email cần kiểm tra</param>
    /// <returns>True nếu hợp lệ, False nếu không hợp lệ</returns>
    public static bool IsEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return Regex.IsMatch(email, EmailPattern);
    }

    /// <summary>
    /// Kiểm tra tên đăng nhập (không chứa khoảng trắng và dài hơn 3 ký tự)
    /// </summary>
    /// <param name="username">Chuỗi username cần kiểm tra</param>
    /// <returns>True nếu hợp lệ, False nếu không hợp lệ</returns>
    public static bool IsUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;
        return !username.Contains(" ") && username.Trim().Length > 3;
    }
    
    /// <summary>
    /// Kiểm tra mật khẩu có hợp lệ không (Tối thiểu 6 ký tự, gồm ít nhất 1 chữ cái và 1 chữ số)
    /// </summary>
    /// <param name="password">Chuỗi mật khẩu cần kiểm tra</param>
    /// <returns>True nếu hợp lệ, False nếu không hợp lệ</returns>
    public static bool IsPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        return Regex.IsMatch(password, PasswordPattern);
    }
}
