/**
 * Quản lý Bác sĩ BVĐK MEDLATEC và Bác sĩ Trạm Y tế
 */

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.Doctors;
using VnuaCare.Business.Business.Doctors.DoctorCommands;
using VnuaCare.Business.Business.Doctors.DoctorQueries;
using VnuaCare.Shared.Helper;
using VnuaCare.Shared.Utils;

namespace VnuaCare.API.Controllers.Business;

/// <summary>
/// Quản lý hồ sơ Bác sĩ và chuyên khoa y tế
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "Quản lý bác sĩ")]
// [Authorize]
public class DoctorController : ApiControllerBase
{
    private readonly IMediator _mediator;
    
    public DoctorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lọc và phân trang danh sách Bác sĩ
    /// </summary>
    /// <param name="model">Điều kiện lọc theo chuyên khoa hoặc từ khóa</param>
    /// <returns>Danh sách bác sĩ phân trang</returns>
    [HttpPost, Route("filter")]
    [ProducesResponseType(typeof(ResponseObject<PaginationList<DoctorModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Filter([FromBody] DoctorFilterModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetFilterDoctorQuery(model));
        });
    }

    /// <summary>
    /// Lấy thông tin chi tiết Bác sĩ theo ID
    /// </summary>
    /// <param name="id">ID của Bác sĩ</param>
    /// <returns>Thông tin chi tiết Bác sĩ</returns>
    [HttpPost, Route("{id}")]
    [ProducesResponseType(typeof(ResponseObject<DoctorModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetDoctorByIdIndex(id));
        });
    }
    
    /// <summary>
    /// Thêm mới Bác sĩ
    /// </summary>
    /// <param name="model">Thông tin bác sĩ và tài khoản đăng nhập</param>
    /// <returns>Kết quả thêm mới</returns>
    [HttpPost, Route("add")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateDoctorModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new CreateDoctorCommand(model));
        });
    }
    
    /// <summary>
    /// Cập nhật thông tin Bác sĩ
    /// </summary>
    /// <param name="model">Thông tin cập nhật</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPut, Route("update")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdateDoctorModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new UpdateDoctorCommand(model));
        });
    }
    
    /// <summary>
    /// Xóa Bác sĩ (vô hiệu hóa tài khoản)
    /// </summary>
    /// <param name="doctorId">ID của Bác sĩ cần xóa</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete, Route("{doctorId}")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] int doctorId)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new DeleteDoctorCommand(doctorId));
        });
    }
}
