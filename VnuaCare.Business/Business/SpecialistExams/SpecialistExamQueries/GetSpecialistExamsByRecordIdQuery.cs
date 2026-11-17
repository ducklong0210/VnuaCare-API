/**
 * Nghiệp vụ Lấy danh sách kết quả khám các chuyên khoa lẻ theo ID hồ sơ (GetSpecialistExamsByRecordIdQuery)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.SpecialistExams.SpecialistExamQueries;

public class GetSpecialistExamsByRecordIdQuery : IRequest<List<SpecialistExamModel>>
{
    public int RecordId { get; set; }
    public string? SpecialtyType { get; set; }

    public GetSpecialistExamsByRecordIdQuery(int recordId, string? specialtyType = null)
    {
        RecordId = recordId;
        SpecialtyType = specialtyType;
    }

    public class Handler : IRequestHandler<GetSpecialistExamsByRecordIdQuery, List<SpecialistExamModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly ICacheService _cacheService;

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }

        public async Task<List<SpecialistExamModel>> Handle(GetSpecialistExamsByRecordIdQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = string.IsNullOrWhiteSpace(request.SpecialtyType)
                ? SpecialistExamConstant.BuildCacheKey(request.RecordId.ToString())
                : SpecialistExamConstant.BuildCacheKey($"{request.RecordId}_{request.SpecialtyType.Trim().ToUpperInvariant()}");

            var item = await _cacheService.GetOrCreate<List<SpecialistExamModel>>(cacheKey, async () =>
            {
                var query = _dataContext.VcSpecialistExams.AsNoTracking()
                    .Include(x => x.ExaminingDoctor)
                    .Where(x => x.RecordId == request.RecordId);

                if (!string.IsNullOrWhiteSpace(request.SpecialtyType))
                {
                    var norm = request.SpecialtyType.Trim().ToUpperInvariant();
                    query = query.Where(x => x.SpecialtyType == norm);
                }

                var list = await query
                    .OrderBy(x => x.CreatedAt)
                    .Select(x => new SpecialistExamModel
                    {
                        SpecialistExamId = x.SpecialistExamId,
                        RecordId = x.RecordId,
                        SpecialtyType = x.SpecialtyType,
                        VisionLeftNoGlass = x.VisionLeftNoGlass,
                        VisionRightNoGlass = x.VisionRightNoGlass,
                        VisionLeftGlass = x.VisionLeftGlass,
                        VisionRightGlass = x.VisionRightGlass,
                        UpperJaw = x.UpperJaw,
                        LowerJaw = x.LowerJaw,
                        TeethFormula = x.TeethFormula,
                        EarLeftDesc = x.EarLeftDesc,
                        EarRightDesc = x.EarRightDesc,
                        NoseDesc = x.NoseDesc,
                        ThroatDesc = x.ThroatDesc,
                        SkinDesc = x.SkinDesc,
                        SurgicalDesc = x.SurgicalDesc,
                        ObstetricDesc = x.ObstetricDesc,
                        Conclusion = x.Conclusion,
                        HealthClass = x.HealthClass,
                        ExaminingDoctorId = x.ExaminingDoctorId,
                        ExaminingDoctorName = x.ExaminingDoctor != null ? x.ExaminingDoctor.FullName : null,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync(cancellationToken);

                return list;
            });

            return item ?? new List<SpecialistExamModel>();
        }
    }
}
