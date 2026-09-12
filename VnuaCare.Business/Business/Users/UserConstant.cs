using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.Users;

public class UserConstant
{
    public const string CachePrefix = VnuaCareCacheConstant.USER;
    public const string SelectItemCacheSubfix = VnuaCareCacheConstant.LIST_SELECT;
    
    
    public static string BuildCacheKey(string id = "")
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