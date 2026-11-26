/**
 * Controller Quản lý Hồ sơ khám sức khỏe định kỳ (Health Checkup Records)
 * Chỉ tập trung vào quản lý phiếu khám tổng hợp, mã tiếp nhận SID, phân loại tổng thể và báo cáo y khoa
 */

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.Campaigns;
using VnuaCare.Business.Business.Campaigns.CampaignQueries;
using VnuaCare.Business.Business.Doctors;
using VnuaCare.Business.Business.Doctors.DoctorQueries;
using VnuaCare.Business.Business.HealthCheckupRecords;
using VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordCommands;
using VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordQueries;
using VnuaCare.Business.Business.Staffs;
using VnuaCare.Business.Business.Staffs.StaffQueries;
using VnuaCare.Shared.Helper;
using VnuaCare.Shared.Utils;

namespace VnuaCare.API.Controllers.Business;

/// <summary>
/// Quản lý hồ sơ khám sức khỏe tổng hợp của cán bộ Học viện Nông nghiệp Việt Nam
/// </summary>
[ApiController]
[Route("api/v1/healthcheckuprecords")]
[ApiExplorerSettings(GroupName = "Thông tin chung bệnh án")]
[Authorize]
public class HealthCheckupRecordController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public HealthCheckupRecordController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách Bác sĩ dạng danh mục chọn (Combobox / Dropdown)
    /// </summary>
    /// <param name="specialtyId">ID chuyên khoa (tùy chọn lọc)</param>
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
    /// Lấy danh sách Cán bộ dạng danh mục chọn (Combobox / Dropdown)
    /// </summary>
    /// <param name="departmentId">ID Khoa/Phòng ban (tùy chọn lọc)</param>
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
    /// Lấy danh sách Khoa / Phòng ban dạng danh mục chọn (Combobox / Dropdown)
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

    /// <summary>
    /// Lấy danh sách Đợt khám sức khỏe dạng danh mục chọn (Combobox / Dropdown)
    /// </summary>
    /// <param name="year">Năm tổ chức đợt khám</param>
    /// <param name="status">Trạng thái đợt khám</param>
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
    /// Lấy danh sách cán bộ có sức khỏe suy giảm so với năm trước (phục vụ cảnh báo qua Email)
    /// </summary>
    /// <param name="campaignId">ID đợt khám cần kiểm tra (tùy chọn)</param>
    /// <returns>Danh sách hồ sơ cán bộ có xếp loại sức khỏe giảm</returns>
    [HttpGet, Route("declined-health")]
    [ProducesResponseType(typeof(ResponseObject<List<DeclinedHealthRecordModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeclinedHealth([FromQuery] int? campaignId)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetDeclinedHealthRecordsQuery(campaignId));
        });
    }

    /// <summary>
    /// Gửi Email cảnh báo cho cán bộ có sức khỏe giảm so với năm trước (gửi lẻ theo RecordId hoặc hàng loạt theo CampaignId)
    /// </summary>
    /// <param name="model">Thông số gửi cảnh báo (RecordId / CampaignId / ForceResend)</param>
    /// <returns>Thống kê số lượng email gửi thành công, bỏ qua và thất bại</returns>
    [HttpPost, Route("send-health-alert")]
    [ProducesResponseType(typeof(ResponseObject<SendHealthAlertResultModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendHealthAlert([FromBody] SendHealthAlertRequestModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new SendHealthDeclineAlertCommand(model));
        });
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
}
