using System.Globalization;
using LaMontanona.Data;
using LaMontanona.Models;
using LaMontanona.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace LaMontanona.Endpoints;

public record EstadoDto(int Estado);

/// <summary>
/// Denuncias reales: las recibe el formulario público de Contacto, el equipo las ve en el mapa
/// y el administrador cambia su estado. La persona consulta su caso con el código de seguimiento.
/// </summary>
public static class IncidenteEndpoints
{
    static readonly string[] Tipos =
        ["Incendio o quema", "Tala ilegal", "Caza o tráfico de fauna", "Contaminación de agua", "Otro"];

    public static void MapIncidenteEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/incidentes").DisableAntiforgery();

        g.MapPost("/", Crear).AllowAnonymous()
         .WithMetadata(new RequestSizeLimitAttribute(6 * 1024 * 1024));
        g.MapGet("/seguimiento/{codigo}", Seguimiento).AllowAnonymous();
        g.MapGet("/", Listar).RequireAuthorization(p => p.RequireRole(Roles.Admin, Roles.Guardabosque));
        g.MapPut("/{id:int}/estado", CambiarEstado).RequireAuthorization(p => p.RequireRole(Roles.Admin));
    }

    static string Codigo(int id) => $"MLM-{id:D5}";

    static string EstadoTexto(EstadoIncidente e) => e switch
    {
        EstadoIncidente.Recibido => "Recibido",
        EstadoIncidente.EnRevision => "En revisión",
        EstadoIncidente.Atendido => "Atendido",
        _ => "Cerrado"
    };

    static double? Coord(string s, double min, double max) =>
        double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) && v >= min && v <= max ? v : null;

    static string? NullSi(string s) => s.Length == 0 ? null : s;

    static async Task<IResult> Crear(HttpRequest req, ApplicationDbContext db, IFileStorageService archivos, CancellationToken ct)
    {
        if (!req.HasFormContentType) return Results.BadRequest(new { error = "Solicitud no válida." });
        var f = await req.ReadFormAsync(ct);

        string Txt(string k, int max)
        {
            var s = f[k].ToString().Trim();
            return s.Length > max ? s[..max] : s;
        }

        var tipo = Txt("tipo", 80);
        var descripcion = Txt("descripcion", 2000);
        var ubicacion = Txt("ubicacion", 300);
        if (!Tipos.Contains(tipo)) return Results.BadRequest(new { error = "Elige el tipo de incidente." });
        if (descripcion.Length < 5 || ubicacion.Length < 2)
            return Results.BadRequest(new { error = "Completa la ubicación y cuéntanos qué está pasando." });

        var anonimo = f["anonimo"].ToString() is "on" or "true";

        string? ruta = null;
        try { ruta = await archivos.GuardarFotoIncidenteAsync(f.Files.GetFile("foto"), ct); }
        catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }

        var inc = new IncidenteDenuncia
        {
            Tipo = tipo,
            Descripcion = descripcion,
            Ubicacion = ubicacion,
            Latitud = Coord(f["lat"].ToString(), -90, 90),
            Longitud = Coord(f["lng"].ToString(), -180, 180),
            RutaFoto = ruta,
            NombreDenunciante = anonimo ? null : NullSi(Txt("nombre", 120)),
            Contacto = anonimo ? null : NullSi(Txt("contacto", 120))
        };
        db.Incidentes.Add(inc);
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { codigo = Codigo(inc.Id) });
    }

    // Público: solo revela tipo, estado y fecha (nada de ubicación ni datos personales)
    static async Task<IResult> Seguimiento(string codigo, ApplicationDbContext db, CancellationToken ct)
    {
        codigo = codigo.Trim();
        if (!codigo.StartsWith("MLM-", StringComparison.OrdinalIgnoreCase) || !int.TryParse(codigo[4..], out var id))
            return Results.NotFound();
        var i = await db.Incidentes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return i is null
            ? Results.NotFound()
            : Results.Ok(new { tipo = i.Tipo, estado = EstadoTexto(i.Estado), fecha = i.FechaReporte });
    }

    static async Task<IResult> Listar(ApplicationDbContext db, CancellationToken ct)
    {
        var lista = await db.Incidentes.AsNoTracking()
            .OrderByDescending(i => i.FechaReporte).Take(500).ToListAsync(ct);
        return Results.Ok(lista.Select(i => new
        {
            i.Id,
            codigo = Codigo(i.Id),
            i.Tipo,
            i.Descripcion,
            i.Ubicacion,
            i.Latitud,
            i.Longitud,
            foto = i.RutaFoto,
            estado = (int)i.Estado,
            estadoTexto = EstadoTexto(i.Estado),
            fecha = i.FechaReporte
        }));
    }

    static async Task<IResult> CambiarEstado(int id, EstadoDto dto, ApplicationDbContext db, CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(EstadoIncidente), dto.Estado))
            return Results.BadRequest(new { error = "Estado no válido." });
        var n = await db.Incidentes.Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Estado, (EstadoIncidente)dto.Estado), ct);
        return n == 0 ? Results.NotFound() : Results.NoContent();
    }
}
