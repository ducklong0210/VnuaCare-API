/**
 * Tiện ích hỗ trợ ánh xạ tự động giữa các đối tượng dữ liệu (Entity <-> Model/DTO) bằng AutoMapper
 */

using AutoMapper;
using AutoMapper.EquivalencyExpression;

namespace VnuaCare.Shared.Utils;

/// <summary>
/// Lớp tiện ích cấu hình và ánh xạ nhanh giữa các đối tượng nguồn và đích
/// </summary>
public static class AutoMapperUtils
{
    /// <summary>
    /// Khởi tạo cấu hình và đối tượng Mapper cho cặp kiểu dữ liệu nguồn - đích
    /// </summary>
    private static IMapper GetMapper<TSource, TDestination>()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddCollectionMappers();
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
            cfg.CreateMap<TSource, TDestination>(MemberList.None);
        });
        IMapper mapper = new Mapper(config);
        return mapper;
    }
    
    /// <summary>
    /// Ánh xạ dữ liệu tự động từ đối tượng nguồn sang đối tượng đích kiểu TDestination
    /// </summary>
    /// <typeparam name="TSource">Kiểu dữ liệu của đối tượng nguồn</typeparam>
    /// <typeparam name="TDestination">Kiểu dữ liệu của đối tượng đích</typeparam>
    /// <param name="source">Đối tượng nguồn</param>
    /// <returns>Đối tượng đích đã được map dữ liệu</returns>
    public static TDestination AutoMap<TSource, TDestination>(TSource source)
    {
        if (source == null) return default;
        var mapper = GetMapper<TSource, TDestination>();
        TDestination dest = mapper.Map<TDestination>(source);
        return dest;
    }
}
