using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace portal_academico.Models;

[Table("Matriculas")]
public class Matricula
{
    public int Id { get; set; }

    [Required]
    public int CursoId { get; set; }

    [ForeignKey("CursoId")]
    public virtual Curso? Curso { get; set; }

    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    [ForeignKey("UsuarioId")]
    public virtual IdentityUser? Usuario { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    [Display(Name = "Fecha de Registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    [Required]
    [Display(Name = "Estado")]
    public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;
}

public enum EstadoMatricula
{
    Pendiente,
    Confirmada,
    Cancelada
}