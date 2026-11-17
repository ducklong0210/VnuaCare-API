/**
 * Nghiệp vụ Lấy chỉ số sinh tồn và thể lực theo ID hồ sơ (GetVitalSignsByRecordIdQuery)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;

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

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }

        public async Task<VitalSignsModel?> Handle(GetVitalSignsByRecordIdQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = VitalSignsConstant.BuildCacheKey(request.RecordId.ToString());

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
                        MeasuredByName = x.MeasuredByDoctor != null ? x.MeasuredByDoctor.FullName : null,
                        CreatedAt = x.CreatedAt
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return vital;
            });

            return item;
        }
    }
}
