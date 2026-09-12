using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.Authorizations;

public class AuthorizationConstant
{
    public const string CachePrefix = VnuaCareCacheConstant.AUTHORIZATION;
    private const string SelectItemCacheSubfix = VnuaCareCacheConstant.LIST_SELECT;
    
    public static string BuildCacheKey(string id = "")
    {
        if (string.IsNullOrEmpty(id))
        {
            //Cache cho danh sách combobox
            return $"{CachePrefix}-{SelectItemCacheSubfix}";
        }
        else
        {
            //Cache cho item
            return $"{CachePrefix}-{id}";
        }
    }
}