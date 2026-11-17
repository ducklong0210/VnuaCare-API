/**
 * Controller Quản lý Khám Chuyên khoa lẻ (Specialist Exams)
 * Mắt, Tai Mũi Họng, Răng Hàm Mặt, Da liễu, Ngoại khoa, Sản phụ khoa
 * Chuẩn CRUD: Create, Update, Delete, Read tách bạch hoàn toàn
 */

using MediatR;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.SpecialistExams;
using VnuaCare.Business.Business.SpecialistExams.SpecialistExamCommands;
using VnuaCare.Business.Business.SpecialistExams.SpecialistExamQueries;
using VnuaCare.Shared.Helper;
using VnuaCare.Shared.Utils;

namespace VnuaCare.API.Controllers.Business;

/// <summary>
/// Quản lý kết quả khám các chuyên khoa lẻ (Mắt, Tai Mũi Họng, Răng Hàm Mặt, Da liễu, Ngoại, Sản)
/// </summary>
[ApiController]
[Route("api/v1/specialistexams")]
[ApiExplorerSettings(GroupName = "Khám chuyên khoa lẻ")]
public class SpecialistExamController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public SpecialistExamController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Thêm mới kết quả khám chuyên khoa lẻ (Create)
    /// </summary>
    /// <param name="model">Dữ liệu khám chuyên khoa lẻ (Mắt, TMH, RHM, Da liễu, Ngoại, Sản)</param>
    /// <returns>Kết quả thêm mới</returns>
    [HttpPost, Route("add")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateSpecialistExamModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new CreateSpecialistExamCommand(model));
        });
    }

    /// <summary>
    /// Cập nhật kết quả khám chuyên khoa lẻ (Update)
    /// </summary>
    /// <param name="model">Dữ liệu cập nhật kết quả khám chuyên khoa</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPut, Route("update")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdateSpecialistExamModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new UpdateSpecialistExamCommand(model));
        });
    }

    /// <summary>
    /// Xóa kết quả khám chuyên khoa lẻ (Delete)
    /// </summary>
    /// <param name="id">ID kết quả khám chuyên khoa (specialistExamId)</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete, Route("{id}")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new DeleteSpecialistExamCommand(id));
        });
    }

    /// <summary>
    /// Lấy danh sách kết quả khám các chuyên khoa lẻ của hồ sơ (Read)
    /// </summary>
    /// <param name="recordId">ID hồ sơ khám sức khỏe</param>
    /// <param name="specialtyType">Mã chuyên khoa cần lọc (EYE, ENT, DENTAL, DERMATOLOGY, SURGERY, OBSTETRICS)</param>
    /// <returns>Danh sách kết quả khám chuyên khoa lẻ</returns>
    [HttpGet, Route("{recordId}")]
    [ProducesResponseType(typeof(ResponseObject<List<SpecialistExamModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromRoute] int recordId, [FromQuery] string? specialtyType)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetSpecialistExamsByRecordIdQuery(recordId, specialtyType));
        });
    }
}
