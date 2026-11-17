/**
 * Nghiệp vụ Lấy chi tiết Đợt khám sức khỏe theo ID (GetCampaignByIdIndex)
 * Kèm thống kê tổng số hồ sơ và số hồ sơ đã hoàn thành
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.DataContext;

namespace VnuaCare.Business.Business.Campaigns.CampaignQueries;

public class GetCampaignByIdIndex : IRequest<CampaignDetailModel?>
{
    public int CampaignId { get; set; }

    public GetCampaignByIdIndex(int campaignId)
    {
        CampaignId = campaignId;
    }

    public class Handler : IRequestHandler<GetCampaignByIdIndex, CampaignDetailModel?>
    {
        private readonly VnuaCareReadDataContext _dataContext;

        public Handler(VnuaCareReadDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<CampaignDetailModel?> Handle(GetCampaignByIdIndex request, CancellationToken cancellationToken)
        {
            var campaign = await _dataContext.VcCheckupCampaigns.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CampaignId == request.CampaignId, cancellationToken);

            if (campaign == null) return null;

            int totalRecords = await _dataContext.VcHealthCheckupRecords.AsNoTracking()
                .CountAsync(x => x.CampaignId == request.CampaignId, cancellationToken);

            int completedRecords = await _dataContext.VcHealthCheckupRecords.AsNoTracking()
                .CountAsync(x => x.CampaignId == request.CampaignId && x.Status == "COMPLETED", cancellationToken);

            return new CampaignDetailModel
            {
                CampaignId = campaign.CampaignId,
                CampaignName = campaign.CampaignName,
                CampaignCode = campaign.CampaignCode,
                Year = campaign.Year,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                HospitalPartner = campaign.HospitalPartner,
                Status = campaign.Status,
                TotalRecords = totalRecords,
                CompletedRecords = completedRecords
            };
        }
    }
}
