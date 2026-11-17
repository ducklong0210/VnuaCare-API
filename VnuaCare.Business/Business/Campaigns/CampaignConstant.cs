/**
 * Hằng số và hàm sinh tiền tố Cache cho Phân hệ Đợt khám sức khỏe (Campaigns)
 */

namespace VnuaCare.Business.Business.Campaigns;

public static class CampaignConstant
{
    public const string CACHE_PREFIX = "campaigns";

    public static string BuildCacheKey(string suffix)
    {
        return $"{CACHE_PREFIX}:{suffix}";
    }
}
