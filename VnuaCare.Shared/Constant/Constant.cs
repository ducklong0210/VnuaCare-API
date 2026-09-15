/**
 * Các hằng số định nghĩa mặc định trong toàn bộ hệ thống
 */

namespace VnuaCare.Shared.Constant;

/// <summary>
/// Lớp chứa các hằng số hệ thống dùng chung
/// </summary>
public class Constant
{
    /// <summary>
    /// Các tham số mặc định cho việc lọc và phân trang dữ liệu
    /// </summary>
    public class QueryFilter()
    {
        /// <summary>
        /// Kích thước trang mặc định (10 bản ghi mỗi trang)
        /// </summary>
        public const int PageSize = 10;

        /// <summary>
        /// Trang bắt đầu mặc định (trang đầu tiên)
        /// </summary>
        public const int PageNumber = 1;
    }
}
