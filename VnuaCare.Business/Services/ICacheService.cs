/**
 * Giao diện dịch vụ bộ nhớ đệm (Cache Service) hỗ trợ cả In-Memory và Redis
 */

using Microsoft.Extensions.Caching.Distributed;

namespace VnuaCare.Business.Business.Services;

/// <summary>
/// Giao diện chuẩn hóa các thao tác tương tác với hệ thống bộ nhớ đệm Cache
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Lấy dữ liệu từ cache hoặc tự động gọi hàm tạo mới bất đồng bộ nếu chưa có
    /// </summary>
    Task<TItem> GetOrCreate<TItem>(string cacheKey, Func<Task<TItem>> createItemFunc,
        DistributedCacheEntryOptions? options = null);

    /// <summary>
    /// Lấy dữ liệu từ cache hoặc tự động gọi hàm tạo mới đồng bộ nếu chưa có
    /// </summary>
    Task<TItem> GetOrCreate<TItem>(string cacheKey, Func<TItem> createItemFunc,
        DistributedCacheEntryOptions? options = null);

    /// <summary>
    /// Ghi dữ liệu vào cache từ hàm bất đồng bộ
    /// </summary>
    Task Set<TItem>(string cacheKey, Func<Task<TItem>> createItemFunc, DistributedCacheEntryOptions? options = null);

    /// <summary>
    /// Ghi dữ liệu vào cache từ hàm đồng bộ
    /// </summary>
    Task Set<TItem>(string cacheKey, Func<TItem> createItemFunc, DistributedCacheEntryOptions? options = null);

    /// <summary>
    /// Ghi trực tiếp một đối tượng dữ liệu vào cache
    /// </summary>
    Task Set<TItem>(string  cacheKey, TItem createItem, DistributedCacheEntryOptions? options = null);

    /// <summary>
    /// Xóa một bản ghi cache theo khóa
    /// </summary>
    void Remove(string cacheKey);

    /// <summary>
    /// Xóa toàn bộ dữ liệu trong cache
    /// </summary>
    void RemoveAll();

    /// <summary>
    /// Xóa tất cả các bản ghi cache có tiền tố khớp với prefix
    /// </summary>
    void RemoveAllWithPrefix(string prefix);
}
