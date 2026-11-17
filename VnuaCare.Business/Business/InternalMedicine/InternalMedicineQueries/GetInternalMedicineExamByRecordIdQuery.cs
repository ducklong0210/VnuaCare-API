/**
 * Nghiệp vụ Lấy kết quả khám lâm sàng Nội khoa theo ID hồ sơ (GetInternalMedicineExamByRecordIdQuery)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.InternalMedicine.InternalMedicineQueries;

public class GetInternalMedicineExamByRecordIdQuery : IRequest<InternalMedicineExamModel?>
{
    public int RecordId { get; set; }

    public GetInternalMedicineExamByRecordIdQuery(int recordId)
    {
        RecordId = recordId;
    }

    public class Handler : IRequestHandler<GetInternalMedicineExamByRecordIdQuery, InternalMedicineExamModel?>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly ICacheService _cacheService;

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }

        public async Task<InternalMedicineExamModel?> Handle(GetInternalMedicineExamByRecordIdQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = InternalMedicineConstant.BuildCacheKey(request.RecordId.ToString());

            var item = await _cacheService.GetOrCreate<InternalMedicineExamModel?>(cacheKey, async () =>
            {
                var exam = await _dataContext.VcInternalMedicineExams.AsNoTracking()
                    .Include(x => x.ExaminingDoctor)
                    .Where(x => x.RecordId == request.RecordId)
                    .Select(x => new InternalMedicineExamModel
                    {
                        InternalExamId = x.InternalExamId,
                        RecordId = x.RecordId,
                        CirculatoryDesc = x.CirculatoryDesc,
                        RespiratoryDesc = x.RespiratoryDesc,
                        DigestiveDesc = x.DigestiveDesc,
                        UrinaryDesc = x.UrinaryDesc,
                        MusculoskeletalDesc = x.MusculoskeletalDesc,
                        NeurologicalDesc = x.NeurologicalDesc,
                        Conclusion = x.Conclusion,
                        HealthClass = x.HealthClass,
                        ExaminingDoctorId = x.ExaminingDoctorId,
                        ExaminingDoctorName = x.ExaminingDoctor != null ? x.ExaminingDoctor.FullName : null,
                        CreatedAt = x.CreatedAt
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return exam;
            });

            return item;
        }
    }
}
