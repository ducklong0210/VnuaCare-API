namespace VnuaCare.Shared.Enums;

/// <summary>
/// Cacheprefix
/// </summary>
public class VnuaCareCacheConstant { 
    public const string LIST_SELECT = "list-select";
    public const string USER =  "users";
    public const string STAFF = "staffs";
    public const string DOCTOR = "doctors";
    public const string AUTHORIZATION = "authorization";
}


/// <summary>
/// Vai trò 
/// </summary>
public enum Role
{
    
    SUPER_ADMIN,
    HEALTH_ADMIN,
    STAFF,
    DOCTOR
}