namespace VnuaCare.Shared.Helper.Response;

public class ApiResponse<T>
{
    public string TraceId { get; set; } = string.Empty;
    public T? Data { get; set; }
    public int StatusCode { get; set; } = 200;
    public string Message { get; set; } = string.Empty;
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

    public static ApiResponse<T> Success(T data, string message = "Success")
    {
        return new ApiResponse<T>(data,200,message);
    }

    public static ApiResponse<T> Error(string message, int code = 400)
    {
        return new ApiResponse<T>(default, code, message);
    }
    
}

public class ApiResponse : ApiResponse<object>
{
    public ApiResponse(){ }
    public ApiResponse(object data, int statusCode, string message = "Success",string traceId = "", double duration = 0 ){}

    public static new ApiResponse Success(object? data, string message = "Success")
    {
        return new ApiResponse(data ?? new object(),200,message);
    }
    public static new ApiResponse Fail(string message, int code = 400)
    {
        return new ApiResponse(new object(), code, message);
    }
}