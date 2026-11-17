using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.SpecialistExams;

public class SpecialistExamConstant
{
    public const string CachePrefix = "specialistexams";
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
