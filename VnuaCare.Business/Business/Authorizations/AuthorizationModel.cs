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

