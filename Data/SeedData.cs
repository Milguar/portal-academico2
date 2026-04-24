using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using portal_academico.Models;

namespace portal_academico.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        using var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

        // Asegurar que la base de datos existe
        context.Database.EnsureCreated();

        // 1. Crear rol Coordinador si no existe
        if (!await roleManager.RoleExistsAsync("Coordinador"))
        {
            await roleManager.CreateAsync(new IdentityRole("Coordinador"));
        }

        // 2. Crear usuario coordinador si no existe
        var coordinatorEmail = "coordinador@demo.com";
        var coordinatorUser = await userManager.FindByEmailAsync(coordinatorEmail);

        if (coordinatorUser == null)
        {
            coordinatorUser = new IdentityUser
            {
                UserName = coordinatorEmail,
                Email = coordinatorEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(coordinatorUser, "Coordinador123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(coordinatorUser, "Coordinador");
            }
        }

        // 3. Crear 3 cursos activos si no existen
        if (!await context.Cursos.AnyAsync())
        {
            var cursos = new List<Curso>
            {
                new Curso
                {
                    Codigo = "CS101",
                    Nombre = "Introducción a la Programación",
                    Creditos = 4,
                    CupoMaximo = 30,
                    HorarioInicio = new TimeSpan(8, 0, 0),
                    HorarioFin = new TimeSpan(10, 0, 0),
                    Activo = true
                },
                new Curso
                {
                    Codigo = "CS201",
                    Nombre = "Estructuras de Datos",
                    Creditos = 4,
                    CupoMaximo = 25,
                    HorarioInicio = new TimeSpan(10, 30, 0),
                    HorarioFin = new TimeSpan(12, 30, 0),
                    Activo = true
                },
                new Curso
                {
                    Codigo = "CS301",
                    Nombre = "Bases de Datos",
                    Creditos = 3,
                    CupoMaximo = 20,
                    HorarioInicio = new TimeSpan(14, 0, 0),
                    HorarioFin = new TimeSpan(16, 0, 0),
                    Activo = true
                }
            };

            context.Cursos.AddRange(cursos);
            await context.SaveChangesAsync();
        }
    }
}