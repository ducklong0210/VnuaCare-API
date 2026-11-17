using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.InternalMedicine;

public class InternalMedicineConstant
{
    public const string CachePrefix = "internalmedicine";
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
