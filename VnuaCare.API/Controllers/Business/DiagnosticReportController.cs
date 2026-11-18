/**
 * Controller Quản lý Kết quả Chẩn đoán hình ảnh & Thăm dò chức năng (Diagnostic Reports)
 * Siêu âm, X-quang, Điện tim, Nội soi, CT, MRI...
 * Chuẩn CRUD: Create, Update, Delete, Read, Filter
 */

using MediatR;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.DiagnosticReports;
using VnuaCare.Business.Business.DiagnosticReports.DiagnosticReportCommands;
using VnuaCare.Business.Business.DiagnosticReports.DiagnosticReportQueries;
using VnuaCare.Shared.Helper;
using VnuaCare.Shared.Utils;

namespace VnuaCare.API.Controllers.Business;

/// <summary>
/// Quản lý kết quả chẩn đoán hình ảnh và thăm dò chức năng (Siêu âm, X-quang, Điện tim, Nội soi, CT, MRI)
/// </summary>
[ApiController]
[Route("api/v1/diagnosticreports")]
[ApiExplorerSettings(GroupName = "Chẩn đoán hình ảnh")]
public class DiagnosticReportController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public DiagnosticReportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Thêm mới phiếu kết quả chẩn đoán hình ảnh (Create)
    /// </summary>
    /// <param name="model">Dữ liệu kết quả chẩn đoán hình ảnh</param>
    /// <returns>Kết quả thêm mới</returns>
    [HttpPost, Route("add")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateDiagnosticReportModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new CreateDiagnosticReportCommand(model));
        });
    }

    /// <summary>
    /// Cập nhật phiếu kết quả chẩn đoán hình ảnh (Update)
    /// </summary>
    /// <param name="model">Dữ liệu cập nhật kết quả chẩn đoán</param>
    /// <returns>Kết quả cập nhật</returns>
    [HttpPut, Route("update")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdateDiagnosticReportModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new UpdateDiagnosticReportCommand(model));
        });
    }

    /// <summary>
    /// Xóa phiếu kết quả chẩn đoán hình ảnh (Delete)
    /// </summary>
    /// <param name="id">ID phiếu kết quả chẩn đoán hình ảnh (reportId)</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete, Route("{id}")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new DeleteDiagnosticReportCommand(id));
        });
    }

    /// <summary>
    /// Lấy danh sách kết quả chẩn đoán hình ảnh của hồ sơ khám (Read by RecordId)
    /// </summary>
    /// <param name="recordId">ID hồ sơ khám sức khỏe</param>
    /// <param name="modalityType">Loại chẩn đoán cần lọc (ULTRASOUND, XRAY, ECG, ENDOSCOPY, CT, MRI)</param>
    /// <returns>Danh sách kết quả chẩn đoán hình ảnh</returns>
    [HttpGet, Route("{recordId}")]
    [ProducesResponseType(typeof(ResponseObject<List<DiagnosticReportModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByRecordId([FromRoute] int recordId, [FromQuery] string? modalityType)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetDiagnosticReportsByRecordIdQuery(recordId, modalityType));
        });
    }

    /// <summary>
    /// Lấy chi tiết một phiếu kết quả chẩn đoán hình ảnh theo ID (Read by ReportId)
    /// </summary>
    /// <param name="id">ID phiếu kết quả chẩn đoán hình ảnh (reportId)</param>
    /// <returns>Chi tiết phiếu kết quả</returns>
    [HttpGet, Route("detail/{id}")]
    [ProducesResponseType(typeof(ResponseObject<DiagnosticReportModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetDiagnosticReportByIdIndex(id));
        });
    }

    /// <summary>
    /// Lọc và phân trang danh sách phiếu kết quả chẩn đoán hình ảnh (Filter)
    /// </summary>
    /// <param name="filter">Điều kiện tìm kiếm, lọc theo hồ sơ, loại chẩn đoán, bác sĩ và phân trang</param>
    /// <returns>Danh sách phiếu chẩn đoán hình ảnh phân trang</returns>
    [HttpPost, Route("filter")]
    [ProducesResponseType(typeof(ResponseObject<PaginationList<DiagnosticReportModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Filter([FromBody] DiagnosticReportFilterModel filter)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetFilterDiagnosticReportQuery(filter));
        });
    }
}
