using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.Staffs;

public class StaffConstant
{
    public const string CachePrefix = VnuaCareCacheConstant.STAFF;
    public const string SelectItemCacheSubfix = VnuaCareCacheConstant.LIST_SELECT;
    public static string BuildCacheKey(string id ="")
    {
        if (string.IsNullOrEmpty(id))
        {
            return $"{CachePrefix}-{SelectItemCacheSubfix}";
        }
        else
        {
            return $"{CachePrefix}-{id}";
        }
    }
}