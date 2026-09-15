using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordQueries;

public class GetHealthCheckupRecordByIdIndex : IRequest<HealthCheckupRecordModel>
{
    public int Id { get; set; }

    public GetHealthCheckupRecordByIdIndex(int id)
    {
        Id = id;
    }

    public class Handler : IRequestHandler<GetHealthCheckupRecordByIdIndex, HealthCheckupRecordModel>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly ICacheService _cacheService;

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }
        
        // lay thong tin ksk

        public async Task<HealthCheckupRecordModel> Handle(GetHealthCheckupRecordByIdIndex request,
            CancellationToken cancellationToken)
        {
            var id = request.Id;
            string cacheKey = HealthCheckupRecordConstant.BuildCacheKey(id.ToString());
            
            // Đọc từ Cache nếu có, nếu chưa có thì truy vấn từ CSDL và lưu vào Cache
            var item = await _cacheService.GetOrCreate<HealthCheckupRecordModel>(cacheKey, async () =>
            {
                var entity = await _dataContext.VcHealthCheckupRecords.AsNoTracking()
                    .Include(x => x.Staff)
                    .Include(x => x.Doctor)
                    .Include(x => x.Campaign)
                    .Where(x => x.RecordId == request.Id)
                    .Select(x => new HealthCheckupRecordModel
                    {
                        RecordId = x.RecordId,
                        //join vao bang lay thong tin ra
                        StaffId =  x.StaffId,
                        StaffName = x.Staff.FullName,
                        EmployeeCode =  x.Staff.EmployeeCode,
                        DoctorName = x.Doctor.FullName,
                        CampaignId = x.CampaignId,
                        CampaignName = x.Campaign.CampaignName,
                        DepartmentName = x.Staff.Department.DepartmentName,
                        Gender =  x.Staff.Gender,
                        DateOfBirth =  x.Staff.DateOfBirth,
                        PhoneNumber =  x.Staff.PhoneNumber,
                        SidCode =  x.SidCode,
                        ActualCheckupDate = x.ActualCheckupDate,
                        MedicalHistoryPersonal =  x.MedicalHistoryPersonal,
                        MedicalHistoryFamily =  x.MedicalHistoryFamily,
                        HealthClassification =   x.HealthClassification,
                        GeneralConclusion =    x.GeneralConclusion,
                        Recommentdations = x.Recommendations,
                        ConcludedAt = x.ConcludedAt,
                        ReportPdfUrl =  x.ReportPdfUrl,
                        Status =   x.Status,
                    }).FirstOrDefaultAsync(cancellationToken);
                return entity;
            });
            return item;
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
    }
    
    
    
    
    
    
    
    
}