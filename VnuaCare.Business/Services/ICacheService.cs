using Microsoft.Extensions.Caching.Distributed;

namespace VnuaCare.Business.Business.Services;

public interface ICacheService
{
    Task<TItem> GetOrCreate<TItem>(string cacheKey, Func<Task<TItem>> createItemFunc,
        DistributedCacheEntryOptions? options = null);
    Task<TItem> GetOrCreate<TItem>(string cacheKey, Func<TItem> createItemFunc,
        DistributedCacheEntryOptions? options = null);
    Task Set<TItem>(string cacheKey, Func<Task<TItem>> createItemFunc, DistributedCacheEntryOptions? options = null);
    Task Set<TItem>(string cacheKey, Func<TItem> createItemFunc, DistributedCacheEntryOptions? options = null);
    Task Set<TItem>(string  cacheKey, TItem createItem, DistributedCacheEntryOptions? options = null);
    void Remove(string cacheKey);
    void RemoveAll();
    void RemoveAllWithPrefix(string prefix);
}