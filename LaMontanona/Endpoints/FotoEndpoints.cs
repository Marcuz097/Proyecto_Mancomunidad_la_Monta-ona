using LaMontanona.Data;
using LaMontanona.Services;
using Microsoft.AspNetCore.Mvc;
namespace LaMontanona.Endpoints;


/// <summary>
/// Las fotos se suben por HTTP normal (multipart), fuera de la conexión SignalR de Blazor.
/// Así un archivo pesado o una cancelación nunca puede tumbar el circuito de la página.
/// </summary>
public static class FotoEndpoints
{
    public static void MapFotoEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/fotos").DisableAntiforgery();

        // Solo guardabosques y administradores
        g.MapPost("/reportes", Subir("reportes"))
         .RequireAuthorization(p => p.RequireRole(Roles.Guardabosque, Roles.Admin))
         .WithMetadata(new RequestSizeLimitAttribute(6 * 1024 * 1024));

        // Denuncias públicas (se usará en el formulario de incidentes)
        g.MapPost("/incidentes", Subir("incidentes"))
         .AllowAnonymous()
         .WithMetadata(new RequestSizeLimitAttribute(6 * 1024 * 1024));
    }

    private static Delegate Subir(string carpeta) =>
        async (HttpRequest req, IFileStorageService archivos, CancellationToken ct) =>
        {
            if (!req.HasFormContentType) return Results.BadRequest(new { error = "Solicitud no válida." });
            var form = await req.ReadFormAsync(ct);
            var archivo = form.Files.FirstOrDefault();
            if (archivo is null || archivo.Length == 0) return Results.BadRequest(new { error = "No se recibió ninguna foto." });
            try
            {
                await using var s = archivo.OpenReadStream();
                var ruta = await archivos.GuardarFotoAsync(s, archivo.FileName, archivo.Length, carpeta, ct);
                return Results.Ok(new { ruta });
            }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        };
}
