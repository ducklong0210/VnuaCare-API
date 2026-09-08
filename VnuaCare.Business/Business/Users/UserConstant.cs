namespace VnuaCare.Business.Business.Users;

public class UserConstant
{
    public const string CachePrefix = "user";
    public const string SelectItemCacheSubfix = "select-item";
    
    
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