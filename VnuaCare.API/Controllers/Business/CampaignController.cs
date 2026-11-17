/**
 * Controller Quản lý Đợt khám sức khỏe định kỳ (Campaigns)
 * Độc lập, tách biệt khỏi Hồ sơ khám sức khỏe theo chuẩn Clean Architecture
 */

using MediatR;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.Campaigns;
using VnuaCare.Business.Business.Campaigns.CampaignCommands;
using VnuaCare.Business.Business.Campaigns.CampaignQueries;
using VnuaCare.Shared.Helper;
using VnuaCare.Shared.Utils;

namespace VnuaCare.API.Controllers.Business;

/// <summary>
/// Quản lý kế hoạch các đợt khám sức khỏe định kỳ hàng năm cho cán bộ
/// </summary>
[ApiController]
[Route("api/v1/campaigns")]
[ApiExplorerSettings(GroupName = "Quản lý đợt khám")]
public class CampaignController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CampaignController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy chi tiết Đợt khám sức khỏe theo ID (kèm thống kê số hồ sơ)
    /// </summary>
    /// <param name="id">ID đợt khám</param>
    /// <returns>Thông tin chi tiết đợt khám</returns>
    [HttpGet, Route("{id}")]
    [ProducesResponseType(typeof(ResponseObject<CampaignDetailModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetCampaignByIdIndex(id));
        });
    }

    /// <summary>
    /// Lấy danh sách Đợt khám sức khỏe dạng danh mục chọn (Combobox / Dropdown)
    /// </summary>
    /// <param name="year">Năm tổ chức khám (tùy chọn lọc)</param>
    /// <param name="status">Trạng thái đợt khám (tùy chọn lọc)</param>
    /// <returns>Danh sách đợt khám rút gọn</returns>
    [HttpGet, Route("select")]
    [ProducesResponseType(typeof(ResponseObject<List<CampaignSelectModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampaignsSelect([FromQuery] int? year, [FromQuery] string? status)
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
