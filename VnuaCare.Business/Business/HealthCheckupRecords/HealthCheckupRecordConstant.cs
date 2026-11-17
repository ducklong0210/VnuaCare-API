using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.HealthCheckupRecords;

public class HealthCheckupRecordConstant
{
    public const string CachePrefix = "healthcheckuprecord";
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
