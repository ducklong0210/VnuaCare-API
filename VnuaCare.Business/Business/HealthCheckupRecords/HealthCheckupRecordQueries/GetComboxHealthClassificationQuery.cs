/**
 * Nghiệp vụ lấy danh mục Phân loại sức khỏe theo tiêu chuẩn Bộ Y tế (Combobox / Dropdown)
 */

using MediatR;
using VnuaCare.Business.Business.Services;
using VnuaCare.Shared.Enums;

namespace VnuaCare.Business.Business.HealthCheckupRecords.HealthCheckupRecordQueries;

/// <summary>
/// Query lấy danh mục Phân loại sức khỏe (Loại I đến Loại V) theo tiêu chuẩn Bộ Y tế
/// </summary>
public class GetComboxHealthClassificationQuery : IRequest<List<HealthClassificationSelectModel>>
{
    public class Handler : IRequestHandler<GetComboxHealthClassificationQuery, List<HealthClassificationSelectModel>>
    {
        private readonly ICacheService _cacheService;

        public Handler(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<List<HealthClassificationSelectModel>> Handle(GetComboxHealthClassificationQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"classifications-{VnuaCareCacheConstant.LIST_SELECT}";

            var result = await _cacheService.GetOrCreate<List<HealthClassificationSelectModel>>(cacheKey, () =>
            {
                var classifications = new List<HealthClassificationSelectModel>
                {
                    new HealthClassificationSelectModel
                    {
                        ClassificationCode = "Loại I",
                        ClassificationName = "Loại I (Rất khỏe)",
                        Description = "Đạt tiêu chuẩn sức khỏe loại I, không có biểu hiện bệnh lý"
                    },
                    new HealthClassificationSelectModel
                    {
                        ClassificationCode = "Loại II",
                        ClassificationName = "Loại II (Khỏe)",
                        Description = "Đạt tiêu chuẩn sức khỏe loại II, có thể có vấn đề nhỏ không ảnh hưởng công việc"
                    },
                    new HealthClassificationSelectModel
                    {
                        ClassificationCode = "Loại III",
                        ClassificationName = "Loại III (Trung bình)",
                        Description = "Đạt tiêu chuẩn loại III, có bệnh mạn tính nhẹ hoặc cần điều chỉnh thói quen sinh hoạt"
                    },
                    new HealthClassificationSelectModel
                    {
                        ClassificationCode = "Loại IV",
                        ClassificationName = "Loại IV (Yếu)",
                        Description = "Sức khỏe yếu, có bệnh lý cần điều trị tích cực và hạn chế lao động nặng"
                    },
                    new HealthClassificationSelectModel
                    {
                        ClassificationCode = "Loại V",
                        ClassificationName = "Loại V (Rất yếu)",
                        Description = "Sức khỏe rất yếu, bệnh lý nặng cần nghỉ ngơi và điều trị chuyên sâu"
                    }
                };

                return Task.FromResult(classifications);
            });

            return result ?? new List<HealthClassificationSelectModel>();
        }
    }
}
