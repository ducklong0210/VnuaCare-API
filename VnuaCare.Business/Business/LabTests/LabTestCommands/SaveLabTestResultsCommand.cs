/**
 * Nghiệp vụ lưu / cập nhật hàng loạt kết quả chỉ số xét nghiệm (SaveLabTestResultsCommand):
 * Kiểm tra phân quyền, xác thực phiếu chỉ định và danh mục chỉ số chuẩn, tự động chuyển đổi giá trị số,
 * tự động so sánh khoảng tham chiếu (ref_min / ref_max) để gắn cờ cảnh báo LOW / HIGH / NORMAL,
 * thực hiện Upsert (có thì cập nhật, chưa có thì thêm mới) vào CSDL và xóa Cache.
 */

using System.Globalization;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.Context;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.LabTests.LabTestCommands;

/// <summary>
/// Lệnh lưu hoặc cập nhật hàng loạt kết quả chỉ số xét nghiệm cho một phiếu chỉ định (vc_lab_test_results)
/// </summary>
public class SaveLabTestResultsCommand : IRequest<Unit>
{
    public SaveLabTestResultsModel Model { get; }

    public SaveLabTestResultsCommand(SaveLabTestResultsModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<SaveLabTestResultsCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;      // DbContext kết nối CSDL để ghi kết quả xét nghiệm
        private readonly ICacheService _cacheService;          // Dịch vụ quản lý và làm mới bộ nhớ đệm (Cache)
        private readonly IContextAccessor _contextAccessor;    // Dịch vụ trích xuất thông tin người dùng đang đăng nhập (Role, UserId)

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        /// <summary>
        /// Xử lý lưu hàng loạt chỉ số xét nghiệm: Kiểm tra quyền, đối chiếu danh mục chuẩn, tự động gắn cờ bất thường và Upsert vào CSDL
        /// </summary>
        public async Task<Unit> Handle(SaveLabTestResultsCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information($"Bắt đầu lưu {model.Results.Count} kết quả xét nghiệm cho phiếu OrderId {model.OrderId}: " + JsonSerializer.Serialize(model));

            #region Phân quyền
            // Chỉ Bác sĩ (DOCTOR), Quản trị viên y tế (HEALTH_ADMIN) hoặc Quản trị hệ thống (SUPER_ADMIN) mới được nhập kết quả xét nghiệm
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Chỉ Bác sĩ hoặc Quản trị viên y tế mới có quyền nhập kết quả xét nghiệm.");
            }
            #endregion

            #region Kiểm tra dữ liệu liên quan
            // Kiểm tra phiếu chỉ định xét nghiệm có tồn tại trong hệ thống hay không
            var order = await _dataContext.VcLabTestOrders
                .FirstOrDefaultAsync(x => x.OrderId == model.OrderId, cancellationToken);
            if (order == null)
            {
                throw new ArgumentException($"Không tìm thấy phiếu chỉ định xét nghiệm ID: {model.OrderId}");
            }

            // Nạp trước toàn bộ danh mục chỉ số chuẩn liên quan thành Dictionary để tra cứu ngưỡng ref_min / ref_max nhanh trong bộ nhớ
            var incomingResults = model.Results ?? new List<LabTestResultItemInput>();
            var indicatorIds = incomingResults.Select(r => r.IndicatorId).Distinct().ToList();
            var submittedIndicatorIdSet = indicatorIds.ToHashSet();

            // Nạp các kết quả đã lưu trước đó của phiếu này để phân biệt dòng cần cập nhật (Update) hay thêm mới (Insert)
            var indicatorsMap = indicatorIds.Count > 0
                ? await _dataContext.VcLabIndicatorCatalog.AsNoTracking()
                    .Where(i => indicatorIds.Contains(i.IndicatorId))
                    .ToDictionaryAsync(i => i.IndicatorId, cancellationToken)
                : new Dictionary<int, VcLabIndicatorCatalog>();

            var existingResults = await _dataContext.VcLabTestResults
                .Where(r => r.OrderId == model.OrderId)
                .ToDictionaryAsync(r => r.IndicatorId, cancellationToken);
            #endregion

            #region Xử lý tự động gắn cờ và Lưu vào CSDL (Upsert)
            // Đồng bộ xóa các chỉ số cũ đã bị người dùng gỡ khỏi bảng nhập trên giao diện
            var removedCount = 0;
            if (model.ReplaceAll)
            {
                // Xác thực chỉ số xét nghiệm có nằm trong danh mục chuẩn hay không
                var resultsToRemove = existingResults.Values
                    .Where(existingItem => !submittedIndicatorIdSet.Contains(existingItem.IndicatorId))
                    .ToList();

                if (resultsToRemove.Count > 0)
                {
                    _dataContext.VcLabTestResults.RemoveRange(resultsToRemove);
                    removedCount = resultsToRemove.Count;
                }
            }

            foreach (var item in incomingResults)
            {
                if (!indicatorsMap.TryGetValue(item.IndicatorId, out var indicator))
                {
                    throw new ArgumentException($"Chỉ số xét nghiệm ID {item.IndicatorId} không tồn tại trong danh mục.");
                }

                // Tự động chuyển đổi giá trị chuỗi ResultValue sang số thập phân (hỗ trợ cả dấu phẩy và dấu chấm) nếu client không truyền NumericValue
                decimal? numVal = item.NumericValue;
                if (!numVal.HasValue && decimal.TryParse(
                        item.ResultValue.Trim().Replace(",", "."),
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out var parsed))
                {
                    numVal = parsed;
                }

                // Tự động tính cờ bất thường (HIGH / LOW / NORMAL) bằng cách so sánh với ref_min / ref_max nếu không truyền cờ tường minh
                string flag = !string.IsNullOrWhiteSpace(item.Flag)
                    ? item.Flag.Trim().ToUpperInvariant()
                    : CalculateFlag(numVal, indicator.RefMin, indicator.RefMax);

                // Tự động sinh ghi chú nhanh ("Tăng" / "Giảm") khi chỉ số vượt ngưỡng mà bác sĩ không nhập ghi chú riêng
                string? note = item.Note;
                if (string.IsNullOrWhiteSpace(note))
                {
                    note = flag == "HIGH" ? "Tăng" : flag == "LOW" ? "Giảm" : null;
                }

                if (existingResults.TryGetValue(item.IndicatorId, out var existing))
                {
                    // Trường hợp chỉ số đã tồn tại trong phiếu: cập nhật lại giá trị đo, cờ đánh giá và ghi chú
                    existing.ResultValue = item.ResultValue.Trim();
                    existing.NumericValue = numVal;
                    existing.Flag = flag;
                    existing.Note = note;
                    _dataContext.VcLabTestResults.Update(existing);
                }
                else
                {
                    // Trường hợp chỉ số chưa có trong phiếu: tạo mới bản ghi kết quả xét nghiệm
                    var newResult = new VcLabTestResults
                    {
                        OrderId = model.OrderId,
                        IndicatorId = item.IndicatorId,
                        ResultValue = item.ResultValue.Trim(),
                        NumericValue = numVal,
                        Flag = flag,
                        Note = note,
                        CreatedAt = DateTime.Now
                    };
                    await _dataContext.VcLabTestResults.AddAsync(newResult, cancellationToken);
                }
            }

            // Cập nhật trạng thái tổng thể của phiếu chỉ định (mặc định chuyển sang COMPLETED sau khi đã có kết quả)
            if (incomingResults.Count == 0 && model.ReplaceAll)
            {
                order.Status = "PENDING";
                _dataContext.VcLabTestOrders.Update(order);
            }
            else if (!string.IsNullOrWhiteSpace(model.OrderStatus))
            {
                order.Status = model.OrderStatus.Trim().ToUpperInvariant();
                _dataContext.VcLabTestOrders.Update(order);
            }

            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region Xóa Cache
            // Xóa bộ nhớ đệm kết quả xét nghiệm của hồ sơ khám để dữ liệu mới hiển thị ngay lập tức
            _cacheService.Remove(LabTestConstant.BuildCacheKey(order.RecordId.ToString()));
            #endregion

            Log.Information($"Đồng bộ kết quả xét nghiệm cho phiếu OrderId {model.OrderId}: lưu {incomingResults.Count} chỉ số, xóa {removedCount} chỉ số.");

            return Unit.Value;
        }

        /// <summary>
        /// Hàm phụ trợ đối chiếu giá trị đo định lượng với ngưỡng cận dưới (refMin) và cận trên (refMax) để trả về cờ LOW / HIGH / NORMAL
        /// </summary>
        private static string CalculateFlag(decimal? value, decimal? refMin, decimal? refMax)
        {
            if (!value.HasValue) return "NORMAL";
            if (refMin.HasValue && value.Value < refMin.Value) return "LOW";
            if (refMax.HasValue && value.Value > refMax.Value) return "HIGH";
            return "NORMAL";
        }
    }
}
