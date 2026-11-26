
using System.Globalization;
using LaMontanona.Data;
using LaMontanona.Models;
using LaMontanona.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaMontanona.Endpoints;

public record EstadoDto(int Estado);

public static class IncidenteEndpoints
{
    private static readonly string[] Tipos =
    [
        "Incendio o quema",
        "Tala ilegal",
        "Caza o tráfico de fauna",
        "Contaminación de agua",
        "Otro"
    ];

    public static void MapIncidenteEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/incidentes")
            .DisableAntiforgery();

        g.MapPost("/", Crear)
            .AllowAnonymous()
            .WithMetadata(new RequestSizeLimitAttribute(6 * 1024 * 1024));

        g.MapGet("/seguimiento/{codigo}", Seguimiento)
            .AllowAnonymous();

        g.MapGet("/", Listar)
            .RequireAuthorization(p =>
                p.RequireRole(Roles.Admin, Roles.Guardabosque));

        g.MapPut("/{id:int}/estado", CambiarEstado)
            .RequireAuthorization(p =>
                p.RequireRole(Roles.Admin));
    }

    private static string Codigo(int id) => $"MLM-{id:D5}";

    private static string EstadoTexto(EstadoIncidente estado) => estado switch
    {
        EstadoIncidente.Recibido => "Recibido",
        EstadoIncidente.EnRevision => "En revisión",
        EstadoIncidente.Atendido => "Atendido",
        _ => "Cerrado"
    };

    private static string Texto(IFormCollection form, string campo)
        => form[campo].ToString().Trim();

    private static string? TextoOpcional(string valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static double? LeerCoordenada(
        string valor,
        double minimo,
        double maximo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        if (!double.TryParse(
            valor.Trim(),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out double numero))
            return null;

        if (!double.IsFinite(numero) ||
            numero < minimo ||
            numero > maximo)
            return null;

        return numero;
    }

    private static async Task<IResult> Crear(
        HttpRequest req,
        ApplicationDbContext db,
        IFileStorageService archivos,
        INotificacionService notificaciones,
        ILogger<IncidenteEndpointsLog> logger,
        CancellationToken ct)
    {
        if (!req.HasFormContentType)
        {
            return Results.BadRequest(new
            {
                error = "El formulario no se recibió correctamente."
            });
        }

        IFormCollection form;

        try
        {
            form = await req.ReadFormAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo leer el formulario de denuncia.");

            return Results.BadRequest(new
            {
                error = "No se pudieron leer los datos enviados."
            });
        }

        var tipo = Texto(form, "tipo");
        var descripcion = Texto(form, "descripcion");
        var ubicacion = Texto(form, "ubicacion");

        if (!Tipos.Contains(tipo))
        {
            return Results.BadRequest(new
            {
                error = "Selecciona un tipo de incidente válido."
            });
        }

        if (descripcion.Length < 5 || descripcion.Length > 2000)
        {
            return Results.BadRequest(new
            {
                error = "La descripción debe tener entre 5 y 2000 caracteres."
            });
        }

        if (ubicacion.Length < 2 || ubicacion.Length > 300)
        {
            return Results.BadRequest(new
            {
                error = "Escribe una ubicación válida."
            });
        }

        var latitudTexto = Texto(form, "lat");
        var longitudTexto = Texto(form, "lng");

        double? latitud = LeerCoordenada(latitudTexto, -90, 90);
        double? longitud = LeerCoordenada(longitudTexto, -180, 180);

        if (latitudTexto.Length > 0 && latitud is null)
        {
            return Results.BadRequest(new
            {
                error = "La latitud no es válida."
            });
        }

        if (longitudTexto.Length > 0 && longitud is null)
        {
            return Results.BadRequest(new
            {
                error = "La longitud no es válida."
            });
        }

        if (latitud.HasValue != longitud.HasValue)
        {
            return Results.BadRequest(new
            {
                error = "Debes enviar latitud y longitud juntas."
            });
        }

        var anonimo =
            Texto(form, "anonimo").Equals("on", StringComparison.OrdinalIgnoreCase) ||
            Texto(form, "anonimo").Equals("true", StringComparison.OrdinalIgnoreCase);

        string? rutaFoto = null;

        try
        {
            var foto = form.Files.GetFile("foto");

            if (foto is not null && foto.Length > 0)
            {
                rutaFoto = await archivos.GuardarFotoIncidenteAsync(foto, ct);
            }
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al guardar fotografía de denuncia.");

            return Results.Problem(
                "No se pudo guardar la fotografía.",
                statusCode: 500);
        }

        var incidente = new IncidenteDenuncia
        {
            Tipo = tipo,
            Descripcion = descripcion,
            Ubicacion = ubicacion,
            Latitud = latitud,
            Longitud = longitud,
            RutaFoto = rutaFoto,
            NombreDenunciante = anonimo
                ? null
                : TextoOpcional(Texto(form, "nombre")),
            Contacto = anonimo
                ? null
                : TextoOpcional(Texto(form, "contacto")),
            Estado = EstadoIncidente.Recibido,
            FechaReporte = DateTime.UtcNow
        };

        try
        {
            db.Incidentes.Add(incidente);

            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Denuncia guardada correctamente. ID: {Id}",
                incidente.Id);
            try
            {
                await notificaciones.CrearParaRolesAsync(
                    new[] { Roles.Admin, Roles.Guardabosque },
                    "Nueva denuncia ambiental",
                    $"Se ha recibido una denuncia de tipo: {incidente.Tipo}. Ubicación: {incidente.Ubicacion}.",
                    "/admin/mapa");
            }
            catch (Exception exNotificacion)
            {
                logger.LogError(
                    exNotificacion,
                    "La denuncia se guardó, pero no se pudo generar la notificación.");
            }

            return Results.Ok(new
            {
                codigo = Codigo(incidente.Id),
                mensaje = "La denuncia se guardó correctamente."
            });
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(
                ex,
                "Error de SQL Server al guardar una denuncia.");

            if (rutaFoto is not null)
                archivos.Eliminar(rutaFoto);

            return Results.Problem(
                "SQL Server no pudo guardar la denuncia. Revisa el registro de errores del servidor.",
                statusCode: 500);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado al guardar denuncia.");

            if (rutaFoto is not null)
                archivos.Eliminar(rutaFoto);

            return Results.Problem(
                "Ocurrió un error inesperado al guardar la denuncia.",
                statusCode: 500);
        }
    }


    private static async Task<IResult> Seguimiento(
        string codigo,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        codigo = codigo.Trim();

        if (!codigo.StartsWith("MLM-", StringComparison.OrdinalIgnoreCase) ||
            !int.TryParse(codigo[4..], out var id))
            return Results.NotFound();

        var incidente = await db.Incidentes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (incidente is null)
            return Results.NotFound();

        return Results.Ok(new
        {
            tipo = incidente.Tipo,
            estado = EstadoTexto(incidente.Estado),
            fecha = incidente.FechaReporte
        });
    }

    private static async Task<IResult> Listar(
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var lista = await db.Incidentes
            .AsNoTracking()
            .OrderByDescending(i => i.FechaReporte)
            .Take(500)
            .ToListAsync(ct);

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

    private static async Task<IResult> CambiarEstado(
        int id,
        EstadoDto dto,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(EstadoIncidente), dto.Estado))
        {
            return Results.BadRequest(new
            {
                error = "Estado no válido."
            });
        }

        var cantidad = await db.Incidentes
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(
                s => s.SetProperty(
                    x => x.Estado,
                    (EstadoIncidente)dto.Estado),
                ct);

        return cantidad == 0
            ? Results.NotFound()
            : Results.NoContent();
    }
}

// Categoría para los registros del endpoint.
public sealed class IncidenteEndpointsLog;
