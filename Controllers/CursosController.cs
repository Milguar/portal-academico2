using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using portal_academico.Data;
using portal_academico.Models;

namespace portal_academico.Controllers;

public class CursosController : Controller
{
    private readonly ApplicationDbContext _context;

    public CursosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Cursos
    public async Task<IActionResult> Index(
        string? searchNombre,
        int? creditosMin,
        int? creditosMax,
        TimeSpan? horarioInicio,
        TimeSpan? horarioFin)
    {
        var query = _context.Cursos.AsQueryable();

        // Filtro por nombre
        if (!string.IsNullOrEmpty(searchNombre))
        {
            query = query.Where(c => c.Nombre.Contains(searchNombre));
        }

        // Filtro por rango de créditos
        if (creditosMin.HasValue)
        {
            query = query.Where(c => c.Creditos >= creditosMin.Value);
        }

        if (creditosMax.HasValue)
        {
            query = query.Where(c => c.Creditos <= creditosMax.Value);
        }

        // Filtro por horario inicio
        if (horarioInicio.HasValue)
        {
            query = query.Where(c => c.HorarioInicio >= horarioInicio.Value);
        }

        // Filtro por horario fin
        if (horarioFin.HasValue)
        {
            query = query.Where(c => c.HorarioFin <= horarioFin.Value);
        }

        ViewData["searchNombre"] = searchNombre;
        ViewData["creditosMin"] = creditosMin;
        ViewData["creditosMax"] = creditosMax;
        ViewData["horarioInicio"] = horarioInicio?.ToString(@"hh\:mm");
        ViewData["horarioFin"] = horarioFin?.ToString(@"hh\:mm");

        return View(await query.ToListAsync());
    }

    // GET: Cursos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var curso = await _context.Cursos
            .FirstOrDefaultAsync(m => m.Id == id);

        if (curso == null)
        {
            return NotFound();
        }

        return View(curso);
    }

    // Validaciones server-side reutilizables
    public static (bool IsValid, string? Error) ValidarCurso(Curso curso)
    {
        // Créditos no negativos
        if (curso.Creditos < 0)
        {
            return (false, "Los créditos no pueden ser negativos.");
        }

        // HorarioInicio < HorarioFin
        if (curso.HorarioInicio >= curso.HorarioFin)
        {
            return (false, "La hora de inicio debe ser anterior a la hora de fin.");
        }

        return (true, null);
    }
}