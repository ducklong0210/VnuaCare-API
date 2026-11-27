/**
 * Khai báo hằng số và hàm dựng khóa bộ nhớ đệm (Cache Key) cho Phân hệ Xét nghiệm Cận lâm sàng (LabTests)
 */

using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.LabTests;

/// <summary>
/// Hằng số quản lý khóa Cache của phiếu xét nghiệm theo hồ sơ và danh mục chỉ số xét nghiệm chuẩn
/// </summary>
public class LabTestConstant
{
    public const string CachePrefix = "lab-tests";                        // Tiền tố Cache cho kết quả xét nghiệm theo hồ sơ khám
    public const string IndicatorCachePrefix = "lab-indicators";          // Tiền tố Cache cho danh mục 34 chỉ số xét nghiệm chuẩn
    public const string SelectItemCacheSubfix = VnuaCareCacheConstant.LIST_SELECT; // Hậu tố mặc định khi không truyền tham số định danh

    /// <summary>
    /// Tạo khóa Cache cho kết quả xét nghiệm gắn với một hồ sơ khám (recordId)
    /// </summary>
    public static string BuildCacheKey(string id = "")
    {
        return string.IsNullOrEmpty(id)
            ? $"{CachePrefix}-{SelectItemCacheSubfix}"
            : $"{CachePrefix}-{id}";
    }

    /// <summary>
    /// Tạo khóa Cache cho danh mục chỉ số xét nghiệm chuẩn theo từng nhóm (HEMATOLOGY, BIOCHEMISTRY, URINALYSIS...)
    /// </summary>
    public static string BuildIndicatorCacheKey(string group = "")
    {
        return string.IsNullOrEmpty(group)
            ? $"{IndicatorCachePrefix}-{SelectItemCacheSubfix}"
            : $"{IndicatorCachePrefix}-{group}";
    }
}
