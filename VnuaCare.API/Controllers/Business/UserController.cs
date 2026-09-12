/**
 * Quản lý hồ sơ cá nhân và đổi mật khẩu người dùng
 */

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.Users;
using VnuaCare.Business.Business.Users.UserCommands;
using VnuaCare.Business.Business.Users.UserQueries;
using VnuaCare.Shared.Helper;

namespace VnuaCare.API.Controllers.Business;

/// <summary>
/// Quản lý thông tin tài khoản và hồ sơ người dùng
/// </summary>
[ApiController]
[Route("api/v1/user")]
[ApiExplorerSettings(GroupName = "Tài khoản & Hồ sơ cá nhân")]
[Authorize] // Bắt buộc người dùng phải đăng nhập và gửi kèm JWT Bearer Token
public class UserController : ApiControllerBase
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
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetMyProfileQuery());
        });
    }

    /// <summary>
    /// Đổi mật khẩu tài khoản đang đăng nhập
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] UpdatePasswordUserModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new ChangePassUserCommand(model));
        });
    }
}
