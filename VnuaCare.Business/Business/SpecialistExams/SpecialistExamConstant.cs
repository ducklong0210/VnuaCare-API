/**
 * Hằng số và tiền tố Cache cho Phân hệ Khám Chuyên khoa lẻ (Specialist Exams)
 */

namespace VnuaCare.Business.Business.SpecialistExams;

public static class SpecialistExamConstant
{
    public const string CACHE_PREFIX = "specialist_exam";

    public static string BuildCacheKey(string recordId)
    {
        return $"{CACHE_PREFIX}:record_{recordId}";
    }
}
