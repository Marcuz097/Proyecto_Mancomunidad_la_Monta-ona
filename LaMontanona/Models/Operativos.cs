using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace LaMontanona.Models;

public enum EstadoIncidente { Recibido, EnRevision, Atendido, Cerrado }
public enum NivelAlerta { Ninguna, Vigilancia, Precaucion, Alerta }

public class IncidenteDenuncia
{
    [Key] public int Id { get; set; }
    [Required, StringLength(80)] public string Tipo { get; set; } = "";
    [Required, StringLength(2000)] public string Descripcion { get; set; } = "";
    [Required, StringLength(300)] public string Ubicacion { get; set; } = "";
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }
    [StringLength(300)] public string? RutaFoto { get; set; }      // /uploads/incidentes/guid.jpg
    [StringLength(120)] public string? NombreDenunciante { get; set; } // opcional: denuncia anónima
    [StringLength(120)] public string? Contacto { get; set; }
    public EstadoIncidente Estado { get; set; } = EstadoIncidente.Recibido;
    public DateTime FechaReporte { get; set; } = DateTime.UtcNow;
}

public class ReporteGuardabosque
{
    [Key] public int Id { get; set; }
    [Required] public string GuardabosqueId { get; set; } = "";   // FK a AspNetUsers (1:N)
    public IdentityUser Guardabosque { get; set; } = null!;
    [DataType(DataType.Date)] public DateTime FechaPatrullaje { get; set; }
    [Required, StringLength(120)] public string Zona { get; set; } = "";
    [Range(0, 500)] public decimal KmRecorridos { get; set; }
    [Required, StringLength(3000)] public string Novedades { get; set; } = "";
    [StringLength(300)] public string? RutaFoto { get; set; }
    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
}

public class PronosticoObservatorio
{
    [Key] public int Id { get; set; }
    [Required, StringLength(120)] public string Titulo { get; set; } = "Pronóstico oficial 48 horas";
    [Required, StringLength(800)] public string Resumen { get; set; } = "";
    public int TempMinC { get; set; }
    public int TempMaxC { get; set; }
    [Range(0, 100)] public int ProbLluviaPct { get; set; }
    public NivelAlerta Alerta { get; set; } = NivelAlerta.Ninguna;
    [StringLength(400)] public string? MensajeAlerta { get; set; }
    public DateTime VigenteDesde { get; set; } = DateTime.UtcNow;
    public bool Publicado { get; set; }
}
