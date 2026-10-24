/**
 * Controller Quản lý Hồ sơ khám sức khỏe định kỳ (Health Checkup Records)
 */

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.HealthCheckupRecords;
using VnuaCare.Business.Business.HealthCheckupRecords.CheckupRecordCommands;
using VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordQueries;
using VnuaCare.Business.Business.Staffs;
using VnuaCare.Business.Business.Staffs.StaffQueries;
using VnuaCare.Business.Business.Doctors;
using VnuaCare.Business.Business.Doctors.DoctorQueries;
using VnuaCare.Shared.Helper;
using VnuaCare.Shared.Utils;


namespace VnuaCare.API.Controllers.Business;

/// <summary>
/// Quản lý hồ sơ khám sức khỏe tổng hợp của cán bộ Học viện Nông nghiệp
/// </summary>
[ApiController]
[Route("api/v1/healthcheckuprecords")]
[ApiExplorerSettings(GroupName = "Thông tin chung bệnh án")]
// [Authorize]
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
    /// Thêm mới hồ sơ khám sức khỏe tổng hợp của cán bộ
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
    /// Cập nhật kết luận, phân loại sức khỏe và thông tin hồ sơ khám
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
    /// Lấy danh sách Đợt khám sức khỏe dạng danh mục chọn (Combobox / Dropdown)
    /// </summary>
    /// <param name="year">Năm tổ chức khám (tùy chọn lọc)</param>
    /// <param name="status">Trạng thái đợt khám (tùy chọn lọc)</param>
    /// <returns>Danh sách đợt khám rút gọn</returns>
    [HttpGet, Route("campaigns")]
    [ProducesResponseType(typeof(ResponseObject<List<CampaignSelectModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampaigns([FromQuery] int? year, [FromQuery] string? status)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetComboxCampaignQuery(year, status));
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

    /// <summary>
    /// Lấy danh sách Cán bộ phục vụ chọn trong hồ sơ khám (Combobox / Dropdown)
    /// </summary>
    /// <param name="departmentId">ID Khoa / Phòng ban cần lọc</param>
    /// <returns>Danh sách cán bộ rút gọn</returns>
    [HttpGet, Route("staffs")]
    [ProducesResponseType(typeof(ResponseObject<List<StaffSelectModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStaffs([FromQuery] int? departmentId)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetComboxStaffQuery(departmentId));
        });
    }

    /// <summary>
    /// Lấy danh sách Bác sĩ kết luận hồ sơ khám (Combobox / Dropdown)
    /// </summary>
    /// <param name="specialtyId">ID Chuyên khoa y tế cần lọc</param>
    /// <returns>Danh sách bác sĩ rút gọn</returns>
    [HttpGet, Route("doctors")]
    [ProducesResponseType(typeof(ResponseObject<List<DoctorSelectModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDoctors([FromQuery] int? specialtyId)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetComboxDocTorQuery(specialtyId));
        });
    }

    /// <summary>
    /// Lấy danh sách Khoa / Phòng ban phục vụ lọc hồ sơ khám (Combobox / Dropdown)
    /// </summary>
    /// <returns>Danh sách khoa phòng ban rút gọn</returns>
    [HttpGet, Route("departments")]
    [ProducesResponseType(typeof(ResponseObject<List<DepartmentSelectModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments()
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetComboxDepartmentQuery());
        });
    }
}