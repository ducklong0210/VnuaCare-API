using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.Campaigns;

/// <summary>
/// Cấu hình khóa lưu trữ bộ nhớ đệm (Cache) cho phân hệ Đợt khám sức khỏe
/// </summary>
public class CampaignConstant
{
    public const string CachePrefix = "campaigns";
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