using LaMontanona.Data;
using LaMontanona.Models;
using LaMontanona.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace LaMontanona.Services;

public enum EstadoRegistro { Nuevo, Actualizado, SinCambios }

public record RegistroResult(EstadoRegistro Estado, string NombreCompleto,
                             IReadOnlyList<string> Agregados, IReadOnlyList<string> YaInscritos);

public interface IParticipanteService
{
    Task<List<Programa>> ProgramasAsync();
    Task<RegistroResult> RegistrarAsync(ParticipanteFormModel m);
}

public class ParticipanteService(ApplicationDbContext db) : IParticipanteService
{
    public Task<List<Programa>> ProgramasAsync() =>
        db.Programas.AsNoTracking().OrderBy(p => p.Orden).ToListAsync();

    // Registro Único: el DUI identifica a la persona. Si ya existe, solo se le suman los programas nuevos.
    public async Task<RegistroResult> RegistrarAsync(ParticipanteFormModel m)
    {
        try
        {
            var seleccion = await db.Programas.Where(x => m.ProgramaIds.Contains(x.Id)).ToListAsync();
            if (seleccion.Count == 0) throw new InvalidOperationException("Elige al menos un programa válido.");

            var p = await db.Participantes.Include(x => x.Inscripciones).FirstOrDefaultAsync(x => x.DUI == m.DUI);
            var esNuevo = p is null;
            if (p is null)
            {
                p = new Participante
                {
                    DUI = m.DUI,
                    Nombres = m.Nombres.Trim(),
                    Apellidos = m.Apellidos.Trim(),
                    FechaNacimiento = m.FechaNacimiento!.Value,
                    Telefono = string.IsNullOrWhiteSpace(m.Telefono) ? null : m.Telefono.Trim(),
                    Comunidad = m.Comunidad.Trim(),
                    Municipio = m.Municipio.Trim()
                };
                db.Participantes.Add(p);
            }

            var yaIds = p.Inscripciones.Select(i => i.ProgramaId).ToHashSet();
            var nuevos = seleccion.Where(x => !yaIds.Contains(x.Id)).ToList();
            var existentes = seleccion.Where(x => yaIds.Contains(x.Id)).ToList();
            foreach (var prog in nuevos) p.Inscripciones.Add(new InscripcionPrograma { ProgramaId = prog.Id });

            try { await db.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException("Este DUI se registró hace un instante. Intenta de nuevo.");
            }

            var estado = esNuevo ? EstadoRegistro.Nuevo : nuevos.Count > 0 ? EstadoRegistro.Actualizado : EstadoRegistro.SinCambios;
            return new RegistroResult(estado, $"{p.Nombres} {p.Apellidos}",
                nuevos.Select(x => x.Nombre).ToList(), existentes.Select(x => x.Nombre).ToList());
        }
        finally { db.ChangeTracker.Clear(); } // el servicio vive toda la sesión Blazor
    }
}
