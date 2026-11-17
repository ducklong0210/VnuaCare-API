/**
 * Nghiệp vụ Lấy danh sách kết quả khám các chuyên khoa lẻ theo ID hồ sơ (GetSpecialistExamsByRecordIdQuery)
 * Sử dụng DbContext chỉ đọc VnuaCareReadDataContext kết hợp AsNoTracking() tối ưu hiệu năng
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
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

        public Handler(VnuaCareReadDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<SpecialistExamModel>> Handle(GetSpecialistExamsByRecordIdQuery request, CancellationToken cancellationToken)
        {
            var query = _dataContext.VcSpecialistExams.AsNoTracking()
                .Include(x => x.ExaminingDoctor)
                .Where(x => x.RecordId == request.RecordId);

            if (!string.IsNullOrWhiteSpace(request.SpecialtyType))
            {
                var norm = request.SpecialtyType.Trim().ToUpperInvariant();
                query = query.Where(x => x.SpecialtyType == norm);
            }

            var exams = await query
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return exams.Select(x => new SpecialistExamModel
            {
                SpecialistExamId = x.SpecialistExamId,
                RecordId = x.RecordId,
                SpecialtyType = x.SpecialtyType,
                SpecialtyDisplayName = GetSpecialtyDisplayName(x.SpecialtyType),
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
            }).ToList();
        }

        private static string GetSpecialtyDisplayName(string specialtyType) => specialtyType?.ToUpperInvariant() switch
        {
            "EYE" => "Chuyên khoa Mắt",
            "ENT" => "Chuyên khoa Tai - Mũi - Họng",
            "DENTAL" => "Chuyên khoa Răng - Hàm - Mặt",
            "DERMATOLOGY" => "Chuyên khoa Da liễu",
            "SURGERY" => "Ngoại khoa",
            "OBSTETRICS" => "Sản - Phụ khoa",
            _ => specialtyType ?? "Chuyên khoa lẻ"
        };
    }
}
