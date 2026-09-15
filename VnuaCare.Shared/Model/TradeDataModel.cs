/**
 * Mô hình dữ liệu nền tảng phục vụ truy vết bằng chứng kiểm toán (Audit Evidence Trace)
 */

namespace VnuaCare.Shared.Model;

/// <summary>
/// Lớp cơ sở chứa TraceId phục vụ việc ghi vết và liên kết log trong toàn hệ thống
/// </summary>
public class EvedenceTraceLog
{
    /// <summary>
    /// Mã định danh vết xử lý nghiệp vụ
    /// </summary>
    public string TraceId { get; set; }
}
