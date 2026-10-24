/**
 * Nghiệp vụ xóa Hồ sơ khám sức khỏe: Kiểm tra quyền quản trị, kiểm tra ràng buộc dữ liệu
 * (kết quả khám nội khoa lâm sàng), xóa hồ sơ khỏi CSDL và giải phóng bộ nhớ đệm.
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.HealthCheckupRecords.CheckupRecordCommands;

/// <summary>
/// Lệnh xóa hồ sơ khám sức khỏe (Health Checkup Record)
/// </summary>
public class DeleteHealthCheckupRecordCommand : IRequest<Unit>
{
    public int RecordId { get; init; }

    public DeleteHealthCheckupRecordCommand(int recordId)
    {
        RecordId = recordId;
    }

    public class Handler : IRequestHandler<DeleteHealthCheckupRecordCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // DbContext kết nối CSDL để tìm kiếm và xóa hồ sơ
        private readonly ICacheService _cacheService;            // Dịch vụ quản lý bộ nhớ đệm
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất quyền người dùng

        public Handler(
            VnuaCareDataContext dataContext, 
            ICacheService cacheService,
            Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        /// <summary>
        /// Xử lý xóa hồ sơ khám sức khỏe và các kết quả khám lâm sàng liên quan
        /// </summary>
        public async Task<Unit> Handle(DeleteHealthCheckupRecordCommand request, CancellationToken cancellationToken)
        {
            var recordId = request.RecordId;
            Log.Information($"Bắt đầu xóa hồ sơ khám sức khỏe ID: {recordId}");

            #region 1. Phân quyền người thực hiện (Authorization)
            // Chỉ Quản trị viên y tế hoặc Quản trị hệ thống mới có quyền xóa hồ sơ khám
            var currentUserRole = _contextAccessor.Role;

            if (currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa hồ sơ khám sức khỏe (Yêu cầu quyền Quản trị viên).");
            }
            #endregion

            #region 2. Kiểm tra sự tồn tại của hồ sơ trong CSDL
            var record = await _dataContext.VcHealthCheckupRecords
                .FirstOrDefaultAsync(x => x.RecordId == recordId, cancellationToken);

            if (record == null)
            {
                throw new ArgumentException($"Không tìm thấy hồ sơ khám sức khỏe có ID '{recordId}'.");
            }
            #endregion

            #region 3. Xử lý các liên kết khóa ngoại (Cascade Delete dữ liệu khám lâm sàng)
            // Xóa các kết quả khám lâm sàng nội khoa liên kết với hồ sơ nếu có
            var internalMedicineExams = await _dataContext.VcInternalMedicineExams
                .Where(x => x.RecordId == recordId)
                .ToListAsync(cancellationToken);

            if (internalMedicineExams.Any())
            {
                _dataContext.VcInternalMedicineExams.RemoveRange(internalMedicineExams);
            }

            // Xóa bản ghi hồ sơ khám chính
            _dataContext.VcHealthCheckupRecords.Remove(record);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region 4. Xóa bộ nhớ đệm (Cache Invalidation)
            _cacheService.Remove(HealthCheckupRecordConstant.BuildCacheKey(recordId.ToString()));
            _cacheService.Remove(HealthCheckupRecordConstant.BuildCacheKey());
            #endregion

            Log.Information($"Xóa thành công hồ sơ khám sức khỏe ID: {recordId} (SID: {record.SidCode})");

            return Unit.Value;
        }
    }
}