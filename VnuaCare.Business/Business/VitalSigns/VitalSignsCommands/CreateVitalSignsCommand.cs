/**
 * Nghiệp vụ Thêm mới chỉ số sinh tồn và thể lực (CreateVitalSignsCommand)
 * Chuẩn CRUD: Thêm mới bản ghi chỉ số sinh tồn tại bàn đón tiếp ban đầu
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

namespace VnuaCare.Business.Business.VitalSigns.VitalSignsCommands;

public class CreateVitalSignsCommand : IRequest<Unit>
{
    public CreateVitalSignsModel Model { get; }

    public CreateVitalSignsCommand(CreateVitalSignsModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<CreateVitalSignsCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để lưu chỉ số sinh tồn và thể lực
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi thêm mới
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu, tự động tính BMI và lưu chỉ số sinh tồn vào CSDL
        public async Task<Unit> Handle(CreateVitalSignsCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu thêm mới chỉ số sinh tồn và thể lực: " + JsonSerializer.Serialize(model));

            #region Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.STAFF.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thêm mới chỉ số sinh tồn và thể lực.");
            }
            #endregion

            #region Kiểm tra tồn tại
            var record = await _dataContext.VcHealthCheckupRecords
                .FirstOrDefaultAsync(x => x.RecordId == model.RecordId, cancellationToken);
            if (record == null)
            {
                throw new ArgumentException($"Không tìm thấy hồ sơ khám sức khỏe với ID: {model.RecordId}");
            }
            #endregion

            #region Kiểm tra trùng lặp
            var exists = await _dataContext.VcVitalSigns
                .AnyAsync(x => x.RecordId == model.RecordId, cancellationToken);
            if (exists)
            {
                throw new ArgumentException($"Hồ sơ khám ID {model.RecordId} đã có dữ liệu chỉ số sinh tồn. Vui lòng sử dụng chức năng cập nhật (Update).");
            }
            #endregion

            #region Tính toán BMI
            decimal? calculatedBmi = model.Bmi;
            if ((!calculatedBmi.HasValue || calculatedBmi.Value <= 0) &&
                model.HeightCm.HasValue && model.HeightCm.Value > 0 &&
                model.WeightKg.HasValue && model.WeightKg.Value > 0)
            {
                decimal heightM = model.HeightCm.Value / 100m;
                calculatedBmi = Math.Round(model.WeightKg.Value / (heightM * heightM), 2);
            }
            #endregion

            #region Xác định người đo
            int? measuredBy = model.MeasuredBy;
            if (!measuredBy.HasValue && currentUserRole == Role.DOCTOR.ToString())
            {
                var currentUserId = _contextAccessor.UserId;
                if (currentUserId.HasValue)
                {
                    var doctor = await _dataContext.VcDoctors
                        .AsNoTracking()
                        .FirstOrDefaultAsync(d => d.UserId == currentUserId.Value, cancellationToken);
                    if (doctor != null)
                    {
                        measuredBy = doctor.DoctorId;
                    }
                }
            }
            #endregion

            #region Lưu vào CSDL
            var vital = new VcVitalSigns
            {
                RecordId = model.RecordId,
                HeightCm = model.HeightCm,
                WeightKg = model.WeightKg,
                Bmi = calculatedBmi,
                WaistCm = model.WaistCm,
                SystolicBp = model.SystolicBp,
                DiastolicBp = model.DiastolicBp,
                HeartRateBpm = model.HeartRateBpm,
                RespiratoryRate = model.RespiratoryRate,
                Spo2Percent = model.Spo2Percent,
                MeasuredBy = measuredBy,
                CreatedAt = DateTime.Now
            };

            await _dataContext.VcVitalSigns.AddAsync(vital, cancellationToken);

            // Chuyển trạng thái hồ sơ sang IN_PROGRESS nếu đang là PENDING
            if (record.Status == "PENDING")
            {
                record.Status = "IN_PROGRESS";
                _dataContext.VcHealthCheckupRecords.Update(record);
            }

            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region Xóa Cache
            _cacheService.Remove(VitalSignsConstant.BuildCacheKey(model.RecordId.ToString()));
            #endregion

            Log.Information($"Thêm mới chỉ số sinh tồn cho hồ sơ ID {model.RecordId} thành công.");
            return Unit.Value;
        }
    }
}
