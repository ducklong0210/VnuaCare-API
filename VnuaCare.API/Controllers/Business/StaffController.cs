using MediatR;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business;
using VnuaCare.Business.Business.Staffs;
using VnuaCare.Business.Business.Staffs.StaffQueries;
using VnuaCare.Shared.Helper;
using VnuaCare.Shared.Utils;

namespace VnuaCare.API.Controllers.Business;

[ApiController]
[Route("v1/staff-manager")]
[ApiExplorerSettings(GroupName = "Quản lý nhân viên")]
public class StaffController : ApiControllerBase
{
    public readonly IMediator _mediator;
    public StaffController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lọc danh sách nhân viên
    /// </summary>
    /// <param name="filter">Điều kiện để lọc</param>
    /// <returns></returns>
    [HttpPost, Route("filter")]
    [ProducesResponseType(typeof(ResponseObject<PaginationList<StaffModel>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Filter([FromBody] StaffFilterModel filter)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetFilterStaffQuery(filter));
        });
    }
}