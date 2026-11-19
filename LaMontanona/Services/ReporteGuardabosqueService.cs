using LaMontanona.Data;
using LaMontanona.Models;
using LaMontanona.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace LaMontanona.Services;

public interface IReporteGuardabosqueService
{
    Task<int> CrearAsync(string guardabosqueId, ReporteFormModel m, byte[]? foto, string? nombreFoto);
    Task<List<ReporteGuardabosque>> MisReportesAsync(string guardabosqueId, int cantidad = 10);
}

public class ReporteGuardabosqueService(ApplicationDbContext db, IFileStorageService archivos) : IReporteGuardabosqueService
{
    public async Task<int> CrearAsync(string guardabosqueId, ReporteFormModel m, byte[]? foto, string? nombreFoto)
    {
        string? ruta = null;
        try
        {
            if (foto is { Length: > 0 } && !string.IsNullOrWhiteSpace(nombreFoto))
            {
                using var ms = new MemoryStream(foto);
                ruta = await archivos.GuardarFotoAsync(ms, nombreFoto, foto.Length, "reportes");
            }

            var r = new ReporteGuardabosque
            {
                GuardabosqueId = guardabosqueId,   // siempre el usuario autenticado, nunca un dato del formulario
                FechaPatrullaje = m.FechaPatrullaje!.Value.Date,
                Zona = m.Zona.Trim(),
                KmRecorridos = m.KmRecorridos!.Value,
                Novedades = m.Novedades.Trim(),
                RutaFoto = ruta
            };
            db.ReportesGuardabosque.Add(r);
            await db.SaveChangesAsync();
            return r.Id;
        }
        catch { archivos.Eliminar(ruta); throw; }   // no dejar fotos huérfanas si falla el guardado
        finally { db.ChangeTracker.Clear(); }
    }

    public Task<List<ReporteGuardabosque>> MisReportesAsync(string guardabosqueId, int cantidad = 10) =>
        db.ReportesGuardabosque.AsNoTracking()
          .Where(r => r.GuardabosqueId == guardabosqueId)
          .OrderByDescending(r => r.FechaPatrullaje).ThenByDescending(r => r.FechaEnvio)
          .Take(cantidad).ToListAsync();
}
