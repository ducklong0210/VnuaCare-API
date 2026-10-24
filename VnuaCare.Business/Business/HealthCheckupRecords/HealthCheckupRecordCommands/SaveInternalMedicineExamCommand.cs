/**
 * Nghiệp vụ Lưu kết quả khám lâm sàng Nội khoa tổng quát (SaveInternalMedicineExamCommand)
 * Dành cho Bác sĩ thực hiện ghi nhận các chỉ số và kết luận chuyên khoa nội tổng quát
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

namespace VnuaCare.Business.Business.HealthCheckupRecords.CheckupRecordCommands;

public class SaveInternalMedicineExamCommand : IRequest<Unit>
{
    public SaveInternalMedicineExamModel Model { get; }

    public SaveInternalMedicineExamCommand(SaveInternalMedicineExamModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<SaveInternalMedicineExamCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;
        private readonly ICacheService _cacheService;
        private readonly IContextAccessor _contextAccessor;

        public Handler(
            VnuaCareDataContext dataContext,
            ICacheService cacheService,
            Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        public async Task<Unit> Handle(SaveInternalMedicineExamCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu lưu kết quả khám lâm sàng nội khoa: " + JsonSerializer.Serialize(model));

            #region 1. Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Chỉ Bác sĩ hoặc Quản trị viên y tế mới có quyền ghi nhận kết quả khám nội khoa.");
            }
            #endregion

            #region 2. Kiểm tra hồ sơ khám tồn tại
            var record = await _dataContext.VcHealthCheckupRecords
                .FirstOrDefaultAsync(x => x.RecordId == model.RecordId, cancellationToken);
            if (record == null)
            {
                throw new ArgumentException($"Không tìm thấy hồ sơ khám sức khỏe với ID: {model.RecordId}");
            }
            #endregion

            #region 3. Xác định Bác sĩ khám
            int? doctorId = model.ExaminingDoctorId;
            if (!doctorId.HasValue && currentUserRole == Role.DOCTOR.ToString())
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
            #endregion

            #region 4. Lưu hoặc cập nhật kết quả khám nội khoa
            var exam = await _dataContext.VcInternalMedicineExams
                .FirstOrDefaultAsync(x => x.RecordId == model.RecordId, cancellationToken);

            if (exam == null)
            {
                // Thêm mới
                exam = new VcInternalMedicineExams
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
                    ExaminingDoctorId = doctorId,
                    CreatedAt = DateTime.Now
                };
                await _dataContext.VcInternalMedicineExams.AddAsync(exam, cancellationToken);
            }
            else
            {
                // Cập nhật
                exam.CirculatoryDesc = model.CirculatoryDesc;
                exam.RespiratoryDesc = model.RespiratoryDesc;
                exam.DigestiveDesc = model.DigestiveDesc;
                exam.UrinaryDesc = model.UrinaryDesc;
                exam.MusculoskeletalDesc = model.MusculoskeletalDesc;
                exam.NeurologicalDesc = model.NeurologicalDesc;
                exam.Conclusion = model.Conclusion;
                exam.HealthClass = model.HealthClass;
                if (doctorId.HasValue)
                {
                    exam.ExaminingDoctorId = doctorId.Value;
                }
                _dataContext.VcInternalMedicineExams.Update(exam);
            }

            // Chuyển trạng thái hồ sơ sang IN_PROGRESS nếu đang là PENDING
            if (record.Status == "PENDING")
            {
                record.Status = "IN_PROGRESS";
                _dataContext.VcHealthCheckupRecords.Update(record);
            }

            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region 5. Xóa Cache
            _cacheService.Remove(HealthCheckupRecordConstant.BuildCacheKey(model.RecordId.ToString()));
            #endregion

            Log.Information($"Lưu kết quả khám nội khoa cho hồ sơ {model.RecordId} thành công.");
            return Unit.Value;
        }
    }
}
