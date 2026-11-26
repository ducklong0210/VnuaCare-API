/**
 * Controller Tra cứu Nhật ký gửi Email (EmailLogs)
 * Phục vụ Quản trị viên kiểm tra lịch sử gửi cảnh báo sức khỏe và lỗi SMTP
 */

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.EmailLogs;
using VnuaCare.Business.Business.EmailLogs.EmailLogQueries;
using VnuaCare.Shared.Helper;
using VnuaCare.Shared.Utils;

namespace VnuaCare.API.Controllers.Business;

/// <summary>
/// Quản lý và tra cứu lịch sử gửi Email thông báo của hệ thống
/// </summary>
[ApiController]
[Route("api/v1/emaillogs")]
[ApiExplorerSettings(GroupName = "Nhật ký gửi Email")]
[Authorize]
public class EmailLogController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public EmailLogController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lọc, tìm kiếm và phân trang danh sách Email đã gửi
    /// </summary>
    /// <param name="filter">Điều kiện lọc theo trạng thái, loại tham chiếu, ngày gửi hoặc từ khóa</param>
    /// <returns>Danh sách nhật ký gửi Email phân trang</returns>
    [HttpPost, Route("filter")]
    [ProducesResponseType(typeof(ResponseObject<PaginationList<EmailLogModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Filter([FromBody] EmailLogFilterModel filter)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetFilterEmailLogQuery(filter));
        });
    }

    /// <summary>
    /// Xem chi tiết một bản ghi nhật ký gửi Email theo ID
    /// </summary>
    /// <param name="id">ID nhật ký gửi Email (emailLogId)</param>
    /// <returns>Chi tiết email đã gửi kèm nội dung HTML và thông báo lỗi nếu có</returns>
    [HttpGet, Route("{id}")]
    [ProducesResponseType(typeof(ResponseObject<EmailLogModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetEmailLogByIdQuery(id));
        });
    }
}
