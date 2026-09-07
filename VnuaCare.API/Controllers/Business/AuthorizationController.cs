using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VnuaCare.Business.Business.Authorizations;
using VnuaCare.Business.Business.Authorizations.AuthorizationCommands;

namespace VnuaCare.API.Controllers.Business;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthorizationController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthorizationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var result = await _mediator.Send(new UserLoginCommand(model));
        return Ok(result);
    }
}
