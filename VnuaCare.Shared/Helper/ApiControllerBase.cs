/**
 * Controller cơ sở (Base Controller) bao bọc việc thực thi API, xử lý ngoại lệ và chuẩn hóa định dạng phản hồi
 */

using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Helper.Response;
using VnuaCare.Shared.Localization;

namespace VnuaCare.Shared.Helper;

/// <summary>
/// Lớp Controller nền tảng cho toàn bộ các API Controller trong hệ thống.
/// Cung cấp hàm ExecuteFuntion dùng chung để bắt lỗi tập trung, tính thời gian xử lý (duration)
/// và đồng nhất cấu trúc phản hồi ApiResponse chuẩn RESTful.
/// </summary>
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

    /// <summary>
    /// Bao bọc thực thi nghiệp vụ có trả về dữ liệu kết quả, tự động đo thời gian và bắt ngoại lệ
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu của kết quả trả về</typeparam>
    /// <param name="func">Hàm delegate bất đồng bộ thực thi nghiệp vụ</param>
    /// <returns>IActionResult chứa ApiResponse chuẩn</returns>
    protected async Task<IActionResult> ExecuteFuntion<T>(Func<Task<T>> func)
    {
        // Khởi động đồng hồ đo thời gian phản hồi của request
        var stopwatch = Stopwatch.StartNew();
        try
        {
            // Thực thi logic nghiệp vụ
            var result = await func();
            stopwatch.Stop();

            // Trả về HTTP 200 OK cùng dữ liệu và mã TraceId
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
            // Lấy thông báo lỗi chi tiết (bao gồm cả InnerException nếu có)
            var errorMsg = ex.InnerException != null ? $"{ex.Message} --> {ex.InnerException.Message}" : ex.Message;

            // Trả về HTTP 400 Bad Request kèm thông báo lỗi
            return BadRequest(new ApiResponse<object>(
                data: null,
                message: errorMsg,
                statusCode: 400,
                traceId: HttpContext.TraceIdentifier,
                duration: stopwatch.ElapsedMilliseconds
            ));
        }
    }

    /// <summary>
    /// Bao bọc thực thi nghiệp vụ không có dữ liệu trả về (void/Task)
    /// </summary>
    /// <param name="func">Hàm delegate bất đồng bộ thực thi nghiệp vụ</param>
    /// <returns>IActionResult chứa ApiResponse chuẩn</returns>
    protected async Task<IActionResult> ExecuteFuntion(Func<Task> func)
    {
        // Khởi động đồng hồ đo thời gian phản hồi của request
        var stopwatch = Stopwatch.StartNew();
        try
        {
            // Thực thi logic nghiệp vụ
            await func();
            stopwatch.Stop();

            // Trả về HTTP 200 OK thành công
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
            // Trả về HTTP 400 Bad Request kèm thông báo lỗi
            return BadRequest(new ApiResponse<object>(
                data: null,
                message: e.Message,
                statusCode: 400,
                traceId: HttpContext.TraceIdentifier,
                duration: stopwatch.ElapsedMilliseconds));
        }
    }
}
