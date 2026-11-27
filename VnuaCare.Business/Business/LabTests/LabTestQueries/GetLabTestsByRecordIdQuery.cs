/**
 * Nghiệp vụ lấy toàn bộ phiếu chỉ định và chi tiết kết quả xét nghiệm theo hồ sơ khám (GetLabTestsByRecordIdQuery):
 * Kiểm tra phân quyền chống truy cập chéo hồ sơ (IDOR) đối với tài khoản Cán bộ (STAFF),
 * sau đó đọc dữ liệu theo cơ chế Cache-Aside từ bảng vc_lab_test_orders kết hợp vc_lab_test_results và vc_lab_indicator_catalog.
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.LabTests.LabTestQueries;

/// <summary>
/// Truy vấn lấy danh sách phiếu chỉ định xét nghiệm cùng toàn bộ chỉ số kết quả của một hồ sơ khám sức khỏe
/// </summary>
public class GetLabTestsByRecordIdQuery : IRequest<List<LabTestOrderModel>>
{
    public int RecordId { get; set; } // ID hồ sơ khám sức khỏe cần xem kết quả xét nghiệm

    public GetLabTestsByRecordIdQuery(int recordId)
    {
        RecordId = recordId;
    }

    public class Handler : IRequestHandler<GetLabTestsByRecordIdQuery, List<LabTestOrderModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;  // DbContext chỉ đọc phục vụ truy vấn kết quả xét nghiệm
        private readonly ICacheService _cacheService;          // Dịch vụ bộ nhớ đệm (Cache-Aside)
        private readonly IContextAccessor _contextAccessor;    // Dịch vụ trích xuất thông tin người dùng đang đăng nhập (Role, UserId)

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        /// <summary>
        /// Xử lý lấy kết quả xét nghiệm theo RecordId: Kiểm tra quyền sở hữu hồ sơ và đọc từ Cache/CSDL
        /// </summary>
        public async Task<List<LabTestOrderModel>> Handle(GetLabTestsByRecordIdQuery request, CancellationToken cancellationToken)
        {
            #region Phân quyền và chống truy cập chéo hồ sơ (IDOR)
            // Nếu người gọi là Cán bộ (STAFF), bắt buộc kiểm tra hồ sơ RecordId này có thuộc chính UserId đang đăng nhập hay không
            if (_contextAccessor.Role == Role.STAFF.ToString())
            {
                var currentUserId = _contextAccessor.UserId;
                var isOwner = currentUserId.HasValue && await _dataContext.VcHealthCheckupRecords.AsNoTracking()
                    .AnyAsync(r => r.RecordId == request.RecordId && r.Staff != null && r.Staff.UserId == currentUserId.Value, cancellationToken);
                if (!isOwner)
                {
                    throw new UnauthorizedAccessException("Bạn chỉ có quyền xem kết quả xét nghiệm trong hồ sơ của chính mình.");
                }
            }
            #endregion

            #region Truy vấn dữ liệu theo cơ chế Cache-Aside
            // Xây dựng khóa bộ nhớ đệm (Cache Key) theo ID hồ sơ khám
            string cacheKey = LabTestConstant.BuildCacheKey(request.RecordId.ToString());

            // Đọc danh sách phiếu chỉ định và kết quả xét nghiệm từ Cache trước; nếu chưa có thì truy vấn CSDL bằng AsNoTracking rồi lưu vào Cache
            var result = await _cacheService.GetOrCreate<List<LabTestOrderModel>>(cacheKey, async () =>
            {
                var orders = await _dataContext.VcLabTestOrders.AsNoTracking()
                    .Where(o => o.RecordId == request.RecordId)
                    .OrderBy(o => o.OrderId)
                    .Select(o => new LabTestOrderModel
                    {
                        OrderId = o.OrderId,
                        RecordId = o.RecordId,
                        ServiceId = o.ServiceId,
                        SampleBarcode = o.SampleBarcode,
                        SampleType = o.SampleType,
                        CollectedAt = o.CollectedAt,
                        ReceivedAt = o.ReceivedAt,
                        Status = o.Status,
                        // Ánh xạ danh sách chỉ số kết quả con kèm thông tin danh mục tham chiếu chuẩn, sắp xếp theo nhóm xét nghiệm
                        Results = o.Results
                            .OrderBy(r => r.Indicator != null ? r.Indicator.CategoryGroup : "")
                            .ThenBy(r => r.IndicatorId)
                            .Select(r => new LabTestResultModel
                            {
                                ResultId = r.ResultId,
                                OrderId = r.OrderId,
                                IndicatorId = r.IndicatorId,
                                IndicatorCode = r.Indicator != null ? r.Indicator.IndicatorCode : string.Empty,
                                IndicatorName = r.Indicator != null ? r.Indicator.IndicatorName : string.Empty,
                                CategoryGroup = r.Indicator != null ? r.Indicator.CategoryGroup : string.Empty,
                                Unit = r.Indicator != null ? r.Indicator.Unit : null,
                                RefMin = r.Indicator != null ? r.Indicator.RefMin : null,
                                RefMax = r.Indicator != null ? r.Indicator.RefMax : null,
                                RefTextRange = r.Indicator != null ? r.Indicator.RefTextRange : null,
                                MeasuringDevice = r.Indicator != null ? r.Indicator.MeasuringDevice : null,
                                ResultValue = r.ResultValue,
                                NumericValue = r.NumericValue,
                                Flag = r.Flag,
                                Note = r.Note,
                                CreatedAt = r.CreatedAt
                            })
                            .ToList()
                    })
                    .ToListAsync(cancellationToken);

                return orders;
            });
            #endregion

            return result ?? new List<LabTestOrderModel>();
        }
    }
}
