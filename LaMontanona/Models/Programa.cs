using System.ComponentModel.DataAnnotations;
namespace LaMontanona.Models;

public class Programa
{
    [Key] public int Id { get; set; }
    [Required, StringLength(150)] public string Nombre { get; set; } = "";
    [StringLength(600)] public string? Descripcion { get; set; }
    public int Orden { get; set; }
    public ICollection<InscripcionPrograma> Inscripciones { get; set; } = new List<InscripcionPrograma>();
}

// Tabla puente N:M Participante <-> Programa
public class InscripcionPrograma
{
    [Key] public int Id { get; set; }
    public int ParticipanteId { get; set; }
    public int ProgramaId { get; set; }
    [StringLength(80)] public string? Linea { get; set; } // ej. "Sistemas agropastoriles", "Turismo"
    public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;

    public Participante Participante { get; set; } = null!;
    public Programa Programa { get; set; } = null!;
}
