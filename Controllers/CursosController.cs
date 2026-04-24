using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using portal_academico.Data;
using portal_academico.Models;
using portal_academico.Services;

namespace portal_academico.Controllers;

public class CursosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICursosCacheService _cacheService;

    public CursosController(ApplicationDbContext context, ICursosCacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    // GET: Cursos
    public async Task<IActionResult> Index(
        string? searchNombre,
        int? creditosMin,
        int? creditosMax,
        TimeSpan? horarioInicio,
        TimeSpan? horarioFin)
    {
        // Obtener cursos activos desde caché o base de datos
        var cursosActivos = await _cacheService.GetCursosActivosAsync();
        
        if (cursosActivos == null)
        {
            cursosActivos = await _context.Cursos
                .Where(c => c.Activo)
                .ToListAsync();
            await _cacheService.SetCursosActivosAsync(cursosActivos);
        }

        // Aplicar filtros sobre los cursos obtenidos
        var query = cursosActivos.AsQueryable();

        if (!string.IsNullOrEmpty(searchNombre))
        {
            query = query.Where(c => c.Nombre.Contains(searchNombre));
        }

        if (creditosMin.HasValue)
        {
            query = query.Where(c => c.Creditos >= creditosMin.Value);
        }

        if (creditosMax.HasValue)
        {
            query = query.Where(c => c.Creditos <= creditosMax.Value);
        }

        if (horarioInicio.HasValue)
        {
            query = query.Where(c => c.HorarioInicio >= horarioInicio.Value);
        }

        if (horarioFin.HasValue)
        {
            query = query.Where(c => c.HorarioFin <= horarioFin.Value);
        }

        ViewData["searchNombre"] = searchNombre;
        ViewData["creditosMin"] = creditosMin;
        ViewData["creditosMax"] = creditosMax;
        ViewData["horarioInicio"] = horarioInicio?.ToString(@"hh\:mm");
        ViewData["horarioFin"] = horarioFin?.ToString(@"hh\:mm");

        return View(query.ToList());
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

        // Guardar en sesión el último curso visitado
        HttpContext.Session.SetInt32("UltimoCursoId", curso.Id);
        HttpContext.Session.SetString("UltimoCursoNombre", curso.Nombre);

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

    // GET: Cursos/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Cursos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Codigo,Nombre,Creditos,CupoMaximo,HorarioInicio,HorarioFin,Activo")] Curso curso)
    {
        var validacion = ValidarCurso(curso);
        if (!validacion.IsValid)
        {
            ModelState.AddModelError(string.Empty, validacion.Error!);
            return View(curso);
        }

        if (ModelState.IsValid)
        {
            _context.Add(curso);
            await _context.SaveChangesAsync();
            
            // Invalidar caché
            await _cacheService.InvalidateCacheAsync();
            
            return RedirectToAction(nameof(Index));
        }
        return View(curso);
    }

    // GET: Cursos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var curso = await _context.Cursos.FindAsync(id);
        if (curso == null)
        {
            return NotFound();
        }
        return View(curso);
    }

    // POST: Cursos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Nombre,Creditos,CupoMaximo,HorarioInicio,HorarioFin,Activo")] Curso curso)
    {
        if (id != curso.Id)
        {
            return NotFound();
        }

        var validacion = ValidarCurso(curso);
        if (!validacion.IsValid)
        {
            ModelState.AddModelError(string.Empty, validacion.Error!);
            return View(curso);
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(curso);
                await _context.SaveChangesAsync();
                
                // Invalidar caché
                await _cacheService.InvalidateCacheAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CursoExists(curso.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(curso);
    }

    private bool CursoExists(int id)
    {
        return _context.Cursos.Any(e => e.Id == id);
    }
}