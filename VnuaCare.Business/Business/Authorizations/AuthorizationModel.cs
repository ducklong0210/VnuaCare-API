/**
 * Định nghĩa các DTO truyền nhận dữ liệu cho phân hệ Xác thực (Login, Token, Logout)
 */

using System.ComponentModel.DataAnnotations;

namespace VnuaCare.Business.Business.Authorizations;


    // base cho các request xác thực
    public abstract record AuthorizationBaseModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không đưụoc để trống")]
        public string LoginIdentifier { get; init; }
       [Required(ErrorMessage = "Mật khẩu không dđược để trống")] 
       public string Password { get; init; }
    }

    // trả về thông tin
    public abstract record ResponseBaseModel
    {
        public int UserId { get; init; }
        public string Username { get; init; }
        public string? Fullname { get; init; }
        public string? Email { get; init; }
        public string Role { get; init; }
        public string? StaffCode { get; init; }
        public string? DoctorCode { get; init; }
        public string? AvatarUrl { get; init; }
    }

    public record LoginModel : AuthorizationBaseModel
    {
    }
    
    //đăng nhập thành công
    public record LoginResponseModel : ResponseBaseModel
    {
        public string AccessToken { get; init; }
        public string RefreshToken { get; init; }
        public DateTime RefreshTokenExpiryTime { get; init; }
    }
    
    // request token
    public record RefreshTokenModel
    {
        [Required] public string AccessToken { get; init; }
        [Required] public string RefreshToken { get; init; }
    }

    // logout
    public record LogoutModel
    {
        [Required] public string RefreshToken { get; init; }
    }
    
    public record CheckTokenResponseModel : ResponseBaseModel
    {
    }

    /// <summary>
    /// Model yêu cầu gửi mã xác nhận quên mật khẩu qua Email
    /// </summary>
    public record ForgotPasswordModel
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; init; } = string.Empty;
    }

    /// <summary>
    /// Model đặt lại mật khẩu mới sau khi xác thực
    /// </summary>
    public record ResetPasswordRequestModel
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; init; } = string.Empty;

        [Required(ErrorMessage = "Mã xác thực không được để trống")]
        public string TokenOrOtp { get; init; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        public string NewPassword { get; init; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        public string ConfirmPassword { get; init; } = string.Empty;
    }


