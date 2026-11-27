/**
 * Controller Quản lý Xét nghiệm Cận lâm sàng (LabTests: Huyết học, Sinh hóa, Nước tiểu)
 */

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.LabTests;
using VnuaCare.Business.Business.LabTests.LabTestCommands;
using VnuaCare.Business.Business.LabTests.LabTestQueries;
using VnuaCare.Shared.Helper;
using VnuaCare.Shared.Utils;

namespace VnuaCare.API.Controllers.Business;

/// <summary>
/// Quản lý danh mục chỉ số xét nghiệm, phiếu chỉ định và kết quả xét nghiệm máu/nước tiểu
/// </summary>
[ApiController]
[Route("api/v1/labtests")]
[ApiExplorerSettings(GroupName = "Xét nghiệm Cận lâm sàng")]
[Authorize]
public class LabTestController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public LabTestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh mục các chỉ số xét nghiệm chuẩn kèm dải tham chiếu (Huyết học, Sinh hóa, Nước tiểu...)
    /// </summary>
    /// <param name="categoryGroup">Nhóm xét nghiệm tùy chọn (HEMATOLOGY, BIOCHEMISTRY, URINALYSIS, IMMUNOLOGY)</param>
    /// <returns>Danh sách chỉ số xét nghiệm</returns>
    [HttpGet, Route("indicators")]
    [ProducesResponseType(typeof(ResponseObject<List<LabIndicatorModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIndicators([FromQuery] string? categoryGroup)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetLabIndicatorsQuery(categoryGroup));
        });
    }

    /// <summary>
    /// Lấy toàn bộ phiếu chỉ định và kết quả xét nghiệm chi tiết theo ID hồ sơ khám
    /// </summary>
    /// <param name="recordId">ID hồ sơ khám sức khỏe</param>
    /// <returns>Danh sách phiếu xét nghiệm kèm danh sách chỉ số kết quả</returns>
    [HttpGet, Route("{recordId}")]
    [ProducesResponseType(typeof(ResponseObject<List<LabTestOrderModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByRecordId([FromRoute] int recordId)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetLabTestsByRecordIdQuery(recordId));
        });
    }

    /// <summary>
    /// Tạo mới phiếu chỉ định xét nghiệm cho hồ sơ khám
    /// </summary>
    /// <param name="model">Thông tin phiếu chỉ định (loại mẫu, mã vạch...)</param>
    /// <returns>ID phiếu chỉ định mới tạo (orderId)</returns>
    [HttpPost, Route("orders/add")]
    [ProducesResponseType(typeof(ResponseObject<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateLabTestOrderModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new CreateLabTestOrderCommand(model));
        });
    }

    /// <summary>
    /// Lưu / cập nhật hàng loạt kết quả chỉ số xét nghiệm cho phiếu chỉ định (tự động gắn cờ HIGH/LOW/NORMAL)
    /// </summary>
    /// <param name="model">OrderId và danh sách chỉ số kết quả</param>
    /// <returns>Kết quả lưu</returns>
    [HttpPost, Route("results/save")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveResults([FromBody] SaveLabTestResultsModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new SaveLabTestResultsCommand(model));
        });
    }

    /// <summary>
    /// Xóa phiếu chỉ định xét nghiệm (kèm các kết quả chi tiết bên trong)
    /// </summary>
    /// <param name="orderId">ID phiếu chỉ định cần xóa</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete, Route("orders/{orderId}")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteOrder([FromRoute] int orderId)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new DeleteLabTestOrderCommand(orderId));
        });
    }
}
