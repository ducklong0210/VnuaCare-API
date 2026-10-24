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

    /// <summary>
    /// Lọc và phân trang danh sách tài khoản người dùng
    /// </summary>
    /// <param name="filter">Điều kiện lọc và phân trang</param>
    /// <returns>Danh sách tài khoản phân trang</returns>
    [HttpPost("filter")]
    [ProducesResponseType(typeof(ResponseObject<PaginationList<UserModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Filter([FromBody] UserFilterModel filter)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetFilterUserQuery(filter));
        });
    }

    /// <summary>
    /// Lấy chi tiết thông tin tài khoản người dùng theo ID
    /// </summary>
    /// <param name="id">ID tài khoản người dùng</param>
    /// <returns>Thông tin chi tiết tài khoản</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseObject<UserModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetUserByIdIndex(id));
        });
    }

    /// <summary>
    /// Thêm mới tài khoản người dùng (Dành cho Quản trị viên)
    /// </summary>
    /// <param name="model">Thông tin tài khoản mới</param>
    /// <returns>Kết quả thêm mới</returns>
    [HttpPost("add")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateUserModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new CreateUserCommand(model));
        });
    }

    /// <summary>
    /// Cập nhật thông tin tài khoản người dùng (Dành cho Quản trị viên)
    /// </summary>
    /// <param name="model">Thông tin cập nhật</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPut("update")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdateUserModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new UpdateUserCommand(model));
        });
    }

    /// <summary>
    /// Vô hiệu hóa / Khóa tài khoản người dùng
    /// </summary>
    /// <param name="id">ID tài khoản cần khóa</param>
    /// <returns>Kết quả khóa</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new DeleteUserCommand(id));
        });
    }

    /// <summary>
    /// Đặt lại mật khẩu tài khoản người dùng (Dành cho Quản trị viên)
    /// </summary>
    /// <param name="id">ID người dùng</param>
    /// <param name="model">Mật khẩu mới</param>
    /// <returns>Kết quả đặt lại mật khẩu</returns>
    [HttpPost("{id}/reset-password")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword([FromRoute] int id, [FromBody] ResetPasswordAdminModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            var commandModel = model with { UserId = id };
            return await _mediator.Send(new ResetPasswordUserCommand(commandModel));
        });
    }
}


