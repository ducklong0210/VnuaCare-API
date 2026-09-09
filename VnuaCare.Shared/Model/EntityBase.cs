using System.ComponentModel.DataAnnotations.Schema;

namespace VnuaCare.Shared.Model;

public class BaseEntity
{
    /// <summary>
    /// Trạng thái hoạt động (1: Đang hoạt động, 0: Khóa/Ngừng)
    /// </summary>
    [Column("is_active", TypeName = "bit")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Thời gian tạo bản ghi
    /// </summary>
    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời gian cập nhật lần cuối
    /// </summary>
    [Column("updated_at", TypeName = "datetime2")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}