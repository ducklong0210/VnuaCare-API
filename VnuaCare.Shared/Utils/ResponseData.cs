using VnuaCare.Shared.Model;

namespace VnuaCare.Shared.Utils;

public class ResponseData
{
}

public class Response : EvedenceTraceLog
{
    public Response()
    {
    }

    public Response(string message)
    {
        Message = message;
    }

    public Response(StatusCode code, string message)
    {
        StatusCode = code;
        Message = message;
    }

    public StatusCode StatusCode { get; set; } = StatusCode.Success;

    public string Message { get; set; } = "Thành công";

    public long RequestDuration { get; set; }
}

public class ResponseObject<T> : Response
{
    public ResponseObject(T data)
    {
        Data = data;
    }

    public ResponseObject(T data, string message)
    {
        Data = data;
        Message = message;
    }

    public ResponseObject(T data, string message, StatusCode code)
    {
        Data = data;
        Message = message;
        StatusCode = code;
    }

    public T Data { get; set; }
}

/// <summary>
/// Phân trang
/// </summary>
/// <typeparam name="T"></typeparam>
public class PaginationList<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int DataCount { get; set; }
    public List<T> Data { get; set; }
}

public enum StatusCode
{
    Success = 200,
    Created = 201,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    MethodNotAllowed = 405,
    NotAcceptable = 406,
    Conflict = 409,
    InternalServerError = 500,
}
