/**
 * Thực thể ánh xạ bảng tài khoản người dùng (vc_users)
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VnuaCare.Shared.Model;

namespace VnuaCare.Data.Systems.Context;

/// <summary>
/// Bảng tài khoản người dùng đăng nhập hệ thống
/// </summary>
[Table("vc_users")]
public class VcUsers : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("user_id", TypeName = "int")]
    public int UserId { get; set; } // Khóa chính: ID tài khoản người dùng

    [Required]
    [Column("username", TypeName = "varchar(100)")]
    public string Username { get; set; } = string.Empty; // Tên đăng nhập tài khoản

    [Required]
    [Column("password_hash", TypeName = "varchar(255)")]
    public string Password { get; set; } = string.Empty; 

    [Column("email", TypeName = "varchar(255)")]
    public string? Email { get; set; } 
    
    [Required]
    [Column("role", TypeName = "varchar(30)")]
    public string Role { get; set; } = string.Empty; // Vai trò phân quyền 

    [Column("refresh_token", TypeName = "varchar(255)")]
    public string? RefreshToken { get; set; } // Mã Refresh Token phục vụ gia hạn phiên đăng nhập

    [Column("refresh_token_expiry_time", TypeName = "datetime2")]
    public DateTime? RefreshTokenExpiryTime { get; set; } // Thời điểm hết hạn của Refresh Token
}
