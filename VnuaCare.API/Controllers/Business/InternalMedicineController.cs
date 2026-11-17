/**
 * Controller Quản lý Khám Lâm sàng Nội khoa tổng quát (Internal Medicine)
 * Chuẩn CRUD: Create, Update, Delete, Read tách bạch hoàn toàn
 */

using MediatR;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.InternalMedicine;
using VnuaCare.Business.Business.InternalMedicine.InternalMedicineCommands;
using VnuaCare.Business.Business.InternalMedicine.InternalMedicineQueries;
using VnuaCare.Shared.Helper;
using VnuaCare.Shared.Utils;

namespace VnuaCare.API.Controllers.Business;

/// <summary>
/// Quản lý kết quả khám lâm sàng Nội khoa tổng quát 6 hệ cơ quan (Tuần hoàn, Hô hấp, Tiêu hóa, Tiết niệu, Cơ xương khớp, Thần kinh)
/// </summary>
[ApiController]
[Route("api/v1/internalmedicines")]
[ApiExplorerSettings(GroupName = "Khám nội khoa tổng quát")]
public class InternalMedicineController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public InternalMedicineController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Thêm mới kết quả khám lâm sàng Nội khoa tổng quát (Create)
    /// </summary>
    /// <param name="model">Dữ liệu khám lâm sàng nội khoa 6 hệ cơ quan</param>
    /// <returns>Kết quả thêm mới</returns>
    [HttpPost, Route("add")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateInternalMedicineExamModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new CreateInternalMedicineExamCommand(model));
        });
    }

    /// <summary>
    /// Cập nhật kết quả khám lâm sàng Nội khoa tổng quát (Update)
    /// </summary>
    /// <param name="model">Dữ liệu cập nhật kết quả khám nội khoa</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPut, Route("update")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdateInternalMedicineExamModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new UpdateInternalMedicineExamCommand(model));
        });
    }

    /// <summary>
    /// Xóa kết quả khám lâm sàng Nội khoa (Delete)
    /// </summary>
    /// <param name="id">ID kết quả khám nội khoa cần xóa (internalExamId)</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete, Route("{id}")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new DeleteInternalMedicineExamCommand(id));
        });
    }

    /// <summary>
    /// Lấy chi tiết kết quả khám lâm sàng Nội khoa theo ID hồ sơ (Read)
    /// </summary>
    /// <param name="recordId">ID hồ sơ khám sức khỏe</param>
    /// <returns>Chi tiết kết quả khám lâm sàng nội khoa</returns>
    [HttpGet, Route("{recordId}")]
    [ProducesResponseType(typeof(ResponseObject<InternalMedicineExamModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByRecordId([FromRoute] int recordId)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetInternalMedicineExamByRecordIdQuery(recordId));
        });
    }
}
