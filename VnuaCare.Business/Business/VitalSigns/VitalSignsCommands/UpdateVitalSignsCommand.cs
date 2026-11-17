/**
 * Nghiệp vụ Cập nhật chỉ số sinh tồn và thể lực (UpdateVitalSignsCommand)
 * Chuẩn CRUD: Cập nhật thông tin bản ghi chỉ số sinh tồn
 */

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.VitalSigns.VitalSignsCommands;

public class UpdateVitalSignsCommand : IRequest<Unit>
{
    public UpdateVitalSignsModel Model { get; }

    public UpdateVitalSignsCommand(UpdateVitalSignsModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<UpdateVitalSignsCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để cập nhật chỉ số sinh tồn và thể lực
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi cập nhật
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu, tự động tính lại BMI và cập nhật chỉ số sinh tồn
        public async Task<Unit> Handle(UpdateVitalSignsCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu cập nhật chỉ số sinh tồn và thể lực: " + JsonSerializer.Serialize(model));

            #region Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.STAFF.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật chỉ số sinh tồn và thể lực.");
            }
            #endregion

            #region Kiểm tra tồn tại
            var vital = await _dataContext.VcVitalSigns
                .FirstOrDefaultAsync(x => x.VitalSignId == model.VitalSignId, cancellationToken);
            if (vital == null)
            {
                throw new ArgumentException($"Không tìm thấy bản ghi chỉ số sinh tồn với ID: {model.VitalSignId}");
            }
            #endregion

            #region Tính toán BMI
            decimal? calculatedBmi = model.Bmi;
            decimal height = model.HeightCm ?? vital.HeightCm ?? 0;
            decimal weight = model.WeightKg ?? vital.WeightKg ?? 0;
            if ((!calculatedBmi.HasValue || calculatedBmi.Value <= 0) && height > 0 && weight > 0)
            {
                decimal heightM = height / 100m;
                calculatedBmi = Math.Round(weight / (heightM * heightM), 2);
            }
            #endregion

            #region Cập nhật CSDL
            model.UpdateEntity(vital, calculatedBmi);
            _dataContext.VcVitalSigns.Update(vital);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region Xóa Cache
            _cacheService.Remove(VitalSignsConstant.BuildCacheKey(vital.RecordId.ToString()));
            #endregion

            Log.Information($"Cập nhật chỉ số sinh tồn ID {model.VitalSignId} thành công.");
            return Unit.Value;
        }
    }
}
