/**
 * Quản lý Cán bộ Học viện Nông nghiệp
 */

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business;
using VnuaCare.Business.Business.Staffs;
using VnuaCare.Business.Business.Staffs.StaffCommands;
using VnuaCare.Business.Business.Staffs.StaffQueries;
using VnuaCare.Shared.Helper;
using VnuaCare.Shared.Utils;

namespace VnuaCare.API.Controllers.Business;

[ApiController]
[Route("v1/staff-manager")]
[ApiExplorerSettings(GroupName = "Quản lý nhân viên")]
[Authorize]
public class StaffController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public StaffController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Thêm mới cán bộ
    /// </summary>
    /// <param name="staff">Thông tin cán bộ</param>
    /// <returns></returns>
    [HttpPost, Route("add")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateStaffModel staff)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new CreateStaffCommand(staff));
        });
    }

    /// <summary>
    /// Cập nhật thông tin cán bộ
    /// </summary>
    /// <param name="staff">Thông tin cần cập nhật</param>
    /// <returns></returns>
    [HttpPut, Route("update")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdateStaffModel staff)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new UpdateStaffCommand(staff));
        });
    }

    /// <summary>
    /// Xóa cán bộ (khóa tài khoản)
    /// </summary>
    /// <param name="staffId">ID của cán bộ cần xóa</param>
    /// <returns></returns>
    [HttpDelete, Route("{staffId}")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] int staffId)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new DeleteStaffCommand(staffId));
        });
    }

    /// <summary>
    /// Lấy thông tin chi tiết cán bộ theo ID
    /// </summary>
    /// <param name="id">ID của cán bộ</param>
    /// <returns></returns>
    [HttpGet, Route("{id}")]
    [ProducesResponseType(typeof(ResponseObject<StaffModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetStaffByIdIndex(id));
        });
    }

    /// <summary>
    /// Lọc và phân trang danh sách cán bộ
    /// </summary>
    /// <param name="filter">Điều kiện tìm kiếm và phân trang</param>
    /// <returns></returns>
    [HttpPost, Route("filter")]
    [ProducesResponseType(typeof(ResponseObject<PaginationList<StaffModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Filter([FromBody] StaffFilterModel filter)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetFilterStaffQuery(filter));
        });
    }
}
