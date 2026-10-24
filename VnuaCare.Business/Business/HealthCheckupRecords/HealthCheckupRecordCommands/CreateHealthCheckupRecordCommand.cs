/**
 * Nghiệp vụ thêm mới Hồ sơ khám sức khỏe: Kiểm tra phân quyền, kiểm tra tính hợp lệ,
 * ràng buộc khóa ngoại (Đợt khám, Cán bộ, Bác sĩ), kiểm tra trùng lặp SID, lưu vào CSDL và xóa cache.
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

/// <summary>
/// Lệnh thêm mới hồ sơ khám sức khỏe tổng hợp của cán bộ (vc_health_checkup_records)
/// </summary>
public class CreateHealthCheckupRecordCommand : IRequest<Unit>
{
    public CreateHealthCheckupRecordModel Model { get; set; }
    
    public CreateHealthCheckupRecordCommand(CreateHealthCheckupRecordModel model)
    {
        Model = model;
    }

    public class Handler : IRequestHandler<CreateHealthCheckupRecordCommand, Unit>
    {
        private readonly VnuaCareDataContext _dataContext;        // DbContext kết nối CSDL để ghi dữ liệu hồ sơ khám
        private readonly ICacheService _cacheService;            // Dịch vụ quản lý và làm mới bộ nhớ đệm (Cache)
        private readonly IContextAccessor _contextAccessor;      // Dịch vụ trích xuất thông tin người dùng đang đăng nhập (Role, UserId)

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
        /// Xử lý tạo mới hồ sơ khám sức khỏe: Kiểm tra quyền, xác thực thông tin, kiểm tra trùng lặp và lưu trữ
        /// </summary>
        public async Task<Unit> Handle(CreateHealthCheckupRecordCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model;
            Log.Information("Bắt đầu thêm hồ sơ khám sức khỏe mới: " + JsonSerializer.Serialize(model));

            #region 1. Phân quyền người thực hiện (Authorization)
            // Chỉ tài khoản có vai trò HEALTH_ADMIN hoặc SUPER_ADMIN mới có quyền lập hồ sơ khám sức khỏe
            var currentUserRole = _contextAccessor.Role;

            if (currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện chức năng này (Yêu cầu quyền Quản trị viên y tế hoặc Quản trị hệ thống).");
            }
            #endregion

            #region 2. Kiểm tra tính hợp lệ dữ liệu đầu vào (Validation)
            // Kiểm tra ID đợt khám sức khỏe
            if (model.CampaignId <= 0)
            {
                throw new ArgumentException("Vui lòng chọn đợt khám sức khỏe hợp lệ.");
            }

            // Kiểm tra ID cán bộ
            if (model.StaffId <= 0)
            {
                throw new ArgumentException("Vui lòng chọn cán bộ khám sức khỏe hợp lệ.");
            }

            // Kiểm tra mã vạch hồ sơ / Mã tiếp nhận (SID)
            if (string.IsNullOrWhiteSpace(model.SidCode))
            {
                throw new ArgumentException("Mã vạch hồ sơ (SID) không được để trống.");
            }
            #endregion

            #region 3. Kiểm tra tính hợp lệ của các thực thể liên quan (Business Existence Check)
            // Kiểm tra đợt khám sức khỏe có tồn tại trong hệ thống hay không
            var campaignExists = await _dataContext.VcCheckupCampaigns
                .AnyAsync(x => x.CampaignId == model.CampaignId, cancellationToken);
            if (!campaignExists)
            {
                throw new ArgumentException($"Đợt khám sức khỏe với ID '{model.CampaignId}' không tồn tại trên hệ thống.");
            }

            // Kiểm tra cán bộ có tồn tại trong hệ thống hay không
            var staffExists = await _dataContext.VcStaffs
                .AnyAsync(x => x.StaffId == model.StaffId, cancellationToken);
            if (!staffExists)
            {
                throw new ArgumentException($"Cán bộ với ID '{model.StaffId}' không tồn tại trên hệ thống.");
            }

            // Nếu có chỉ định bác sĩ kết luận, kiểm tra bác sĩ có tồn tại trong hệ thống hay không
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

            #region 4. Kiểm tra trùng lặp thông tin định danh (Check Duplicate)
            // Kiểm tra trùng lặp mã vạch SID (mỗi hồ sơ khám có một mã định danh duy nhất)
            var isSidExisted = await _dataContext.VcHealthCheckupRecords
                .AnyAsync(x => x.SidCode == model.SidCode.Trim(), cancellationToken);
            if (isSidExisted)
            {
                throw new ArgumentException($"Mã vạch hồ sơ (SID) '{model.SidCode}' đã tồn tại trên hệ thống. Vui lòng quét hoặc nhập mã khác.");
            }

            // Kiểm tra cán bộ đã có hồ sơ trong đợt khám này chưa (mỗi cán bộ tham gia 1 đợt khám chỉ lập 1 hồ sơ duy nhất)
            var isStaffAlreadyRecorded = await _dataContext.VcHealthCheckupRecords
                .AnyAsync(x => x.CampaignId == model.CampaignId && x.StaffId == model.StaffId, cancellationToken);
            if (isStaffAlreadyRecorded)
            {
                throw new ArgumentException("Cán bộ này đã được lập hồ sơ trong đợt khám sức khỏe được chọn.");
            }
            #endregion

            #region 5. Khởi tạo thực thể và lưu vào CSDL (Persistence)
            var recordEntity = new VcHealthCheckupRecords
            {
                CampaignId = model.CampaignId,
                StaffId = model.StaffId,
                SidCode = model.SidCode.Trim(),
                ActualCheckupDate = model.ActualCheckupDate == default ? DateTime.Now : model.ActualCheckupDate,
                MedicalHistoryPersonal = model.MedicalHistoryPersonal?.Trim(),
                MedicalHistoryFamily = model.MedicalHistoryFamily?.Trim(),
                HealthClassification = model.HealthClassification?.Trim(),
                GeneralConclusion = model.GeneralConclusion?.Trim(),
                Recommendations = model.Recommentdations?.Trim(),
                ConcludedByDoctorId = model.ConcludedByDoctorId > 0 ? model.ConcludedByDoctorId : null,
                ConcludedAt = model.ConcludedAt == default ? DateTime.Now : model.ConcludedAt,
                Status = string.IsNullOrWhiteSpace(model.Status) ? Status.PENDING.ToString() : model.Status.Trim(),
                ReportPdfUrl = model.ReportPdfUrl?.Trim()
            };

            await _dataContext.VcHealthCheckupRecords.AddAsync(recordEntity, cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken);
            #endregion

            #region 6. Xóa bộ nhớ đệm (Cache Invalidation)
            // Xóa cache danh sách dropdown/lọc hồ sơ khám và cache chi tiết nếu có
            _cacheService.Remove(HealthCheckupRecordConstant.BuildCacheKey());
            _cacheService.Remove(HealthCheckupRecordConstant.BuildCacheKey(recordEntity.RecordId.ToString()));
            #endregion

            Log.Information($"Thêm hồ sơ khám sức khỏe thành công - RecordId: {recordEntity.RecordId}, SID: {recordEntity.SidCode}, StaffId: {recordEntity.StaffId}, CampaignId: {recordEntity.CampaignId}");

            return Unit.Value;
        }
    }
}