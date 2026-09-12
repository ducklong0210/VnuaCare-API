using System.Text.RegularExpressions;

namespace VnuaCare.Shared.Utils;

public class ValidationUtils
{
    // Regex cho Email chuẩn
    private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    // Regex mật khẩu
    // Tối thiểu 6 ký tự, gồm ít nhất 1 chữ cái và 1 chữ số
    private const string PasswordPattern = @"^(?=.*[A-Za-z])(?=.*\d).{6,}$";
    
    public static bool IsEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return Regex.IsMatch(email, EmailPattern);
    }

    public static bool IsUsername(string username)
    {
        if(string.IsNullOrWhiteSpace(username)) return  false;
        // Kiểm tra xem username có chứa khoảng trắng hay không và nhiều hơn 3 ký tự
        return !username.Contains(" ") && username.Trim().Length > 3;
    }
    
    /// <summary>
    /// Kiểm tra mật khẩu có hợp lệ không (Tối thiểu 6 ký tự, gồm chữ và số)
    /// </summary>
    public static bool IsPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        return Regex.IsMatch(password, PasswordPattern);
    }
}