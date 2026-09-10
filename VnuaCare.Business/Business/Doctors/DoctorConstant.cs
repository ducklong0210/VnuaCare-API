namespace VnuaCare.Business.Business.Doctors;

public class DoctorConstant
{
    public const string CachePrefix = "doctor";
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