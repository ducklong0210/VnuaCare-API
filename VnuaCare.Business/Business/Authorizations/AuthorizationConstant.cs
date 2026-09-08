namespace VnuaCare.Business.Business.Authorizations;

public class AuthorizationConstant
{
    public const string CachePrefix = "appoinment";
    private const string SelectItemCacheSubfix = "select-item";
    
    public static string BuildCacheKey(string id = "")
    {
        if (string.IsNullOrEmpty(id))
        {
            //Cache cho danh sách combobox
            return $"{CachePrefix}-{SelectItemCacheSubfix}";
        }
        else
        {
            //Cache cho item
            return $"{CachePrefix}-{id}";
        }
    }
}