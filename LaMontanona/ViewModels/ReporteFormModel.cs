using System.ComponentModel.DataAnnotations;
namespace LaMontanona.ViewModels;

public class ReporteFormModel
{
    [Required(ErrorMessage = "Indica la fecha del patrullaje."), FechaPatrullajeValida]
    public DateTime? FechaPatrullaje { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Indica la zona patrullada."), StringLength(120)]
    public string Zona { get; set; } = "";

    [Required(ErrorMessage = "Indica los kilómetros recorridos.")]
    [Range(0, 500, ErrorMessage = "Los kilómetros deben estar entre 0 y 500.")]
    public decimal? KmRecorridos { get; set; }

    [Required(ErrorMessage = "Describe las novedades del patrullaje.")]
    [StringLength(3000, MinimumLength = 10, ErrorMessage = "Escribe al menos 10 caracteres (máximo 3000).")]
    public string Novedades { get; set; } = "";
}

public class FechaPatrullajeValidaAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        if (value is not DateTime f) return ValidationResult.Success;
        if (f.Date > DateTime.Today) return new ValidationResult("La fecha no puede ser futura.");
        if (f.Date < DateTime.Today.AddDays(-60)) return new ValidationResult("Solo se aceptan patrullajes de los últimos 60 días.");
        return ValidationResult.Success;
    }
}
