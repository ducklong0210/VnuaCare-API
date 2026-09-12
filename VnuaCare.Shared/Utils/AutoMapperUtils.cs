using AutoMapper;
using AutoMapper.EquivalencyExpression;

namespace VnuaCare.Shared.Utils;

public static class AutoMapperUtils
{
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
    
    // #region Single
    public static TDestination AutoMap<TSource, TDestination>(TSource source)
    {
        if (source == null) return default;
        var mapper = GetMapper<TSource, TDestination>();
        TDestination dest = mapper.Map<TDestination>(source);
        return dest;
    }
}