using System.ComponentModel.DataAnnotations;
using VnuaCare.Data.Systems.Context;

namespace VnuaCare.Business.Business.Users;

public abstract record UserBaseModel
{
    public int UserId { get; init; }

    [Required(ErrorMessage = "user.username.required")]
    public string Username { get; init; }

    [Required(ErrorMessage = "user.email.required")]
    public string Email { get; init; }

    [Required(ErrorMessage = "user.email.required")]
    public string Role { get; init; }

    public bool IsActive { get; init; } = true;

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record UserModel : UserBaseModel
{
    // public List
}

public record UpdatePasswordUserModel
{
    public int? ModifiedUserId {get; init;}
    
    public string OldPassword {get; init;}
    
    public string NewPassword {get; init;}
    
    public string ConfirmPassword {get; init;}

    public void UpdatePassword(VcUsers entity)
    {
        entity.Password = this.NewPassword;
    }
    
}

public record LoginUserModel
{
    // public string 
}