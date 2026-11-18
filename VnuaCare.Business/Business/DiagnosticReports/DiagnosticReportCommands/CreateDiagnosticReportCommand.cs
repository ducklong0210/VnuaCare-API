/**
 * Nghiệp vụ Thêm mới phiếu kết quả chẩn đoán hình ảnh (CreateDiagnosticReportCommand)
 * Chuẩn CRUD: Dành cho Bác sĩ Chẩn đoán hình ảnh, Bác sĩ thăm dò chức năng hoặc Quản trị viên
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

namespace VnuaCare.Business.Business.DiagnosticReports.DiagnosticReportCommands;

public class CreateDiagnosticReportCommand : IRequest<Unit>
{
    public CreateDiagnosticReportModel Model { get; }

    public CreateDiagnosticReportCommand(CreateDiagnosticReportModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<CreateDiagnosticReportCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để lưu phiếu kết quả chẩn đoán hình ảnh
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi thêm mới
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu và lưu phiếu chẩn đoán hình ảnh vào CSDL
        public async Task<Unit> Handle(CreateDiagnosticReportCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu thêm mới phiếu kết quả chẩn đoán hình ảnh: " + JsonSerializer.Serialize(model));

            #region Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Chỉ Bác sĩ hoặc Quản trị viên y tế mới có quyền thêm phiếu chẩn đoán hình ảnh.");
            }
            #endregion

            #region Kiểm tra tồn tại hồ sơ
            var record = await _dataContext.VcHealthCheckupRecords
                .FirstOrDefaultAsync(x => x.RecordId == model.RecordId, cancellationToken);
            if (record == null)
            {
                throw new ArgumentException($"Không tìm thấy hồ sơ khám sức khỏe với ID: {model.RecordId}");
            }
            #endregion

            #region Xác định bác sĩ đọc kết quả
            int? readingDoctorId = model.ReadingDoctorId;
            if ((!readingDoctorId.HasValue || readingDoctorId <= 0) && currentUserRole == Role.DOCTOR.ToString())
            {
                var currentUserId = _contextAccessor.UserId;
                if (currentUserId.HasValue)
                {
                    var doctor = await _dataContext.VcDoctors
                        .AsNoTracking()
                        .FirstOrDefaultAsync(d => d.UserId == currentUserId.Value, cancellationToken);
                    if (doctor != null)
                    {
                        readingDoctorId = doctor.DoctorId;
                    }
                }
            }
            int? finalReadingDoctorId = (readingDoctorId.HasValue && readingDoctorId.Value > 0) ? readingDoctorId.Value : null;
            int? finalApprovingDoctorId = (model.ApprovingDoctorId.HasValue && model.ApprovingDoctorId.Value > 0) ? model.ApprovingDoctorId.Value : null;
            #endregion

            #region Lưu vào CSDL
            var report = new VcDiagnosticReports
            {
                RecordId = model.RecordId,
                ModalityType = model.ModalityType.Trim().ToUpperInvariant(),
                ExamTitle = model.ExamTitle.Trim(),
                TechniqueDescription = model.TechniqueDescription,
                FindingsData = model.FindingsData,
                Conclusion = model.Conclusion.Trim(),
                ImageUrls = model.ImageUrls,
                ReadingDoctorId = finalReadingDoctorId,
                ApprovingDoctorId = finalApprovingDoctorId,
                PerformedAt = model.PerformedAt ?? DateTime.Now
            };

            await _dataContext.VcDiagnosticReports.AddAsync(report, cancellationToken);

            // Chuyển trạng thái hồ sơ sang IN_PROGRESS nếu đang là PENDING
            if (record.Status == "PENDING")
            {
                record.Status = "IN_PROGRESS";
                _dataContext.VcHealthCheckupRecords.Update(record);
            }

            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region Xóa Cache
            _cacheService.Remove(DiagnosticReportConstant.BuildCacheKey(model.RecordId.ToString()));
            #endregion

            Log.Information($"Thêm mới phiếu kết quả chẩn đoán hình ảnh {model.ExamTitle} cho hồ sơ {model.RecordId} thành công.");
            return Unit.Value;
        }
    }
}
