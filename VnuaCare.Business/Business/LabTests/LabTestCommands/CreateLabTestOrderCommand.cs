/**
 * Nghiệp vụ tạo mới phiếu chỉ định xét nghiệm (CreateLabTestOrderCommand):
 * Kiểm tra phân quyền, xác thực hồ sơ khám liên kết, tạo phiếu mẫu bệnh phẩm (Máu/Nước tiểu),
 * tự động chuyển trạng thái hồ sơ sang đang khám (IN_PROGRESS), lưu vào CSDL và xóa Cache.
 */

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
/// Lệnh tạo mới phiếu chỉ định xét nghiệm cận lâm sàng gắn với hồ sơ khám (vc_lab_test_orders)
/// </summary>
public class CreateLabTestOrderCommand : IRequest<int>
{
    public CreateLabTestOrderModel Model { get; }

    public CreateLabTestOrderCommand(CreateLabTestOrderModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<CreateLabTestOrderCommand, int>
    {
        private readonly VnuaCareDataContext _dataContext;      // DbContext kết nối CSDL để ghi phiếu chỉ định xét nghiệm
        private readonly ICacheService _cacheService;          // Dịch vụ quản lý và xóa bộ nhớ đệm (Cache)
        private readonly IContextAccessor _contextAccessor;    // Dịch vụ lấy thông tin người dùng đang đăng nhập (Role, UserId)

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        /// <summary>
        /// Xử lý tạo mới phiếu chỉ định xét nghiệm: Kiểm tra quyền, xác thực hồ sơ, lưu phiếu và làm mới Cache
        /// </summary>
        public async Task<int> Handle(CreateLabTestOrderCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu tạo phiếu chỉ định xét nghiệm mới: " + JsonSerializer.Serialize(model));

            #region Phân quyền
            // Chỉ Bác sĩ (DOCTOR), Quản trị viên y tế (HEALTH_ADMIN) hoặc Quản trị hệ thống (SUPER_ADMIN) mới được tạo phiếu chỉ định
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Chỉ Bác sĩ hoặc Quản trị viên y tế mới có quyền tạo phiếu chỉ định xét nghiệm.");
            }
            #endregion

            #region Kiểm tra tính hợp lệ dữ liệu
            // Kiểm tra ID hồ sơ khám sức khỏe hợp lệ
            if (model.RecordId <= 0)
            {
                throw new ArgumentException("ID hồ sơ khám sức khỏe không hợp lệ.");
            }

            // Kiểm tra loại mẫu bệnh phẩm không được bỏ trống
            if (string.IsNullOrWhiteSpace(model.SampleType))
            {
                throw new ArgumentException("Loại mẫu bệnh phẩm không được để trống (ví dụ: MÁU, NƯỚC TIỂU).");
            }
            #endregion

            #region Kiểm tra dữ liệu liên quan
            // Xác thực hồ sơ khám sức khỏe có tồn tại trong hệ thống hay không
            var record = await _dataContext.VcHealthCheckupRecords
                .FirstOrDefaultAsync(x => x.RecordId == model.RecordId, cancellationToken);
            if (record == null)
            {
                throw new ArgumentException($"Không tìm thấy hồ sơ khám sức khỏe với ID: {model.RecordId}");
            }
            #endregion

            #region Lưu vào CSDL
            // Khởi tạo thực thể phiếu chỉ định xét nghiệm mới (mã vạch mặc định kế thừa mã SID nếu không truyền riêng)
            var order = new VcLabTestOrders
            {
                RecordId = model.RecordId,
                ServiceId = (model.ServiceId.HasValue && model.ServiceId.Value > 0) ? model.ServiceId.Value : null,
                SampleBarcode = string.IsNullOrWhiteSpace(model.SampleBarcode) ? record.SidCode : model.SampleBarcode.Trim(),
                SampleType = model.SampleType.Trim().ToUpperInvariant(),
                CollectedAt = model.CollectedAt ?? DateTime.Now,
                ReceivedAt = model.ReceivedAt ?? DateTime.Now,
                Status = string.IsNullOrWhiteSpace(model.Status) ? "PENDING" : model.Status.Trim().ToUpperInvariant()
            };

            await _dataContext.VcLabTestOrders.AddAsync(order, cancellationToken);

            // Nếu hồ sơ khám đang ở trạng thái chờ (PENDING), tự động chuyển sang đang khám (IN_PROGRESS)
            if (record.Status == "PENDING")
            {
                record.Status = "IN_PROGRESS";
                _dataContext.VcHealthCheckupRecords.Update(record);
            }

            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region Xóa Cache
            // Xóa bộ nhớ đệm danh sách phiếu xét nghiệm của hồ sơ để lần đọc tiếp theo lấy dữ liệu mới nhất
            _cacheService.Remove(LabTestConstant.BuildCacheKey(model.RecordId.ToString()));
            #endregion

            Log.Information($"Tạo phiếu chỉ định xét nghiệm ID {order.OrderId} cho hồ sơ {model.RecordId} thành công.");

            return order.OrderId;
        }
    }
}
