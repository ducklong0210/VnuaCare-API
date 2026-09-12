using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;

namespace VnuaCare.Business.Business.Doctors.DoctorQueries;

public class GetDoctorByIdIndex : IRequest<DoctorModel>
{
    
    public int Id { get; init; }
    
    public GetDoctorByIdIndex(int id)
    {
        Id = id;
    }

    public class Handler : IRequestHandler<GetDoctorByIdIndex, DoctorModel>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly IContextAccessor _contextAccessor;
        private readonly ICacheService _cacheService;

        public Handler(VnuaCareReadDataContext dataContext, IContextAccessor contextAccessor, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _contextAccessor = contextAccessor;
            _cacheService = cacheService;
        }

        public async Task<DoctorModel> Handle(GetDoctorByIdIndex request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            string cacheKey = DoctorConstant.BuildCacheKey(id.ToString());

            var item = await _cacheService.GetOrCreate<DoctorModel>(cacheKey, async () =>
                {
                    var entity = await _dataContext.VcDoctors.AsNoTracking()
                        .Include(x => x.Specialty)
                        .Where(x => x.DoctorId == id)
                        .Select(x => new DoctorModel
                        {
                            UserId = x.UserId,
                            DoctorId = x.DoctorId,
                            Email = x.User.Email,
                            DoctorCode = x.DoctorCode,
                            FullName = x.FullName,
                            SpecialtyId = x.SpecialtyId,
                            SpesialtyName =  x.Specialty != null ? x.Specialty.SpecialtyName : "",
                            LicenseNumber = x.LicenseNumber,
                            HospitalName = x.HospitalName
                        }).FirstOrDefaultAsync(cancellationToken);
                    return  entity;
                });
            return item;
        }
        
    }
}