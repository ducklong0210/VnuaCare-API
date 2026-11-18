/**
 * Nghiệp vụ Thêm mới kết quả khám lâm sàng Nội khoa tổng quát (CreateInternalMedicineExamCommand)
 * Chuẩn CRUD: Thêm mới bản ghi kết quả khám nội khoa
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

namespace VnuaCare.Business.Business.InternalMedicine.InternalMedicineCommands;

public class CreateInternalMedicineExamCommand : IRequest<Unit>
{
    public CreateInternalMedicineExamModel Model { get; }

    public CreateInternalMedicineExamCommand(CreateInternalMedicineExamModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<CreateInternalMedicineExamCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để lưu kết quả khám lâm sàng nội khoa
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi thêm mới
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu, bác sĩ khám và lưu kết quả khám nội khoa vào CSDL
        public async Task<Unit> Handle(CreateInternalMedicineExamCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu thêm mới kết quả khám lâm sàng nội khoa: " + JsonSerializer.Serialize(model));

            #region Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Chỉ Bác sĩ hoặc Quản trị viên y tế mới có quyền thêm kết quả khám nội khoa.");
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

            #region Xác định bác sĩ khám
            int? doctorId = model.ExaminingDoctorId;
            if ((!doctorId.HasValue || doctorId <= 0) && currentUserRole == Role.DOCTOR.ToString())
            {
                var currentUserId = _contextAccessor.UserId;
                if (currentUserId.HasValue)
                {
                    var doctor = await _dataContext.VcDoctors
                        .AsNoTracking()
                        .FirstOrDefaultAsync(d => d.UserId == currentUserId.Value, cancellationToken);
                    if (doctor != null)
                    {
                        doctorId = doctor.DoctorId;
                    }
                }
            }
            int? finalDoctorId = (doctorId.HasValue && doctorId.Value > 0) ? doctorId.Value : null;
            #endregion

            #region Lưu hoặc Cập nhật vào CSDL (Upsert)
            var existingExam = await _dataContext.VcInternalMedicineExams
                .FirstOrDefaultAsync(x => x.RecordId == model.RecordId, cancellationToken);

            if (existingExam != null)
            {
                existingExam.CirculatoryDesc = model.CirculatoryDesc;
                existingExam.RespiratoryDesc = model.RespiratoryDesc;
                existingExam.DigestiveDesc = model.DigestiveDesc;
                existingExam.UrinaryDesc = model.UrinaryDesc;
                existingExam.MusculoskeletalDesc = model.MusculoskeletalDesc;
                existingExam.NeurologicalDesc = model.NeurologicalDesc;
                existingExam.Conclusion = model.Conclusion;
                existingExam.HealthClass = model.HealthClass;
                if (finalDoctorId.HasValue)
                {
                    existingExam.ExaminingDoctorId = finalDoctorId.Value;
                }

                _dataContext.VcInternalMedicineExams.Update(existingExam);
            }
            else
            {
                var exam = new VcInternalMedicineExams
                {
                    RecordId = model.RecordId,
                    CirculatoryDesc = model.CirculatoryDesc,
                    RespiratoryDesc = model.RespiratoryDesc,
                    DigestiveDesc = model.DigestiveDesc,
                    UrinaryDesc = model.UrinaryDesc,
                    MusculoskeletalDesc = model.MusculoskeletalDesc,
                    NeurologicalDesc = model.NeurologicalDesc,
                    Conclusion = model.Conclusion,
                    HealthClass = model.HealthClass,
                    ExaminingDoctorId = finalDoctorId,
                    CreatedAt = DateTime.Now
                };

                await _dataContext.VcInternalMedicineExams.AddAsync(exam, cancellationToken);
            }

            // Chuyển trạng thái hồ sơ sang IN_PROGRESS nếu đang là PENDING
            if (record.Status == "PENDING")
            {
                record.Status = "IN_PROGRESS";
                _dataContext.VcHealthCheckupRecords.Update(record);
            }

            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region Xóa Cache
            _cacheService.Remove(InternalMedicineConstant.BuildCacheKey(model.RecordId.ToString()));
            #endregion

            Log.Information($"Thêm mới kết quả khám nội khoa cho hồ sơ {model.RecordId} thành công.");
            return Unit.Value;
        }
    }
}
