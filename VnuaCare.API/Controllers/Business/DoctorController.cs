using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.Doctors;
using VnuaCare.Business.Business.Doctors.DoctorCommands;
using VnuaCare.Business.Business.Doctors.DoctorQueries;
using VnuaCare.Shared.Helper;
using VnuaCare.Shared.Utils;

namespace VnuaCare.API.Controllers.Business;

[ApiController]
[Route("api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "Quản lý bác sĩ")]
[Authorize]
public class DoctorController :ApiControllerBase
{
    private readonly IMediator _mediator;
    public DoctorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost, Route("filter")]
    [ProducesResponseType(typeof(ResponseObject<DoctorModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Filter([FromBody] DoctorFilterModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetFilterDoctorQuery(model));
        });
    }

    [HttpPost, Route("{id}")]
    [ProducesResponseType(typeof(ResponseObject<DoctorModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new GetDoctorByIdIndex(id));
        });
    }
    
    
    [HttpPost,Route("add")]
    [ProducesResponseType(typeof(ResponseObject<CreateDoctorModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateDoctorModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new CreateDoctorCommand(model));
        });
    }
    
    [HttpPut,Route("update")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdateDoctorModel model)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new UpdateDoctorCommand(model));
        });
    }
    
    [HttpDelete,Route("{doctorId}")]
    [ProducesResponseType(typeof(ResponseObject<Unit>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete([FromRoute] int doctorId)
    {
        return await ExecuteFuntion(async () =>
        {
            return await _mediator.Send(new DeleteDoctorCommand(doctorId));
        });
    }
    
    
}