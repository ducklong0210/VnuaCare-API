/**
 * Mô hình lọc và phân trang cơ sở (Base Query Filter Model)
 */

using VnuaCare.Shared.Constant;

namespace VnuaCare.Business;

/// <summary>
/// Lớp cơ sở chứa các tiêu chí tìm kiếm, lọc và phân trang cho các câu truy vấn (Queries)
/// </summary>
public record BaseQueryFilterModel
{
    /// <summary>
    /// Từ khóa tìm kiếm chung (Họ tên, mã, email...)
    /// </summary>
    public string? TextSearch { get; set; }

    /// <summary>
    /// Lọc theo ID người dùng
    /// </summary>
    public int? UserId { get; set; } 

    /// <summary>
    /// Lọc theo ID cán bộ
    /// </summary>
    public int? StaffId { get; set; }

    /// <summary>
    /// Lọc theo ID bác sĩ
    /// </summary>
    public int? DoctorId { get; set; }

    /// <summary>
    /// Số thứ tự trang hiện tại
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Số lượng bản ghi trên một trang
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Lọc theo trạng thái hoạt động (true: Đang hoạt động, false: Bị khóa, null: Tất cả)
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Tên thuộc tính dùng để sắp xếp (mặc định: CreatedAt)
    /// </summary>
    public string PropertyName { get; set; } = "CreatedAt";
    
    /// <summary>
    /// Hướng sắp xếp: "asc" (tăng dần) hoặc "desc" (giảm dần)
    /// </summary>
    public string Ascending { get; set; } = "desc";

    public BaseQueryFilterModel()
    {
        PageNumber = Constant.QueryFilter.PageNumber;
        PageSize = Constant.QueryFilter.PageSize;
    }
}
