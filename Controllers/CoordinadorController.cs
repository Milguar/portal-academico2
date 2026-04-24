using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using portal_academico.Data;
using portal_academico.Models;
using portal_academico.Services;

namespace portal_academico.Controllers;

[Authorize(Roles = "Coordinador")]
public class CoordinadorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICursosCacheService _cacheService;

    public CoordinadorController(ApplicationDbContext context, ICursosCacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    // GET: Coordinador/Cursos
    public async Task<IActionResult> Cursos()
    {
        return View(await _context.Cursos.ToListAsync());
    }

    // GET: Coordinador/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Coordinador/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Codigo,Nombre,Creditos,CupoMaximo,HorarioInicio,HorarioFin,Activo")] Curso curso)
    {
        var validacion = CursosController.ValidarCurso(curso);
        if (!validacion.IsValid)
        {
            ModelState.AddModelError(string.Empty, validacion.Error!);
            return View(curso);
        }

        if (ModelState.IsValid)
        {
            _context.Add(curso);
            await _context.SaveChangesAsync();
            await _cacheService.InvalidateCacheAsync();
            return RedirectToAction(nameof(Cursos));
        }
        return View(curso);
    }

    // GET: Coordinador/Edit/5
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

    // POST: Coordinador/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Nombre,Creditos,CupoMaximo,HorarioInicio,HorarioFin,Activo")] Curso curso)
    {
        if (id != curso.Id)
        {
            return NotFound();
        }

        var validacion = CursosController.ValidarCurso(curso);
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
                await _cacheService.InvalidateCacheAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CursoExists(curso.Id))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Cursos));
        }
        return View(curso);
    }

    // GET: Coordinador/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var curso = await _context.Cursos.FirstOrDefaultAsync(m => m.Id == id);
        if (curso == null)
        {
            return NotFound();
        }

        return View(curso);
    }

    // POST: Coordinador/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso != null)
        {
            _context.Cursos.Remove(curso);
            await _context.SaveChangesAsync();
            await _cacheService.InvalidateCacheAsync();
        }

        return RedirectToAction(nameof(Cursos));
    }

    // GET: Coordinador/Matriculas/5
    public async Task<IActionResult> Matriculas(int? id)
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

        var matriculas = await _context.Matriculas
            .Include(m => m.Usuario)
            .Where(m => m.CursoId == id)
            .OrderBy(m => m.FechaRegistro)
            .ToListAsync();

        ViewData["Curso"] = curso;
        return View(matriculas);
    }

    // POST: Coordinador/ConfirmarMatricula/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarMatricula(int id)
    {
        var matricula = await _context.Matriculas.FindAsync(id);
        if (matricula == null)
        {
            return NotFound();
        }

        // Verificar cupo
        var curso = await _context.Cursos.FindAsync(matricula.CursoId);
        var matriculados = await _context.Matriculas
            .CountAsync(m => m.CursoId == matricula.CursoId && m.Estado == EstadoMatricula.Confirmada);

        if (matriculados >= curso!.CupoMaximo)
        {
            TempData["Error"] = "No hay cupo disponible.";
            return RedirectToAction(nameof(Matriculas), new { id = matricula.CursoId });
        }

        matricula.Estado = EstadoMatricula.Confirmada;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Matrícula confirmada.";
        return RedirectToAction(nameof(Matriculas), new { id = matricula.CursoId });
    }

    // POST: Coordinador/CancelarMatricula/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarMatricula(int id)
    {
        var matricula = await _context.Matriculas.FindAsync(id);
        if (matricula == null)
        {
            return NotFound();
        }

        var cursoId = matricula.CursoId;
        matricula.Estado = EstadoMatricula.Cancelada;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Matrícula cancelada.";
        return RedirectToAction(nameof(Matriculas), new { id = cursoId });
    }

    private bool CursoExists(int id)
    {
        return _context.Cursos.Any(e => e.Id == id);
    }
}