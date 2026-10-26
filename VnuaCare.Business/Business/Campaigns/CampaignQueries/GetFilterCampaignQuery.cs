/**
 * Nghiệp vụ lọc, tìm kiếm và phân trang danh sách Đợt khám sức khỏe
 */

using MediatR;
using Microsoft.EntityFrameworkCore;
using VnuaCare.Data.Systems.DataContext;
using VnuaCare.Shared.ContextAccessor;
using VnuaCare.Shared.Enums;
using VnuaCare.Shared.Utils;

namespace VnuaCare.Business.Business.Campaigns.CampaignQueries;

public class GetFilterCampaignQuery : IRequest<PaginationList<CampaignModel>>
{
    public CampaignFilterModel Filter { get; set; }

    public GetFilterCampaignQuery(CampaignFilterModel filter)
    {
        Filter = filter;
    }

    public class Handler : IRequestHandler<GetFilterCampaignQuery, PaginationList<CampaignModel>>
    {
        private readonly VnuaCareReadDataContext _dataContext;
        private readonly IContextAccessor _contextAccessor;

        public Handler(VnuaCareReadDataContext dataContext, IContextAccessor contextAccessor)
        {
            _dataContext = dataContext;
            _contextAccessor = contextAccessor;
        }

        public async Task<PaginationList<CampaignModel>> Handle(GetFilterCampaignQuery request, CancellationToken cancellationToken)
        {
            var filter = request.Filter;
            var currentUserRole = _contextAccessor.Role;

            if (currentUserRole != Role.SUPER_ADMIN.ToString() &&
                currentUserRole != Role.HEALTH_ADMIN.ToString() &&
                currentUserRole != Role.DOCTOR.ToString() &&
                currentUserRole != Role.STAFF.ToString())
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem danh sách đợt khám.");
            }

            var query = _dataContext.VcCheckupCampaigns.AsNoTracking();

            if (filter.Year.HasValue && filter.Year.Value > 0)
            {
                query = query.Where(x => x.Year == filter.Year.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                var status = filter.Status.Trim();
                query = query.Where(x => x.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(filter.HospitalPartner))
            {
                var partner = filter.HospitalPartner.Trim().ToLower();
                query = query.Where(x => x.HospitalPartner.ToLower().Contains(partner));
            }

            if (!string.IsNullOrWhiteSpace(filter.TextSearch))
            {
                var ts = filter.TextSearch.Trim().ToLower();
                query = query.Where(x => x.CampaignName.ToLower().Contains(ts) ||
                                         x.CampaignCode.ToLower().Contains(ts) ||
                                         x.HospitalPartner.ToLower().Contains(ts));
            }

            int totalCount = await query.CountAsync(cancellationToken);

            if (filter.PageSize <= 0) filter.PageSize = 10;
            if (filter.PageNumber <= 0) filter.PageNumber = 1;

            int excludedRows = (filter.PageNumber - 1) * filter.PageSize;

            var orderedQuery = (filter.Ascending?.ToLower() == "asc")
                ? query.OrderBy(x => x.Year).ThenBy(x => x.CampaignId)
                : query.OrderByDescending(x => x.Year).ThenByDescending(x => x.CampaignId);

            var listData = await orderedQuery
                .Skip(excludedRows)
                .Take(filter.PageSize)
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
                .ToListAsync(cancellationToken);

            return new PaginationList<CampaignModel>
            {
                DataCount = listData.Count,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Data = listData
            };
        }
    }
}