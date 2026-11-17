/**
 * Nghiệp vụ lấy thông tin chi tiết Đợt khám theo ID (kèm Cache)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.Campaigns.CampaignQueries;

public class GetCampaignByIdIndex : IRequest<CampaignModel?>
{
    public int Id { get; init; }

    public GetCampaignByIdIndex(int id)
    {
        Id = id;
    }

    public class Handler : IRequestHandler<GetCampaignByIdIndex, CampaignModel?>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly ICacheService _cacheService;

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }

        public async Task<CampaignModel?> Handle(GetCampaignByIdIndex request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            string cacheKey = CampaignConstant.BuildCacheKey(id.ToString());

            var item = await _cacheService.GetOrCreate<CampaignModel?>(cacheKey, async () =>
            {
                var entity = await _dataContext.VcCheckupCampaigns.AsNoTracking()
                    .Where(x => x.CampaignId == id)
                    .Select(x => new CampaignModel
                    {
                        CampaignId = x.CampaignId,
                        CampaignName = x.CampaignName,
                        CampaignCode = x.CampaignCode,
                        Year = x.Year,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        HospitalPartner = x.HospitalPartner,
                        Status = x.Status
                    }).FirstOrDefaultAsync(cancellationToken);
                return entity;
            });
            return item;
        }
    }
}
