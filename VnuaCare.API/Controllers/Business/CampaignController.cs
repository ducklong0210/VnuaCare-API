/**
 * Controller Quản lý Đợt khám sức khỏe định kỳ (Campaigns)
 */

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.Campaigns;
using VnuaCare.Business.Business.Campaigns.CampaignCommands;
using VnuaCare.Business.Business.Campaigns.CampaignQueries;
using VnuaCare.Shared.Helper;
using VnuaCare.Shared.Utils;

namespace VnuaCare.API.Controllers.Business;

/// <summary>
/// Quản lý các đợt khám sức khỏe định kỳ hàng năm của cán bộ Học viện Nông nghiệp
/// </summary>
[ApiController]
[Route("api/v1/campaigns")]
[ApiExplorerSettings(GroupName = "Quản lý đợt khám sức khỏe")]
// [Authorize]
public class CampaignController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CampaignController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy thông tin chi tiết đợt khám sức khỏe theo ID
    /// </summary>
    /// <param name="id">ID đợt khám sức khỏe</param>
    /// <returns>Thông tin chi tiết đợt khám</returns>
    [HttpGet, Route("{id}")]
    [ProducesResponseType(typeof(ResponseObject<CampaignModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetCampaignByIdIndex(id));
        });
    }

    /// <summary>
    /// Lọc, tìm kiếm và phân trang danh sách các đợt khám sức khỏe
    /// </summary>
    /// <param name="filter">Điều kiện tìm kiếm, lọc theo năm, trạng thái, bệnh viện đối tác</param>
    /// <returns>Danh sách đợt khám phân trang</returns>
    [HttpPost, Route("filter")]
    [ProducesResponseType(typeof(ResponseObject<PaginationList<CampaignModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Filter([FromBody] CampaignFilterModel filter)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetFilterCampaignQuery(filter));
        });
    }

    /// <summary>
    /// Lấy danh sách Đợt khám sức khỏe phục vụ hiển thị Combobox / Dropdown
    /// </summary>
    /// <param name="year">Lọc theo năm tổ chức (tùy chọn)</param>
    /// <param name="status">Lọc theo trạng thái (PLANNING, IN_PROGRESS, COMPLETED)</param>
    /// <returns>Danh sách đợt khám rút gọn</returns>
    [HttpGet, Route("select")]
    [ProducesResponseType(typeof(ResponseObject<List<CampaignSelectModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSelect([FromQuery] int? year, [FromQuery] string? status)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetComboxCampaignQuery(year, status));
        });
    }

    /// <summary>
    /// Thêm mới Đợt khám sức khỏe định kỳ (Dành cho Quản trị viên)
    /// </summary>
    /// <param name="model">Thông tin đợt khám mới</param>
    /// <returns>Kết quả thêm mới</returns>
    [HttpPost, Route("add")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateCampaignModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new CreateCampaignCommand(model));
        });
    }

    /// <summary>
    /// Cập nhật thông tin Đợt khám sức khỏe định kỳ (Dành cho Quản trị viên)
    /// </summary>
    /// <param name="model">Thông tin cập nhật</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPut, Route("update")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdateCampaignModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new UpdateCampaignCommand(model));
        });
    }

    /// <summary>
    /// Xóa Đợt khám sức khỏe định kỳ (chỉ cho phép khi chưa có hồ sơ liên kết)
    /// </summary>
    /// <param name="id">ID đợt khám cần xóa</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete, Route("{id}")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new DeleteCampaignCommand(id));
        });
    }
}
