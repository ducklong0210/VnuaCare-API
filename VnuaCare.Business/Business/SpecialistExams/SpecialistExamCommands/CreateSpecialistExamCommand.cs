/**
 * Nghiệp vụ Thêm mới kết quả khám chuyên khoa lẻ (CreateSpecialistExamCommand)
 * Chuẩn CRUD: Dành cho Bác sĩ các chuyên khoa Mắt, Tai Mũi Họng, Răng Hàm Mặt, Da liễu, Ngoại, Sản
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

namespace VnuaCare.Business.Business.SpecialistExams.SpecialistExamCommands;

public class CreateSpecialistExamCommand : IRequest<Unit>
{
    public CreateSpecialistExamModel Model { get; }

    public CreateSpecialistExamCommand(CreateSpecialistExamModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<CreateSpecialistExamCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // Kết nối CSDL để lưu kết quả khám chuyên khoa lẻ
        private readonly ICacheService _cacheService;            // Dịch vụ xóa Cache sau khi thêm mới
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất Role của người thực hiện

        public Handler(VnuaCareDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        // Xử lý kiểm tra dữ liệu, chuyên khoa và lưu kết quả khám chuyên khoa vào CSDL
        public async Task<Unit> Handle(CreateSpecialistExamCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu thêm mới kết quả khám chuyên khoa lâm sàng: " + JsonSerializer.Serialize(model));

            #region Phân quyền
            var currentUserRole = _contextAccessor.Role;
            if (currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Chỉ Bác sĩ chuyên khoa hoặc Quản trị viên y tế mới có quyền thêm kết quả khám chuyên khoa.");
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

            #region Kiểm tra chuyên khoa và bác sĩ khám
            string specialtyType = model.SpecialtyType.Trim().ToUpperInvariant();

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
            var existingExam = await _dataContext.VcSpecialistExams
                .FirstOrDefaultAsync(x => x.RecordId == model.RecordId && x.SpecialtyType == specialtyType, cancellationToken);

            if (existingExam != null)
            {
                existingExam.VisionLeftNoGlass = model.VisionLeftNoGlass;
                existingExam.VisionRightNoGlass = model.VisionRightNoGlass;
                existingExam.VisionLeftGlass = model.VisionLeftGlass;
                existingExam.VisionRightGlass = model.VisionRightGlass;
                existingExam.UpperJaw = model.UpperJaw;
                existingExam.LowerJaw = model.LowerJaw;
                existingExam.TeethFormula = model.TeethFormula;
                existingExam.EarLeftDesc = model.EarLeftDesc;
                existingExam.EarRightDesc = model.EarRightDesc;
                existingExam.NoseDesc = model.NoseDesc;
                existingExam.ThroatDesc = model.ThroatDesc;
                existingExam.SkinDesc = model.SkinDesc;
                existingExam.SurgicalDesc = model.SurgicalDesc;
                existingExam.ObstetricDesc = model.ObstetricDesc;
                existingExam.Conclusion = model.Conclusion;
                existingExam.HealthClass = model.HealthClass;
                if (finalDoctorId.HasValue)
                {
                    existingExam.ExaminingDoctorId = finalDoctorId.Value;
                }

                _dataContext.VcSpecialistExams.Update(existingExam);
            }
            else
            {
                var exam = new VcSpecialistExams
                {
                    RecordId = model.RecordId,
                    SpecialtyType = specialtyType,
                    VisionLeftNoGlass = model.VisionLeftNoGlass,
                    VisionRightNoGlass = model.VisionRightNoGlass,
                    VisionLeftGlass = model.VisionLeftGlass,
                    VisionRightGlass = model.VisionRightGlass,
                    UpperJaw = model.UpperJaw,
                    LowerJaw = model.LowerJaw,
                    TeethFormula = model.TeethFormula,
                    EarLeftDesc = model.EarLeftDesc,
                    EarRightDesc = model.EarRightDesc,
                    NoseDesc = model.NoseDesc,
                    ThroatDesc = model.ThroatDesc,
                    SkinDesc = model.SkinDesc,
                    SurgicalDesc = model.SurgicalDesc,
                    ObstetricDesc = model.ObstetricDesc,
                    Conclusion = model.Conclusion,
                    HealthClass = model.HealthClass,
                    ExaminingDoctorId = finalDoctorId,
                    CreatedAt = DateTime.Now
                };

                await _dataContext.VcSpecialistExams.AddAsync(exam, cancellationToken);
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
            _cacheService.Remove(SpecialistExamConstant.BuildCacheKey(model.RecordId.ToString()));
            #endregion

            Log.Information($"Thêm mới kết quả khám chuyên khoa {specialtyType} cho hồ sơ {model.RecordId} thành công.");
            return Unit.Value;
        }
    }
}
