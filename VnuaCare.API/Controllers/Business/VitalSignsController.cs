/**
 * Controller Quản lý Chỉ số sinh tồn & Thể lực ban đầu (Vital Signs)
 * Chuẩn CRUD: Create, Update, Delete, Read tách bạch hoàn toàn
 */

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.VitalSigns;
using VnuaCare.Business.Business.VitalSigns.VitalSignsCommands;
using VnuaCare.Business.Business.VitalSigns.VitalSignsQueries;
using VnuaCare.Shared.Helper;
using VnuaCare.Shared.Utils;

namespace VnuaCare.API.Controllers.Business;

/// <summary>
/// Quản lý dữ liệu đo lường thể lực và các chỉ số sinh tồn tại bàn đón tiếp ban đầu
/// </summary>
[ApiController]
[Route("api/v1/vitalsigns")]
[ApiExplorerSettings(GroupName = "Khám thể lực & Sinh tồn")]
[Authorize]
public class VitalSignsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public VitalSignsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Thêm mới chỉ số sinh tồn và thể lực (Create)
    /// </summary>
    /// <param name="model">Dữ liệu chỉ số sinh tồn và thể lực ban đầu</param>
    /// <returns>Kết quả thêm mới</returns>
    [HttpPost, Route("add")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateVitalSignsModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new CreateVitalSignsCommand(model));
        });
    }

    /// <summary>
    /// Cập nhật chỉ số sinh tồn và thể lực (Update)
    /// </summary>
    /// <param name="model">Dữ liệu cập nhật chỉ số sinh tồn</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPut, Route("update")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdateVitalSignsModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new UpdateVitalSignsCommand(model));
        });
    }

    /// <summary>
    /// Xóa chỉ số sinh tồn (Delete)
    /// </summary>
    /// <param name="id">ID bản ghi chỉ số sinh tồn (vitalSignId)</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete, Route("{id}")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new DeleteVitalSignsCommand(id));
        });
    }

    /// <summary>
    /// Lấy chỉ số sinh tồn và thể lực theo ID hồ sơ kèm đánh giá chuẩn WHO (Read)
    /// </summary>
    /// <param name="recordId">ID hồ sơ khám sức khỏe</param>
    /// <returns>Chi tiết chỉ số sinh tồn, thể lực và phân loại</returns>
    [HttpGet, Route("{recordId}")]
    [ProducesResponseType(typeof(ResponseObject<VitalSignsModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByRecordId([FromRoute] int recordId)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetVitalSignsByRecordIdQuery(recordId));
        });
    }
}
