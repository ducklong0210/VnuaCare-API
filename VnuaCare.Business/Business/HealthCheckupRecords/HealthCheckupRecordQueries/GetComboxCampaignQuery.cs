/**
 * Nghiệp vụ lấy danh sách Đợt khám sức khỏe dạng danh mục chọn (Combobox / Dropdown)
 * Hỗ trợ lưu bộ nhớ đệm Cache.
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Business.Business.Services;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordQueries;

/// <summary>
/// Query lấy danh sách Đợt khám sức khỏe phục vụ hiển thị combobox / dropdown trên giao diện
/// </summary>
public class GetComboxCampaignQuery : IRequest<List<CampaignSelectModel>>
{
    public int? Year { get; set; }
    public string? Status { get; set; }

    public GetComboxCampaignQuery(int? year = null, string? status = null)
    {
        Year = year;
        Status = status;
    }

    public class Handler : IRequestHandler<GetComboxCampaignQuery, List<CampaignSelectModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly ICacheService _cacheService;

        public Handler(VnuaCareReadDataContext dataContext, ICacheService cacheService)
        {
            _dataContext = dataContext;
            _cacheService = cacheService;
        }

        public async Task<List<CampaignSelectModel>> Handle(GetComboxCampaignQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"campaigns-{VnuaCareCacheConstant.LIST_SELECT}-year-{(request.Year.HasValue ? request.Year.Value.ToString() : "all")}-status-{(string.IsNullOrWhiteSpace(request.Status) ? "all" : request.Status.Trim())}";

            var result = await _cacheService.GetOrCreate<List<CampaignSelectModel>>(cacheKey, async () =>
            {
                var query = _dataContext.VcCheckupCampaigns.AsNoTracking();

                if (request.Year.HasValue && request.Year.Value > 0)
                {
                    query = query.Where(x => x.Year == request.Year.Value);
                }

                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    var statusFilter = request.Status.Trim();
                    query = query.Where(x => x.Status == statusFilter);
                }

                var list = await query
                    .OrderByDescending(x => x.Year)
                    .ThenByDescending(x => x.CampaignId)
                    .Select(x => new CampaignSelectModel
                    {
                        CampaignId = x.CampaignId,
                        CampaignName = x.CampaignName,
                        CampaignCode = x.CampaignCode,
                        Year = x.Year,
                        Status = x.Status
                    })
                    .ToListAsync(cancellationToken);

                return list;
            });

            return result ?? new List<CampaignSelectModel>();
        }
    }
}
