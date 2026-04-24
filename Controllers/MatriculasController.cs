using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using portal_academico.Data;
using portal_academico.Models;

namespace portal_academico.Controllers;

[Authorize]
public class MatriculasController : Controller
{
    private readonly ApplicationDbContext _context;

    public MatriculasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: Matriculas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CursoId")] Matricula matricula)
    {
        var userId = User.GetUserId();
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Verificar si el curso existe y está activo
        var curso = await _context.Cursos.FindAsync(matricula.CursoId);
        if (curso == null || !curso.Activo)
        {
            TempData["Error"] = "El curso no está disponible.";
            return RedirectToAction("Index", "Cursos");
        }

        // Verificar si ya está matriculado
        var existente = await _context.Matriculas
            .FirstOrDefaultAsync(m => m.CursoId == matricula.CursoId && m.UsuarioId == userId);

        if (existente != null)
        {
            TempData["Error"] = "Ya estás matriculado en este curso.";
            return RedirectToAction("Index", "Cursos");
        }

        // Verificar disponibilidad de cupo
        var matriculados = await _context.Matriculas
            .CountAsync(m => m.CursoId == matricula.CursoId && m.Estado != EstadoMatricula.Cancelada);

        if (matriculados >= curso.CupoMaximo)
        {
            TempData["Error"] = "El curso ha alcanzado su cupo máximo.";
            return RedirectToAction("Index", "Cursos");
        }

        // Crear matrícula
        matricula.UsuarioId = userId;
        matricula.FechaRegistro = DateTime.Now;
        matricula.Estado = EstadoMatricula.Pendiente;

        _context.Matriculas.Add(matricula);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Te has matriculado correctamente.";
        return RedirectToAction("Index", "Cursos");
    }

    // GET: Matriculas/MisMatriculas
    public async Task<IActionResult> MisMatriculas()
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var matriculas = await _context.Matriculas
            .Include(m => m.Curso)
            .Where(m => m.UsuarioId == userId)
            .ToListAsync();

        return View(matriculas);
    }

    // POST: Matriculas/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.GetUserId();
        var matricula = await _context.Matriculas
            .FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == userId);

        if (matricula == null)
        {
            return NotFound();
        }

        matricula.Estado = EstadoMatricula.Cancelada;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Matrícula cancelada.";
        return RedirectToAction("MisMatriculas");
    }
}