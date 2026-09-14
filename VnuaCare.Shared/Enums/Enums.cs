/**
 * Định nghĩa các tiền tố Cache và danh mục Vai trò phân quyền trong hệ thống
 */

namespace VnuaCare.Shared.Enums;

/// <summary>
/// Các tiền tố khóa lưu trữ bộ nhớ đệm (Redis Cache Keys) để phân vùng dữ liệu cache
/// </summary>
public class VnuaCareCacheConstant 
{ 
    public const string LIST_SELECT = "list-select";
    public const string USER = "users";
    public const string STAFF = "staffs";
    public const string DOCTOR = "doctors";
    public const string AUTHORIZATION = "authorization";
}

/// <summary>
/// Danh sách các vai trò (Roles) phân quyền người dùng trong hệ thống V-Care Health
/// </summary>
public enum Role
{
    SUPER_ADMIN,
    HEALTH_ADMIN,
    STAFF,
    DOCTOR
}

public enum Status
{
    COMPLETED,
    PENDING,
    FAILED,
}
