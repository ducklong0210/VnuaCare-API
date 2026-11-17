using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.Campaigns;

public class CampaignConstant
{
    public const string CachePrefix = "campaigns";
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
