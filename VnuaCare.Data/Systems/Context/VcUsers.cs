using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VnuaCare.Shared.Model;

namespace VnuaCare.Data.Systems.Context;

/// <summary>
/// Bảng người dùng đăng nhập
/// </summary>
[Table("vc_users")]
public class VcUsers : EntityBase.BaseEntity
{
    public VcUsers()
    {
        
    }
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("user_id", TypeName = "int")]
    public int UserId { get; set; }
    /// <summary>
    /// Tên đăng nhập (Mã cán bộ hoặc tên tài khoản)
    /// </summary>
    [Column("username", TypeName = "varchar(100)")]
    public string Username { get; set; }

    /// <summary>
    /// Mật khẩu đã mã hóa băm (BCrypt)
    /// </summary>
    [Column("password_hash", TypeName = "varchar(255)")]
    public string Password { get; set; }

    /// <summary>
    /// Địa chỉ Email
    /// </summary>
    [Column("email", TypeName = "varchar(255)")]
    public string? Email { get; set; }
    
    /// <summary>
    /// Vai trò (ADMIN, DOCTOR, STAFF)
    /// </summary>
    [Column("role", TypeName = "varchar(30)")]
    public string Role { get; set; }
    
    /// <summary>
    /// Thời gian tạo tài khoản
    /// </summary>
    [Column("created_at", TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời gian cập nhật tài khoản lần cuối
    /// </summary>
    [Column("updated_at", TypeName = "datetime2")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Trạng thái hoạt động (true: Đang hoạt động, false: Bị khóa)
    /// </summary>
    [Column("is_active", TypeName = "bit")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Mã Refresh Token dùng để cấp lại Access Token
    /// </summary>
    [Column("refresh_token", TypeName = "varchar(255)")]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Thời gian hết hạn của Refresh Token
    /// </summary>
    [Column("refresh_token_expiry_time", TypeName = "datetime2")]
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    // (Liên kết 1-1 với Staff và Doctor)
    [InverseProperty("User")]
    public virtual VcStaffs? Staff { get; set; }
    [InverseProperty("User")]
    public virtual VcDoctors? Doctor { get; set; }
}