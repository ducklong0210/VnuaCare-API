using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.Staffs.StaffQueries;

public class GetStaffByIdIndex : IRequest<StaffModel>
{
    public int Id { get; set; }

    /// <summary>
    /// Lất thông tin nhân viên theo id
    /// </summary>
    /// <param name="id"></param>
    public GetStaffByIdIndex(int id)
    {
        Id = id;
    }

    public class Handler : IRequestHandler<GetStaffByIdIndex, StaffModel>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly ICacheService _cacheService;

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }

        public async Task<StaffModel> Handle(GetStaffByIdIndex request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            string cacheKey = StaffConstant.BuildCacheKey(id.ToString());

            var item = await _cacheService.GetOrCreate<StaffModel>(cacheKey, async () =>
            {
                var entity = await _dataContext.VcStaffs.AsNoTracking()
                    .Where(x => x.StaffId == id)
                    .Select(x => new StaffModel
                    {
                        StaffId =  x.StaffId,
                        StaffCode = x.EmployeeCode,
                        FullName = x.FullName,
                        JobTitle = x.JobTitle,
                        PhoneNumber = x.PhoneNumber,
                        Gender =  x.Gender,
                        DateOfBirth =  x.DateOfBirth,
                        Email = x.User.Email,
                        Adrress = x.Address,
                        AvatarUrl = string.IsNullOrEmpty(x.AvatarUrl)  ? "" : x.AvatarUrl,
                        DepartmentName = x.Department.DepartmentName,
                        AcademicTitle =  x.AcademicTitle,
                    }).FirstOrDefaultAsync(cancellationToken);
                return entity;
            });
            return item;
        }
        
    }
    
}