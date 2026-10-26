/**
 * Nghiệp vụ lấy thông tin chi tiết Đợt khám sức khỏe theo ID (kèm Cache)
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.Campaigns.CampaignQueries;

/// <summary>
/// Query lấy thông tin chi tiết Đợt khám sức khỏe theo ID
/// </summary>
public class GetCampaignByIdIndex : IRequest<CampaignModel?>
{
    public int Id { get; set; }

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

            var result = await _cacheService.GetOrCreate<CampaignModel?>(cacheKey, async () =>
            {
                var campaign = await _dataContext.VcCheckupCampaigns.AsNoTracking()
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
                        Status = x.Status,
                        TotalRecords = _dataContext.VcHealthCheckupRecords.Count(r => r.CampaignId == x.CampaignId)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return campaign;
            });

            return result;
        }
    }
}