using System.ComponentModel.DataAnnotations;
namespace LaMontanona.ViewModels;

public class ParticipanteFormModel
{
    private string _dui = "";

    [Required(ErrorMessage = "Ingresa el DUI.")]
    [DuiValido]
    public string DUI { get => _dui; set => _dui = Normalizar(value); }

    [Required(ErrorMessage = "Ingresa los nombres."), StringLength(100)]
    public string Nombres { get; set; } = "";

    [Required(ErrorMessage = "Ingresa los apellidos."), StringLength(100)]
    public string Apellidos { get; set; } = "";

    [Required(ErrorMessage = "Ingresa la fecha de nacimiento."), FechaNacimientoValida]
    public DateTime? FechaNacimiento { get; set; }

    [RegularExpression(@"^\d{4}-?\d{4}$", ErrorMessage = "Teléfono de 8 dígitos, ej. 7000-0000.")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "Ingresa la comunidad."), StringLength(120)]
    public string Comunidad { get; set; } = "";

    [Required(ErrorMessage = "Ingresa el municipio."), StringLength(80)]
    public string Municipio { get; set; } = "";

    [MinLength(1, ErrorMessage = "Elige al menos un programa.")]
    public List<int> ProgramaIds { get; set; } = new();

    // "012345678" -> "01234567-8"
    public static string Normalizar(string? v)
    {
        var d = new string((v ?? "").Where(char.IsDigit).ToArray());
        return d.Length == 9 ? $"{d[..8]}-{d[8]}" : (v ?? "").Trim();
    }
}

public class DuiValidoAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        var s = value as string ?? "";
        if (string.IsNullOrWhiteSpace(s)) return ValidationResult.Success; // lo cubre [Required]
        var d = new string(s.Where(char.IsDigit).ToArray());
        if (d.Length != 9 || s.Length != 10 || s[8] != '-')
            return new ValidationResult("El DUI debe tener el formato 00000000-0.");

        var suma = 0;
        for (var i = 0; i < 8; i++) suma += (d[i] - '0') * (9 - i);   // pesos 9..2
        var verificador = (10 - suma % 10) % 10;
        return verificador == d[8] - '0' ? ValidationResult.Success
                                         : new ValidationResult("El DUI no es válido: revisa el último dígito.");
    }
}

public class FechaNacimientoValidaAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        if (value is not DateTime f) return ValidationResult.Success;
        if (f.Date > DateTime.Today || f.Year < 1900)
            return new ValidationResult("Ingresa una fecha de nacimiento válida.");
        return ValidationResult.Success;
    }
}
