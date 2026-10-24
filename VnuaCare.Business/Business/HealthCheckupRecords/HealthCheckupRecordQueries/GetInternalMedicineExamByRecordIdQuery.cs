/**
 * Nghiệp vụ Lấy kết quả khám lâm sàng Nội khoa theo ID hồ sơ (GetInternalMedicineExamByRecordIdQuery)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordQueries;

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

        public Handler(VnuaCareReadDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<InternalMedicineExamModel?> Handle(GetInternalMedicineExamByRecordIdQuery request, CancellationToken cancellationToken)
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
        }
    }
}
