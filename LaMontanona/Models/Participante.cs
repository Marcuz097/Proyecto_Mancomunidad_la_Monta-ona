using System.ComponentModel.DataAnnotations;
namespace LaMontanona.Models;

public class Participante
{
    [Key] public int Id { get; set; }

    [Required, StringLength(10), RegularExpression(@"^\d{8}-\d$", ErrorMessage = "Formato de DUI: 00000000-0")]
    public string DUI { get; set; } = "";

    [Required, StringLength(100)] public string Nombres { get; set; } = "";
    [Required, StringLength(100)] public string Apellidos { get; set; } = "";
    [DataType(DataType.Date)] public DateTime FechaNacimiento { get; set; }
    [StringLength(15)] public string? Telefono { get; set; }
    [Required, StringLength(120)] public string Comunidad { get; set; } = "";
    [Required, StringLength(80)] public string Municipio { get; set; } = "";
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public ICollection<InscripcionPrograma> Inscripciones { get; set; } = new List<InscripcionPrograma>();
}
