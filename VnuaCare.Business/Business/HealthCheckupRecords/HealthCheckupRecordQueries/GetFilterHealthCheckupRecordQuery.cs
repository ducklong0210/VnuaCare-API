using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;
using VnuaCare.Shared.Utils;

namespace VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordQueries;

public class GetFilterHealthCheckupRecordQuery : IRequest<PaginationList<HealthCheckupRecordModel>>
{
    public HealthCheckupRecordFilterModel Filter { get; set; }
    
    public GetFilterHealthCheckupRecordQuery(HealthCheckupRecordFilterModel filter)
    {
        Filter = filter;
    }

    public class Handler : IRequestHandler<GetFilterHealthCheckupRecordQuery, PaginationList<HealthCheckupRecordModel>>
    {

        private readonly VnuaCareReadDataContext _dataContext;
        private readonly ICacheService _cacheService;
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService, IContextAccessor contextAccessor)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
            _contextAccessor = contextAccessor;
        }

        public async Task<PaginationList<HealthCheckupRecordModel>> Handle(GetFilterHealthCheckupRecordQuery request,
            CancellationToken cancellationToken)
        {
            var filter = request.Filter;
            
            var currentUserRole = _contextAccessor.Role;

            var query = _dataContext.VcHealthCheckupRecords.AsNoTracking();

            if (currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.SUPER_ADMIN.ToString())
            {
                throw new UnauthorizedAccessException("Ban khong co quyen admin");
            }

            if (filter.CampaignId.HasValue && filter.CampaignId.Value > 0)
            {
                query = query.Where(x => x.CampaignId == filter.CampaignId.Value);
            }

            if (!string.IsNullOrEmpty(filter.TextSearch))
            {
                string ts = filter.TextSearch.Trim().ToLower();
                query = query
                    .Include(x => x.Doctor)
                    .Include(x => x.Staff)
                    .Where(x => x.HealthClassification.ToLower().Contains(ts)
                || x.MedicalHistoryFamily.ToLower().Contains(ts)
                || x.HealthClassification.ToLower().Contains(ts)
                || x.Recommendations.ToLower().Contains(ts)
                || x.Staff.FullName.ToLower().Contains(ts)
                || x.Doctor.FullName.ToLower().Contains(ts) );
                
            }
            
            // phana trang
            if(filter.PageNumber <= 0) filter.PageNumber = 1;
            if (filter.PageSize <= 0) filter.PageSize = 10;
            
            int totalCount = await query.CountAsync(cancellationToken);
            int excludedRows = (filter.PageNumber - 1) * filter.PageSize;

            var listData = await query
                .OrderBy(x => x.RecordId)
                .Skip(excludedRows)
                .Take(filter.PageSize)
                .Select(x => new HealthCheckupRecordModel
                {
                    RecordId = x.RecordId,
                    SidCode =  x.SidCode,
                    CampaignId =  x.CampaignId,
                    CampaignName = x.Campaign.CampaignName,
                    StaffId =  x.StaffId,
                    StaffName =   x.Staff.FullName,
                    EmployeeCode = x.Staff.EmployeeCode,
                    DateOfBirth = x.Staff.DateOfBirth,
                    PhoneNumber = x.Staff.PhoneNumber,
                    DepartmentName = x.Staff.Department.DepartmentName,
                    DoctorName = x.Doctor.FullName,
                    ConcludedByDoctorId = x.Doctor.DoctorId,
                    ActualCheckupDate =  x.ActualCheckupDate,
                    MedicalHistoryPersonal =  x.MedicalHistoryPersonal,
                    MedicalHistoryFamily =  x.MedicalHistoryFamily,
                    HealthClassification =   x.HealthClassification,
                    GeneralConclusion =    x.GeneralConclusion,
                    Gender = x.Staff.Gender,
                    Recommentdations =  x.Recommendations,
                    ConcludedAt =   x.ConcludedAt,
                    Status =       x.Status,
                    ReportPdfUrl =    x.ReportPdfUrl,
                }).ToListAsync(cancellationToken);
            
            return new PaginationList<HealthCheckupRecordModel>()
            {
                DataCount =  listData.Count,
                TotalCount =   totalCount,
                PageNumber =  filter.PageNumber,
                PageSize =  filter.PageSize,
                Data = listData
            };
        }

    }
    
    
}