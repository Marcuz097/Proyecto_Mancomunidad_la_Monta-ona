using LaMontanona.Data;
using LaMontanona.Models;
using LaMontanona.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace LaMontanona.Services;

public interface IReporteGuardabosqueService
{
    Task<int> CrearAsync(string guardabosqueId, ReporteFormModel m, string? fotoRuta);
    Task<List<ReporteGuardabosque>> MisReportesAsync(string guardabosqueId, int cantidad = 10);
}

public class ReporteGuardabosqueService(ApplicationDbContext db, IFileStorageService archivos) : IReporteGuardabosqueService
{
    public async Task<int> CrearAsync(string guardabosqueId, ReporteFormModel m, string? fotoRuta)
    {
        if (!string.IsNullOrWhiteSpace(fotoRuta) && !archivos.EsRutaValida(fotoRuta, "reportes"))
            throw new InvalidOperationException("La foto no es válida. Súbela de nuevo.");
        try
        {
            var r = new ReporteGuardabosque
            {
                GuardabosqueId = guardabosqueId,   // siempre el usuario autenticado
                FechaPatrullaje = m.FechaPatrullaje!.Value.Date,
                Zona = m.Zona.Trim(),
                KmRecorridos = m.KmRecorridos!.Value,
                Novedades = m.Novedades.Trim(),
                RutaFoto = string.IsNullOrWhiteSpace(fotoRuta) ? null : fotoRuta
            };
            db.ReportesGuardabosque.Add(r);
            await db.SaveChangesAsync();
            return r.Id;
        }
        finally { db.ChangeTracker.Clear(); }
    }

    public Task<List<ReporteGuardabosque>> MisReportesAsync(string guardabosqueId, int cantidad = 10) =>
        db.ReportesGuardabosque.AsNoTracking()
          .Where(r => r.GuardabosqueId == guardabosqueId)
          .OrderByDescending(r => r.FechaPatrullaje).ThenByDescending(r => r.FechaEnvio)
          .Take(cantidad).ToListAsync();
}
