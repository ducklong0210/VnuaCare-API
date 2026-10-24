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
using VnuaCare.Shared.Utils;

namespace VnuaCare.API.Controllers.Business;

/// <summary>
/// Quản lý thông tin tài khoản và hồ sơ người dùng
/// </summary>
[ApiController]
[Route("api/v1/users")]
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
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangePassword([FromBody] UpdatePasswordUserModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new ChangePassUserCommand(model));
        });
    }

    /// <summary>
    /// Cập nhật thông tin hồ sơ cá nhân (SĐT, Địa chỉ, Avatar, Email...)
    /// </summary>
    /// <param name="model">Thông tin cập nhật</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateMyProfileModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new UpdateMyProfileCommand(model));
        });
    }

    /// <summary>
    /// Lấy danh sách Tài khoản người dùng dạng danh mục chọn (Combobox / Dropdown)
    /// </summary>
    /// <param name="role">Vai trò cần lọc (tùy chọn: SUPER_ADMIN, HEALTH_ADMIN, STAFF, DOCTOR)</param>
    /// <param name="isActive">Trạng thái kích hoạt (mặc định: true)</param>
    /// <returns>Danh sách tài khoản rút gọn</returns>
    [HttpGet("select")]
    [ProducesResponseType(typeof(ResponseObject<List<UserSelectModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSelect([FromQuery] string? role, [FromQuery] bool? isActive = true)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetComboxUserQuery(role, isActive));
        });
    }

    /// <summary>
    /// Lấy danh sách các Vai trò phân quyền trong hệ thống (Combobox / Dropdown)
    /// </summary>
    /// <returns>Danh sách các vai trò</returns>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(ResponseObject<List<RoleSelectModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles()
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetComboxRoleQuery());
        });
    }
}

