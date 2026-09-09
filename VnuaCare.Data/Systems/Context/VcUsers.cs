using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VnuaCare.Shared.Model;

namespace VnuaCare.Data.Systems.Context;

[Table("vc_users")]
public class VcUsers : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("user_id", TypeName = "int")]
    public int UserId { get; set; }

    [Required]
    [Column("username", TypeName = "varchar(100)")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [Column("password_hash", TypeName = "varchar(255)")]
    public string Password { get; set; } = string.Empty;

    [Column("email", TypeName = "varchar(255)")]
    public string? Email { get; set; }
    
    [Required]
    [Column("role", TypeName = "varchar(30)")]
    public string Role { get; set; } = string.Empty;

    [Column("refresh_token", TypeName = "varchar(255)")]
    public string? RefreshToken { get; set; }

    [Column("refresh_token_expiry_time", TypeName = "datetime2")]
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
