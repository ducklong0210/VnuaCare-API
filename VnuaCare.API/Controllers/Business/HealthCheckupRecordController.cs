/**
 * Controller Quản lý Hồ sơ khám sức khỏe định kỳ (Health Checkup Records)
 * Chỉ tập trung vào quản lý phiếu khám tổng hợp, mã tiếp nhận SID, phân loại tổng thể và báo cáo y khoa
 */

using MediatR;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.HealthCheckupRecords;
using VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordCommands;
using VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordQueries;
using VnuaCare.Shared.Helper;
using VnuaCare.Shared.Utils;

namespace VnuaCare.API.Controllers.Business;

/// <summary>
/// Quản lý hồ sơ khám sức khỏe tổng hợp của cán bộ Học viện Nông nghiệp Việt Nam
/// </summary>
[ApiController]
[Route("api/v1/healthcheckuprecords")]
[ApiExplorerSettings(GroupName = "Thông tin chung bệnh án")]
public class HealthCheckupRecordController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public HealthCheckupRecordController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy thông tin chi tiết hồ sơ khám sức khỏe theo ID
    /// </summary>
    /// <param name="id">ID hồ sơ khám sức khỏe</param>
    /// <returns>Thông tin chi tiết hồ sơ</returns>
    [HttpGet, Route("{id}")]
    [ProducesResponseType(typeof(ResponseObject<HealthCheckupRecordModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int id)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetHealthCheckupRecordByIdIndex(id));
        });
    }

    /// <summary>
    /// Lọc và phân trang danh sách hồ sơ khám sức khỏe
    /// </summary>
    /// <param name="filter">Điều kiện lọc theo đợt khám, phòng ban, trạng thái hoặc từ khóa</param>
    /// <returns>Danh sách hồ sơ khám sức khỏe phân trang</returns>
    [HttpPost, Route("filter")]
    [ProducesResponseType(typeof(ResponseObject<PaginationList<HealthCheckupRecordModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Filter([FromBody] HealthCheckupRecordFilterModel filter)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetFilterHealthCheckupRecordQuery(filter));
        });
    }

    /// <summary>
    /// Thêm mới hồ sơ khám sức khỏe tổng hợp của cán bộ (Tiếp nhận ban đầu / Mã vạch SID)
    /// </summary>
    /// <param name="model">Dữ liệu hồ sơ khám sức khỏe mới</param>
    /// <returns>Kết quả thêm mới</returns>
    [HttpPost, Route("add")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateHealthCheckupRecordModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new CreateHealthCheckupRecordCommand(model));
        });
    }

    /// <summary>
    /// Cập nhật kết luận hội đồng bác sĩ, phân loại sức khỏe tổng thể và thông tin hồ sơ khám
    /// </summary>
    /// <param name="model">Dữ liệu cập nhật hồ sơ khám sức khỏe</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPut, Route("update")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdateHealthCheckupRecordModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new UpdateHealthCheckupRecordCommand(model));
        });
    }

    /// <summary>
    /// Xóa hồ sơ khám sức khỏe
    /// </summary>
    /// <param name="id">ID hồ sơ khám sức khỏe cần xóa</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete, Route("{id}")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new DeleteHealthCheckupRecordCommand(id));
        });
    }

    /// <summary>
    /// Lấy danh mục Phân loại sức khỏe theo tiêu chuẩn Bộ Y tế (Loại I đến Loại V)
    /// </summary>
    /// <returns>Danh mục phân loại sức khỏe</returns>
    [HttpGet, Route("classifications")]
    [ProducesResponseType(typeof(ResponseObject<List<HealthClassificationSelectModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClassifications()
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetComboxHealthClassificationQuery());
        });
    }

    /// <summary>
    /// Lấy danh mục Trạng thái hồ sơ khám sức khỏe (Combobox / Dropdown)
    /// </summary>
    /// <returns>Danh mục trạng thái hồ sơ</returns>
    [HttpGet, Route("statuses")]
    [ProducesResponseType(typeof(ResponseObject<List<RecordStatusSelectModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatuses()
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetComboxRecordStatusQuery());
        });
    }
}
