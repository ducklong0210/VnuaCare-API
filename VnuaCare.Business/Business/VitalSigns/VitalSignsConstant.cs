/**
 * Hằng số và tiền tố Cache cho Phân hệ Khám Thể lực & Chỉ số Sinh tồn (Vital Signs)
 */

namespace VnuaCare.Business.Business.VitalSigns;

public static class VitalSignsConstant
{
    public const string CACHE_PREFIX = "vital_signs";

    public static string BuildCacheKey(string recordId)
    {
        return $"{CACHE_PREFIX}:record_{recordId}";
    }
}
