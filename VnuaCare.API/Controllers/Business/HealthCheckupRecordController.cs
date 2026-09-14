using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.HealthCheckupRecords;
using VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordQueries;
using VnuaCare.Shared.Helper;
using VnuaCare.Shared.Utils;

namespace VnuaCare.API.Controllers.Business;

[ApiController]
[Route("v1/healthcheckuprecord")]
[ApiExplorerSettings(GroupName = " Thoong tin chung benh an")]
// [Authorize]
public class HealthCheckupRecordController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public HealthCheckupRecordController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet, Route("{id}")]
    [ProducesResponseType(typeof(ResponseObject<HealthCheckupRecordModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int id)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetHealthCheckupRecordByIdIndex(id));
        });
    }

}