using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using portal_academico.Models;

namespace portal_academico.Services;

public class CursosCacheService : ICursosCacheService
{
    private readonly IDistributedCache _cache;
    private readonly DistributedCacheEntryOptions _options;
    private const string CacheKey = "cursos_activos";

    public CursosCacheService(IDistributedCache cache)
    {
        _cache = cache;
        _options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
        };
    }

    public async Task<List<Curso>?> GetCursosActivosAsync()
    {
        var json = await _cache.GetStringAsync(CacheKey);
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<List<Curso>>(json);
    }

    public async Task SetCursosActivosAsync(List<Curso> cursos)
    {
        var json = JsonSerializer.Serialize(cursos);
        await _cache.SetStringAsync(CacheKey, json, _options);
    }

    public async Task InvalidateCacheAsync()
    {
        await _cache.RemoveAsync(CacheKey);
    }
}