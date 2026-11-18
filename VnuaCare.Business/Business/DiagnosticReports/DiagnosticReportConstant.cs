using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.DiagnosticReports;

public class DiagnosticReportConstant
{
    public const string CachePrefix = "diagnostic-reports";
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
