/**
 * Cấu trúc đóng gói dữ liệu phản hồi API chuẩn hóa (Standard API Response Contract)
 */

namespace VnuaCare.Shared.Helper.Response;

/// <summary>
/// Mô hình dữ liệu phản hồi API chuẩn hóa dạng Generic chứa dữ liệu Data kiểu T
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của payload trả về cho Client</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Mã định danh vết của request phục vụ tra cứu log và gỡ lỗi
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Dữ liệu kết quả nghiệp vụ trả về cho Client
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Mã trạng thái HTTP (200, 400, 401, 500...)
    /// </summary>
    public int StatusCode { get; set; } = 200;

    /// <summary>
    /// Thông điệp phản hồi gửi tới Client (Thành công hoặc chi tiết lỗi)
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian hệ thống xử lý request (tính bằng mili-giây ms)
    /// </summary>
    public double Duration { get; set; }
    
    public ApiResponse()
    {
    }

    public ApiResponse(T data, int statusCode, string message = "Success",string traceId = "", double duration = 0)
    {
        Data = data;
        StatusCode = statusCode;
        Message = message;
        TraceId = traceId;
        Duration = duration;
    }

    /// <summary>
    /// Tạo đối tượng phản hồi thành công (HTTP 200)
    /// </summary>
    public static ApiResponse<T> Success(T data, string message = "Success")
    {
        return new ApiResponse<T>(data, 200, message);
    }

    /// <summary>
    /// Tạo đối tượng phản hồi thất bại kèm mã lỗi
    /// </summary>
    public static ApiResponse<T> Error(string message, int code = 400)
    {
        return new ApiResponse<T>(default, code, message);
    }
    
}

/// <summary>
/// Phiên bản không generic của ApiResponse dành cho các phản hồi không có dữ liệu trả về hoặc dữ liệu dạng object
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public ApiResponse(){ }
    public ApiResponse(object data, int statusCode, string message = "Success",string traceId = "", double duration = 0 ){}

    /// <summary>
    /// Tạo đối tượng phản hồi thành công dạng object
    /// </summary>
    public static new ApiResponse Success(object? data, string message = "Success")
    {
        return new ApiResponse(data ?? new object(), 200, message);
    }

    /// <summary>
    /// Tạo đối tượng phản hồi thất bại dạng object
    /// </summary>
    public static new ApiResponse Fail(string message, int code = 400)
    {
        return new ApiResponse(new object(), code, message);
    }
}
