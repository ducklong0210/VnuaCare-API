using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Helper.Response;
using VnuaCare.Shared.Localization;

namespace VnuaCare.Shared.Helper;

public class ApiControllerBase : ControllerBase
{
    protected readonly Func<IContextAccessor> _contextAccessor;
    protected readonly IMediator _mediator;
    protected readonly IStringLocalizer<Resources> _localizer;
    protected readonly IConfiguration _config;

    public ApiControllerBase(Func<IContextAccessor> contextAccessor, IMediator mediator,
        IStringLocalizer<Resources> localizer, IConfiguration config)
    {
        _contextAccessor = contextAccessor;
        _mediator = mediator;
        _localizer = localizer;
        _config = config;
    }

    public ApiControllerBase()
    {
    }

    protected async Task<IActionResult> ExecuteFuntion<T>(Func<Task<T>> func)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await func();
            stopwatch.Stop();
            return Ok(new ApiResponse<T>(
                data: result,
                message: "Success",
                statusCode: 200,
                traceId: HttpContext.TraceIdentifier,
                duration: stopwatch.ElapsedMilliseconds
            ));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var errorMsg = ex.InnerException != null ? $"{ex.Message} --> {ex.InnerException.Message}" : ex.Message;
            return BadRequest(new ApiResponse<object>(
                data: null,
                message: errorMsg,
                statusCode: 400,
                traceId: HttpContext.TraceIdentifier,
                duration: stopwatch.ElapsedMilliseconds
            ));
        }
    }

    protected async Task<IActionResult> ExecuteFuntion(Func<Task> func)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await func();
            stopwatch.Stop();
            return Ok(new ApiResponse<object>(
                data: null,
                message: "Success",
                statusCode: 200,
                traceId: HttpContext.TraceIdentifier,
                duration: stopwatch.ElapsedMilliseconds));
        }
        catch (Exception e)
        {
            stopwatch.Stop();
            return BadRequest(new ApiResponse<object>(
                data: null,
                message: e.Message,
                statusCode: 400,
                traceId: HttpContext.TraceIdentifier,
                duration: stopwatch.ElapsedMilliseconds));
        }
    }
}
