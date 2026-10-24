/**
 * Nghiệp vụ lấy danh mục Trạng thái hồ sơ khám sức khỏe (Combobox / Dropdown)
 */

using MediatR;
using VnuaCare.Business.Business.Services;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordQueries;

/// <summary>
/// Query lấy danh mục Trạng thái hồ sơ khám sức khỏe phục vụ combobox / dropdown
/// </summary>
public class GetComboxRecordStatusQuery : IRequest<List<RecordStatusSelectModel>>
{
    public class Handler : IRequestHandler<GetComboxRecordStatusQuery, List<RecordStatusSelectModel>>
    {
        private readonly ICacheService _cacheService;

        public Handler(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<List<RecordStatusSelectModel>> Handle(GetComboxRecordStatusQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"record-statuses-{VnuaCareCacheConstant.LIST_SELECT}";

            var result = await _cacheService.GetOrCreate<List<RecordStatusSelectModel>>(cacheKey, () =>
            {
                var statuses = new List<RecordStatusSelectModel>
                {
                    new RecordStatusSelectModel
                    {
                        StatusCode = "PENDING",
                        StatusName = "Chờ khám",
                        Description = "Hồ sơ đã tạo, cán bộ chưa thực hiện khám lâm sàng"
                    },
                    new RecordStatusSelectModel
                    {
                        StatusCode = "IN_PROGRESS",
                        StatusName = "Đang khám / Chờ kết luận",
                        Description = "Đã có kết quả khám chuyên khoa, đang chờ bác sĩ trưởng đoàn ký kết luận"
                    },
                    new RecordStatusSelectModel
                    {
                        StatusCode = "COMPLETED",
                        StatusName = "Đã kết luận / Hoàn thành",
                        Description = "Bác sĩ đã ký duyệt kết luận xếp loại sức khỏe tổng thể"
                    },
                    new RecordStatusSelectModel
                    {
                        StatusCode = "CANCELLED",
                        StatusName = "Đã hủy",
                        Description = "Hồ sơ bị hủy do vắng mặt hoặc hoãn khám"
                    }
                };

                return Task.FromResult(statuses);
            });

            return result ?? new List<RecordStatusSelectModel>();
        }
    }
}
