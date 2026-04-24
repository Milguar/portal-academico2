using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace portal_academico.Models;

[Table("Cursos")]
public class Curso
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [Range(1, 20, ErrorMessage = "Los créditos deben estar entre 1 y 20")]
    [Display(Name = "Créditos")]
    public int Creditos { get; set; }

    [Required]
    [Display(Name = "Cupo Máximo")]
    public int CupoMaximo { get; set; }

    [Required]
    [DataType(DataType.Time)]
    [Display(Name = "Hora de Inicio")]
    public TimeSpan HorarioInicio { get; set; }

    [Required]
    [DataType(DataType.Time)]
    [Display(Name = "Hora de Fin")]
    public TimeSpan HorarioFin { get; set; }

    [Required]
    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;
}