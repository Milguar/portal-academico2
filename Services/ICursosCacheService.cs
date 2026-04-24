using portal_academico.Models;

namespace portal_academico.Services;

public interface ICursosCacheService
{
    Task<List<Curso>?> GetCursosActivosAsync();
    Task SetCursosActivosAsync(List<Curso> cursos);
    Task InvalidateCacheAsync();
}