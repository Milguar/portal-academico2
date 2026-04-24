using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using portal_academico.Models;

namespace portal_academico.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Curso> Cursos { get; set; }
    public DbSet<Matricula> Matriculas { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuración de Curso
        builder.Entity<Curso>(entity =>
        {
            // Código único
            entity.HasIndex(e => e.Codigo).IsUnique();

            // Validación de Credits > 0 (a nivel de modelo)
            entity.Property(e => e.Creditos)
                .HasAnnotation("Range", new[] { 1, 20 });

            // Validación: HorarioInicio < HorarioFin
            entity.HasCheckConstraint("CK_Curso_Horario", "HorarioInicio < HorarioFin");
        });

        // Configuración de Matrícula
        builder.Entity<Matricula>(entity =>
        {
            // Índice compuesto único: evita duplicados (mismo usuario en mismo curso)
            entity.HasIndex(e => new { e.CursoId, e.UsuarioId }).IsUnique();

            // Relaciones
            entity.HasOne(m => m.Curso)
                .WithMany()
                .HasForeignKey(m => m.CursoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Usuario)
                .WithMany()
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
