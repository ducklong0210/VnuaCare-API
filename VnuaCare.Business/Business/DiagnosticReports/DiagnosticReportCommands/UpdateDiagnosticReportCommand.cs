/**
 * Nghiệp vụ Cập nhật phiếu kết quả chẩn đoán hình ảnh (UpdateDiagnosticReportCommand)
 * Chuẩn CRUD: Cập nhật thông tin phiếu kết quả chẩn đoán hình ảnh
 */

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.DiagnosticReports.DiagnosticReportCommands;

public class UpdateDiagnosticReportCommand : IRequest<Unit>
{
    public UpdateDiagnosticReportModel Model { get; }

    public UpdateDiagnosticReportCommand(UpdateDiagnosticReportModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<UpdateDiagnosticReportCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để cập nhật phiếu chẩn đoán hình ảnh
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi cập nhật
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu và cập nhật phiếu chẩn đoán hình ảnh
        public async Task<Unit> Handle(UpdateDiagnosticReportCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu cập nhật phiếu kết quả chẩn đoán hình ảnh: " + JsonSerializer.Serialize(model));

            #region Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Chỉ Bác sĩ hoặc Quản trị viên y tế mới có quyền cập nhật phiếu chẩn đoán hình ảnh.");
            }
            #endregion

            #region Kiểm tra tồn tại
            var report = await _dataContext.VcDiagnosticReports
                .FirstOrDefaultAsync(x => x.ReportId == model.ReportId, cancellationToken);
            if (report == null)
            {
                throw new ArgumentException($"Không tìm thấy phiếu kết quả chẩn đoán hình ảnh với ID: {model.ReportId}");
            }
            #endregion

            #region Cập nhật CSDL
            model.UpdateEntity(report);
            _dataContext.VcDiagnosticReports.Update(report);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region Xóa Cache
            _cacheService.Remove(DiagnosticReportConstant.BuildCacheKey(report.RecordId.ToString()));
            #endregion

            Log.Information($"Cập nhật phiếu kết quả chẩn đoán hình ảnh ID {model.ReportId} thành công.");
            return Unit.Value;
        }
    }
}
