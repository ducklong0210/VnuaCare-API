/**
 * Hằng số và tiền tố Cache cho Phân hệ Khám Nội khoa tổng quát (Internal Medicine)
 */

namespace VnuaCare.Business.Business.InternalMedicine;

public static class InternalMedicineConstant
{
    public const string CACHE_PREFIX = "internal_medicine";

    public static string BuildCacheKey(string recordId)
    {
        return $"{CACHE_PREFIX}:record_{recordId}";
    }
}
