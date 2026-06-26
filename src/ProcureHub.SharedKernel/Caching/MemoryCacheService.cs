using Microsoft.Extensions.Caching.Memory;

namespace ProcureHub.SharedKernel.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly HashSet<string> _keys = [];
    private readonly object _lock = new();

    public MemoryCacheService(IMemoryCache cache) => _cache = cache;

    public T? Get<T>(string key)
        => _cache.TryGetValue(key, out T? value) ? value : default;

    public void Set<T>(string key, T value, TimeSpan ttl)
    {
        lock (_lock) _keys.Add(key);
        _cache.Set(key, value, ttl);
    }

    public void Remove(string key)
    {
        lock (_lock) _keys.Remove(key);
        _cache.Remove(key);
    }

    public void RemoveByPrefix(string prefix)
    {
        List<string> toRemove;
        lock (_lock)
        {
            toRemove = _keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var key in toRemove) _keys.Remove(key);
        }
        foreach (var key in toRemove) _cache.Remove(key);
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl)
    {
        if (_cache.TryGetValue(key, out T? cached) && cached is not null)
            return cached;

        var value = await factory();
        Set(key, value, ttl);
        return value;
    }
}
