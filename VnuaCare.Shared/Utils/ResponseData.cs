/**
 * Mô hình đóng gói kết quả phản hồi nghiệp vụ, phân trang dữ liệu và định nghĩa mã trạng thái
 */

using VnuaCare.Shared.Model;

namespace VnuaCare.Shared.Utils;

/// <summary>
/// Lớp hỗ trợ dữ liệu phản hồi
/// </summary>
public class ResponseData
{
}

/// <summary>
/// Cấu trúc phản hồi nghiệp vụ cơ bản kế thừa từ EvedenceTraceLog
/// </summary>
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

    /// <summary>
    /// Mã trạng thái nghiệp vụ
    /// </summary>
    public StatusCode StatusCode { get; set; } = StatusCode.Success;

    /// <summary>
    /// Thông điệp kết quả
    /// </summary>
    public string Message { get; set; } = "Thành công";

    /// <summary>
    /// Thời gian thực thi request (ms)
    /// </summary>
    public long RequestDuration { get; set; }
}

/// <summary>
/// Cấu trúc phản hồi nghiệp vụ chứa đối tượng dữ liệu cụ thể
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu trả về</typeparam>
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

    /// <summary>
    /// Dữ liệu nghiệp vụ trả về
    /// </summary>
    public T Data { get; set; }
}

/// <summary>
/// Cấu trúc phân trang danh sách dữ liệu (Pagination)
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của phần tử trong danh sách</typeparam>
public class PaginationList<T>
{
    /// <summary>
    /// Số thứ tự trang hiện tại
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Số lượng bản ghi trên một trang
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Tổng số bản ghi thỏa mãn điều kiện lọc trong CSDL
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Số lượng bản ghi thực tế trong trang hiện tại
    /// </summary>
    public int DataCount { get; set; }

    /// <summary>
    /// Danh sách các bản ghi của trang hiện tại
    /// </summary>
    public List<T> Data { get; set; }
}

/// <summary>
/// Danh mục mã trạng thái phản hồi HTTP/nghiệp vụ
/// </summary>
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
