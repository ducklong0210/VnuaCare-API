/**
 * Nghiệp vụ Lấy chỉ số sinh tồn và thể lực theo ID hồ sơ (GetVitalSignsByRecordIdQuery)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.VitalSigns.VitalSignsQueries;

public class GetVitalSignsByRecordIdQuery : IRequest<VitalSignsModel?>
{
    public int RecordId { get; set; }

    public GetVitalSignsByRecordIdQuery(int recordId)
    {
        RecordId = recordId;
    }

    public class Handler : IRequestHandler<GetVitalSignsByRecordIdQuery, VitalSignsModel?>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly ICacheService _cacheService;
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        public async Task<VitalSignsModel?> Handle(GetVitalSignsByRecordIdQuery request, CancellationToken cancellationToken)
        {
            if (_contextAccessor.Role == Role.STAFF.ToString())
            {
                var currentUserId = _contextAccessor.UserId;
                var isOwner = currentUserId.HasValue && await _dataContext.VcHealthCheckupRecords.AsNoTracking()
                    .AnyAsync(r => r.RecordId == request.RecordId && r.Staff != null && r.Staff.UserId == currentUserId.Value, cancellationToken);
                if (!isOwner)
                {
                    throw new UnauthorizedAccessException("Bạn chỉ có quyền xem chỉ số sinh tồn trong hồ sơ của chính mình.");
                }
            }

            // Xây dựng khóa bộ nhớ đệm (Cache Key) theo ID hồ sơ khám
            string cacheKey = VitalSignsConstant.BuildCacheKey(request.RecordId.ToString());

            // Kiểm tra dữ liệu trong Cache trước (Cache-Aside): nếu đã có thì trả về ngay;
            // nếu chưa có thì truy vấn bảng vc_vital_signs bằng AsNoTracking (tối ưu tốc độ đọc),
            // ánh xạ sang VitalSignsModel (lấy tên bác sĩ đo từ bảng vc_users) rồi tự động lưu vào Cache
            var item = await _cacheService.GetOrCreate<VitalSignsModel?>(cacheKey, async () =>
            {
                var vital = await _dataContext.VcVitalSigns.AsNoTracking()
                    .Include(x => x.MeasuredByDoctor)
                    .Where(x => x.RecordId == request.RecordId)
                    .Select(x => new VitalSignsModel
                    {
                        VitalSignId = x.VitalSignId,
                        RecordId = x.RecordId,
                        HeightCm = x.HeightCm,
                        WeightKg = x.WeightKg,
                        Bmi = x.Bmi,
                        WaistCm = x.WaistCm,
                        SystolicBp = x.SystolicBp,
                        DiastolicBp = x.DiastolicBp,
                        HeartRateBpm = x.HeartRateBpm,
                        RespiratoryRate = x.RespiratoryRate,
                        Spo2Percent = x.Spo2Percent,
                        MeasuredBy = x.MeasuredBy,
                        MeasuredByName = x.MeasuredByDoctor != null && x.MeasuredByDoctor.User != null ? x.MeasuredByDoctor.User.FullName : null,
                        CreatedAt = x.CreatedAt
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return vital;
            });

            return item;
        }
    }
}
