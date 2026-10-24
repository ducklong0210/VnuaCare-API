/**
 * Quản lý xác thực người dùng (Đăng nhập, Làm mới Token, Đăng xuất)
 */

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.Authorizations;
using VnuaCare.Business.Business.Authorizations.AuthorizationCommands;
using VnuaCare.Business.Business.Authorizations.AuthorizationQueries;
using VnuaCare.Shared.Helper;
using VnuaCare.Shared.Utils;

namespace VnuaCare.API.Controllers.Business;

[ApiController]
[Route("api/v1/authorization")]
[ApiExplorerSettings(GroupName = "Xác thực người dùng")]
public class AuthorizationController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AuthorizationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Đăng nhập hệ thống
    /// </summary>
    /// <param name="model">Thông tin đăng nhập</param>
    /// <returns>Token và thông tin người dùng</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ResponseObject<LoginResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new UserLoginCommand(model));
        });
    }

    /// <summary>
    /// Làm mới Access Token
    /// </summary>
    /// <param name="model">Access Token cũ và Refresh Token</param>
    /// <returns>Token mới</returns>
    [HttpPost("refreshtoken")]
    [ProducesResponseType(typeof(ResponseObject<LoginResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new RefreshTokenCommand(model));
        });
    }

    /// <summary>
    /// Đăng xuất hệ thống
    /// </summary>
    /// <param name="model">Refresh Token cần vô hiệu hóa</param>
    /// <returns>Kết quả đăng xuất</returns>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] LogoutModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new UserLogoutCommand(model));
        });
    }

    /// <summary>
    /// Yêu cầu gửi mã OTP đặt lại mật khẩu qua email
    /// </summary>
    /// <param name="model">Email tài khoản</param>
    /// <returns>Thông báo xác nhận</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ResponseObject<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new ForgotPasswordCommand(model));
        });
    }

    /// <summary>
    /// Xác nhận mã OTP và đặt lại mật khẩu mới
    /// </summary>
    /// <param name="model">Thông tin mã OTP và mật khẩu mới</param>
    /// <returns>Kết quả đặt lại mật khẩu</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new ResetPasswordWithTokenCommand(model));
        });
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của Token hiện tại và lấy thông tin phiên làm việc
    /// </summary>
    /// <returns>Thông tin người dùng hiện tại</returns>
    [HttpGet("check-token")]
    [Authorize]
    [ProducesResponseType(typeof(ResponseObject<CheckTokenResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckToken()
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new CheckTokenQuery());
        });
    }
}

