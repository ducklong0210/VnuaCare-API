namespace VnuaCare.Business.Business.Staffs;

public class StaffConstant
{
    public const string CachePrefix = "staff";
    public const string SelectItemCacheSubfix = "select-item";
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