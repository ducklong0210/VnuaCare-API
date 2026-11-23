/**
 * Nghiệp vụ Lấy kết quả khám lâm sàng Nội khoa theo ID hồ sơ (GetInternalMedicineExamByRecordIdQuery)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;

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
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService, Func<IContextAccessor> contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor();
        }

        public async Task<InternalMedicineExamModel?> Handle(GetInternalMedicineExamByRecordIdQuery request, CancellationToken cancellationToken)
        {
            if (_contextAccessor.Role == Role.STAFF.ToString())
            {
                var currentUserId = _contextAccessor.UserId;
                var isOwner = currentUserId.HasValue && await _dataContext.VcHealthCheckupRecords.AsNoTracking()
                    .AnyAsync(r => r.RecordId == request.RecordId && r.Staff != null && r.Staff.UserId == currentUserId.Value, cancellationToken);
                if (!isOwner)
                {
                    throw new UnauthorizedAccessException("Bạn chỉ có quyền xem kết quả khám nội khoa trong hồ sơ của chính mình.");
                }
            }

            // Xây dựng khóa bộ nhớ đệm (Cache Key) theo ID hồ sơ khám
            string cacheKey = InternalMedicineConstant.BuildCacheKey(request.RecordId.ToString());

            // Đọc kết quả khám nội khoa từ Cache trước; nếu chưa có thì truy vấn CSDL bằng AsNoTracking
            // (không theo dõi thay đổi để tăng tốc), ánh xạ sang InternalMedicineExamModel (lấy tên bác sĩ từ vc_users) rồi lưu lại vào Cache
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
                        ExaminingDoctorName = x.ExaminingDoctor != null && x.ExaminingDoctor.User != null ? x.ExaminingDoctor.User.FullName : null,
                        CreatedAt = x.CreatedAt
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return exam;
            });

            return item;
        }
    }
}
