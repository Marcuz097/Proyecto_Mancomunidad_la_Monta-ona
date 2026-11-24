using System.ComponentModel.DataAnnotations;
namespace LaMontanona.Models;

/// <summary>Actividad con meta y avance dentro de un programa del plan de trabajo.</summary>
public class Actividad
{
    [Key] public int Id { get; set; }
    public int ProgramaId { get; set; }
    public Programa Programa { get; set; } = null!;
    [Required, StringLength(200)] public string Nombre { get; set; } = "";
    [StringLength(30)] public string Unidad { get; set; } = "";   // familias, hectáreas, talleres...
    public decimal Meta { get; set; }
    public decimal Avance { get; set; }
    public bool Ejemplo { get; set; }                              // dato de ejemplo por confirmar
}

public class Noticia
{
    [Key] public int Id { get; set; }
    [Required, StringLength(160)] public string Titulo { get; set; } = "";
    [StringLength(40)] public string Categoria { get; set; } = "";
    [Required, StringLength(600)] public string Resumen { get; set; } = "";
    [StringLength(300)] public string? RutaImagen { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public bool Publicada { get; set; } = true;
}

public class Alianza
{
    [Key] public int Id { get; set; }
    [Required, StringLength(120)] public string Nombre { get; set; } = "";
    [StringLength(400)] public string Descripcion { get; set; } = "";
    [StringLength(300)] public string? RutaLogo { get; set; }
    public int Orden { get; set; }
    public bool Publicada { get; set; } = true;
}
