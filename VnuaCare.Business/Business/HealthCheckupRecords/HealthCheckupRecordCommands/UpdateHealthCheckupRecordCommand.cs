/**
 * Nghiệp vụ cập nhật Hồ sơ khám sức khỏe: Cập nhật kết luận khám, phân loại sức khỏe,
 * thông tin tiền sử bệnh, bác sĩ chủ trì kết luận và trạng thái hồ sơ.
 */

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.HealthCheckupRecords.CheckupRecordCommands;

/// <summary>
/// Lệnh cập nhật kết luận và thông tin hồ sơ khám sức khỏe
/// </summary>
public class UpdateHealthCheckupRecordCommand : IRequest<Unit>
{
    public UpdateHealthCheckupRecordModel Model { get; set; }

    public UpdateHealthCheckupRecordCommand(UpdateHealthCheckupRecordModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<UpdateHealthCheckupRecordCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // DbContext kết nối CSDL để cập nhật hồ sơ khám
        private readonly ICacheService _cacheService;            // Dịch vụ quản lý và làm mới bộ nhớ đệm (Cache)
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất thông tin người dùng đang thực hiện (Role, UserId)

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
        /// Xử lý cập nhật hồ sơ khám sức khỏe: Kiểm tra quyền, xác thực dữ liệu, cập nhật và làm mới cache
        /// </summary>
        public async Task<Unit> Handle(UpdateHealthCheckupRecordCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information($"Bắt đầu cập nhật hồ sơ khám sức khỏe ID: {model.RecordId} - {JsonSerializer.Serialize(model)}");

            #region 1. Phân quyền người thực hiện (Authorization)
            // Cho phép SUPER_ADMIN, HEALTH_ADMIN và DOCTOR cập nhật hồ sơ khám sức khỏe
            var currentUserRole = _contextAccessor.Role;

            if (currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.DOCTOR.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật hồ sơ khám sức khỏe (Yêu cầu quyền Bác sĩ hoặc Quản trị viên).");
            }
            #endregion

            #region 2. Kiểm tra tính hợp lệ dữ liệu đầu vào (Validation)
            if (model.RecordId <= 0)
            {
                throw new ArgumentException("ID hồ sơ khám sức khỏe không hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(model.SidCode))
            {
                throw new ArgumentException("Mã vạch hồ sơ (SID) không được để trống.");
            }
            #endregion

            #region 3. Kiểm tra sự tồn tại của hồ sơ trong CSDL
            var entity = await _dataContext.VcHealthCheckupRecords
                .FirstOrDefaultAsync(x => x.RecordId == model.RecordId, cancellationToken);

            if (entity == null)
            {
                throw new ArgumentException($"Không tìm thấy hồ sơ khám sức khỏe có ID '{model.RecordId}'.");
            }
            #endregion

            #region 4. Kiểm tra trùng lặp mã vạch SID (nếu SID bị thay đổi)
            var sidTrimmed = model.SidCode.Trim();
            if (!string.Equals(entity.SidCode, sidTrimmed, StringComparison.OrdinalIgnoreCase))
            {
                var isSidExisted = await _dataContext.VcHealthCheckupRecords
                    .AnyAsync(x => x.SidCode == sidTrimmed && x.RecordId != model.RecordId, cancellationToken);

                if (isSidExisted)
                {
                    throw new ArgumentException($"Mã vạch hồ sơ (SID) '{model.SidCode}' đã được sử dụng bởi một hồ sơ khác.");
                }
            }
            #endregion

            #region 5. Kiểm tra bác sĩ ký kết luận (nếu có cập nhật ConcludedByDoctorId)
            if (model.ConcludedByDoctorId > 0)
            {
                var doctorExists = await _dataContext.VcDoctors
                    .AnyAsync(x => x.DoctorId == model.ConcludedByDoctorId, cancellationToken);
                if (!doctorExists)
                {
                    throw new ArgumentException($"Bác sĩ kết luận với ID '{model.ConcludedByDoctorId}' không tồn tại trên hệ thống.");
                }
            }
            #endregion

            #region 6. Cập nhật dữ liệu vào thực thể và lưu CSDL
            model.UpdateEntity(entity);

            _dataContext.VcHealthCheckupRecords.Update(entity);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region 7. Xóa bộ nhớ đệm (Cache Invalidation)
            _cacheService.Remove(HealthCheckupRecordConstant.BuildCacheKey(entity.RecordId.ToString()));
            _cacheService.Remove(HealthCheckupRecordConstant.BuildCacheKey());
            #endregion

            Log.Information($"Cập nhật hồ sơ khám sức khỏe thành công - RecordId: {entity.RecordId}, SID: {entity.SidCode}");

            return Unit.Value;
        }
    }
}