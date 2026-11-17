/**
 * Nghiệp vụ lấy thông tin chi tiết Hồ sơ khám sức khỏe theo ID (kèm Cache)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordQueries;

public class GetHealthCheckupRecordByIdIndex : IRequest<HealthCheckupRecordModel?>
{
    public int Id { get; init; }

    public GetHealthCheckupRecordByIdIndex(int id)
    {
        Id = id;
    }

    public class Handler : IRequestHandler<GetHealthCheckupRecordByIdIndex, HealthCheckupRecordModel?>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly ICacheService _cacheService;

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }

        public async Task<HealthCheckupRecordModel?> Handle(GetHealthCheckupRecordByIdIndex request,
            CancellationToken cancellationToken)
        {
            var id = request.Id;
            string cacheKey = HealthCheckupRecordConstant.BuildCacheKey(id.ToString());
            
            var item = await _cacheService.GetOrCreate<HealthCheckupRecordModel?>(cacheKey, async () =>
            {
                var entity = await _dataContext.VcHealthCheckupRecords.AsNoTracking()
                    .Where(x => x.RecordId == id)
                    .Select(x => new HealthCheckupRecordModel
                    {
                        RecordId = x.RecordId,
                        SidCode = x.SidCode,
                        CampaignId = x.CampaignId,
                        CampaignName = x.Campaign != null ? x.Campaign.CampaignName : null,
                        StaffId = x.StaffId,
                        StaffName = x.Staff != null ? x.Staff.FullName : null,
                        EmployeeCode = x.Staff != null ? x.Staff.EmployeeCode : null,
                        DepartmentName = x.Staff != null && x.Staff.Department != null ? x.Staff.Department.DepartmentName : null,
                        DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                        ConcludedByDoctorId = x.ConcludedByDoctorId ?? 0,
                        Gender = x.Staff != null ? x.Staff.Gender : null,
                        DateOfBirth = x.Staff != null ? x.Staff.DateOfBirth : default,
                        PhoneNumber = x.Staff != null ? x.Staff.PhoneNumber ?? string.Empty : string.Empty,
                        ActualCheckupDate = x.ActualCheckupDate,
                        MedicalHistoryPersonal = x.MedicalHistoryPersonal,
                        MedicalHistoryFamily = x.MedicalHistoryFamily,
                        HealthClassification = x.HealthClassification,
                        GeneralConclusion = x.GeneralConclusion,
                        Recommendations = x.Recommendations,
                        ConcludedAt = x.ConcludedAt,
                        ReportPdfUrl = x.ReportPdfUrl,
                        Status = x.Status,
                    }).FirstOrDefaultAsync(cancellationToken);

                return entity;
            });

            return item;
        }
    }
}
