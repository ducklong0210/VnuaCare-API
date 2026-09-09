using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.Users;
using VnuaCare.Business.Business.Users.UserCommands;
using VnuaCare.Business.Business.Users.UserQueries;

namespace VnuaCare.API.Controllers.Business;

/// <summary>
/// Quản lý thông tin tài khoản và hồ sơ người dùng
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // Bắt buộc người dùng phải đăng nhập và gửi kèm JWT Bearer Token
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy thông tin cá nhân và hồ sơ chuyên môn của người đang đăng nhập
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var result = await _mediator.Send(new GetMyProfileQuery());
        return Ok(result);
    }

    /// <summary>
    /// Đổi mật khẩu tài khoản đang đăng nhập
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] UpdatePasswordUserModel model)
    {
        await _mediator.Send(new ChangePassUserCommand(model));
        return Ok(new { message = "Đổi mật khẩu thành công." });
    }
}
