/**
 * Nghiệp vụ xóa phiếu chỉ định xét nghiệm (DeleteLabTestOrderCommand):
 * Kiểm tra phân quyền, tìm phiếu chỉ định kèm toàn bộ chỉ số kết quả con bên trong,
 * xóa dữ liệu an toàn theo thứ tự bảng con -> bảng cha trong CSDL và xóa Cache.
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.LabTests.LabTestCommands;

/// <summary>
/// Lệnh xóa phiếu chỉ định xét nghiệm và toàn bộ các dòng chỉ số kết quả thuộc phiếu đó (vc_lab_test_orders & vc_lab_test_results)
/// </summary>
public class DeleteLabTestOrderCommand : IRequest<Unit>
{
    public int OrderId { get; }

    public DeleteLabTestOrderCommand(int orderId)
    {
        OrderId = orderId;
    }

    public class Handler : IRequestHandler<DeleteLabTestOrderCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;      // DbContext kết nối CSDL để xóa phiếu và kết quả xét nghiệm
        private readonly ICacheService _cacheService;          // Dịch vụ quản lý và xóa bộ nhớ đệm (Cache)
        private readonly IContextAccessor _contextAccessor;    // Dịch vụ trích xuất thông tin người dùng đang đăng nhập (Role, UserId)

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        /// <summary>
        /// Xử lý xóa phiếu xét nghiệm: Kiểm tra quyền, tải phiếu cùng các kết quả con, xóa khỏi CSDL và làm mới Cache
        /// </summary>
        public async Task<Unit> Handle(DeleteLabTestOrderCommand request, CancellationToken cancellationToken)
        {
            Log.Information($"Bắt đầu xóa phiếu chỉ định xét nghiệm OrderId: {request.OrderId}");

            #region Phân quyền
            // Chỉ Bác sĩ (DOCTOR), Quản trị viên y tế (HEALTH_ADMIN) hoặc Quản trị hệ thống (SUPER_ADMIN) mới được xóa phiếu xét nghiệm
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Chỉ Bác sĩ hoặc Quản trị viên y tế mới có quyền xóa phiếu xét nghiệm.");
            }
            #endregion

            #region Kiểm tra dữ liệu tồn tại
            // Truy vấn phiếu chỉ định kèm toàn bộ danh sách chỉ số kết quả con (Include)
            var order = await _dataContext.VcLabTestOrders
                .Include(o => o.Results)
                .FirstOrDefaultAsync(x => x.OrderId == request.OrderId, cancellationToken);
            if (order == null)
            {
                throw new ArgumentException($"Không tìm thấy phiếu chỉ định xét nghiệm ID: {request.OrderId}");
            }
            #endregion

            #region Xóa dữ liệu trong CSDL
            // Lưu lại RecordId trước khi xóa để dùng xóa Cache tương ứng
            int recordId = order.RecordId;

            // Xóa toàn bộ các dòng kết quả chỉ số con (bảng vc_lab_test_results) trước để tránh lỗi ràng buộc khóa ngoại
            if (order.Results.Count > 0)
            {
                _dataContext.VcLabTestResults.RemoveRange(order.Results);
            }

            // Xóa phiếu chỉ định xét nghiệm cha (bảng vc_lab_test_orders)
            _dataContext.VcLabTestOrders.Remove(order);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region Xóa Cache
            // Xóa bộ nhớ đệm kết quả xét nghiệm theo ID hồ sơ khám
            _cacheService.Remove(LabTestConstant.BuildCacheKey(recordId.ToString()));
            #endregion

            Log.Information($"Xóa phiếu xét nghiệm OrderId {request.OrderId} (thuộc hồ sơ RecordId {recordId}) thành công.");

            return Unit.Value;
        }
    }
}
