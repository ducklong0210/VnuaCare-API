using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.EmailLogs;

public class EmailLogConstant
{
    public const string CachePrefix = "email-logs";
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
