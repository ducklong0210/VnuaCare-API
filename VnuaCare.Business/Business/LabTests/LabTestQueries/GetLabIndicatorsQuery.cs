/**
 * Nghiệp vụ đọc danh mục chỉ số xét nghiệm chuẩn (GetLabIndicatorsQuery):
 * Chuẩn hóa tham số nhóm xét nghiệm, đọc danh mục từ bộ nhớ đệm (Cache-Aside) trước;
 * nếu chưa có trong Cache thì truy vấn bảng vc_lab_indicator_catalog bằng ReadDbContext (AsNoTracking) và lưu vào Cache.
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.LabTests.LabTestQueries;

/// <summary>
/// Truy vấn lấy danh mục các chỉ số xét nghiệm chuẩn kèm ngưỡng tham chiếu (có thể lọc theo nhóm xét nghiệm)
/// </summary>
public class GetLabIndicatorsQuery : IRequest<List<LabIndicatorModel>>
{
    public string? CategoryGroup { get; set; } // Nhóm xét nghiệm cần lọc (HEMATOLOGY, BIOCHEMISTRY, URINALYSIS, IMMUNOLOGY hoặc để trống lấy tất cả)

    public GetLabIndicatorsQuery(string? categoryGroup = null)
    {
        CategoryGroup = categoryGroup;
    }

    public class Handler : IRequestHandler<GetLabIndicatorsQuery, List<LabIndicatorModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;  // DbContext chỉ đọc tối ưu cho thao tác truy vấn danh mục
        private readonly ICacheService _cacheService;          // Dịch vụ bộ nhớ đệm (Cache-Aside) giúp giảm tải truy vấn lặp lại

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Xử lý lấy danh mục chỉ số xét nghiệm từ Cache hoặc CSDL
        /// </summary>
        public async Task<List<LabIndicatorModel>> Handle(GetLabIndicatorsQuery request, CancellationToken cancellationToken)
        {
            #region Chuẩn hóa điều kiện lọc và khóa Cache
            string normGroup = string.IsNullOrWhiteSpace(request.CategoryGroup)
                ? string.Empty
                : request.CategoryGroup.Trim().ToUpperInvariant();

            // Xây dựng khóa Cache cho danh mục chỉ số xét nghiệm theo nhóm lọc
            string cacheKey = LabTestConstant.BuildIndicatorCacheKey(normGroup);
            #endregion

            #region Truy vấn theo cơ chế Cache-Aside
            // Đọc danh mục chỉ số xét nghiệm từ Cache trước; nếu chưa có thì truy vấn bảng vc_lab_indicator_catalog bằng AsNoTracking rồi lưu Cache
            var result = await _cacheService.GetOrCreate<List<LabIndicatorModel>>(cacheKey, async () =>
            {
                var query = _dataContext.VcLabIndicatorCatalog.AsNoTracking();

                // Lọc theo nhóm xét nghiệm nếu người dùng có truyền tham số categoryGroup
                if (!string.IsNullOrEmpty(normGroup))
                {
                    query = query.Where(x => x.CategoryGroup.ToUpper() == normGroup);
                }

                // Sắp xếp theo nhóm xét nghiệm và thứ tự ID chuẩn rồi ánh xạ sang DTO LabIndicatorModel
                return await query
                    .OrderBy(x => x.CategoryGroup)
                    .ThenBy(x => x.IndicatorId)
                    .Select(x => new LabIndicatorModel
                    {
                        IndicatorId = x.IndicatorId,
                        IndicatorCode = x.IndicatorCode,
                        IndicatorName = x.IndicatorName,
                        CategoryGroup = x.CategoryGroup,
                        Unit = x.Unit,
                        RefMin = x.RefMin,
                        RefMax = x.RefMax,
                        RefTextRange = x.RefTextRange,
                        GenderSpecific = x.GenderSpecific,
                        MeasuringDevice = x.MeasuringDevice
                    })
                    .ToListAsync(cancellationToken);
            });
            #endregion

            return result ?? new List<LabIndicatorModel>();
        }
    }
}
