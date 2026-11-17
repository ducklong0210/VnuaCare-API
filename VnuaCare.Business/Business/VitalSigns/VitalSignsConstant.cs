using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.VitalSigns;

public class VitalSignsConstant
{
    public const string CachePrefix = "vitalsigns";
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
