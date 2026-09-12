using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.Doctors;

public class DoctorConstant
{
    public const string CachePrefix = VnuaCareCacheConstant.DOCTOR;
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