using LaMontanona.Data;
using LaMontanona.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LaMontanona.Services;

public interface INotificacionService
{
    Task CrearParaRolesAsync(
        IEnumerable<string> roles,
        string titulo,
        string mensaje,
        string? url = null);

    Task<List<Notificacion>> ObtenerAsync(string usuarioId);

    Task<int> ContarNoLeidasAsync(string usuarioId);

    Task<bool> MarcarLeidaAsync(int id, string usuarioId);

    Task MarcarTodasLeidasAsync(string usuarioId);
}

public class NotificacionService(
    ApplicationDbContext db,
    UserManager<IdentityUser> usuarios) : INotificacionService
{
    public async Task CrearParaRolesAsync(
        IEnumerable<string> roles,
        string titulo,
        string mensaje,
        string? url = null)
    {
        var destinatarios = new HashSet<string>();

        foreach (var rol in roles.Distinct())
        {
            var miembros = await usuarios.GetUsersInRoleAsync(rol);

            foreach (var usuario in miembros)
                destinatarios.Add(usuario.Id);
        }

        if (destinatarios.Count == 0)
            return;

        var fecha = DateTime.UtcNow;

        foreach (var usuarioId in destinatarios)
        {
            db.Notificaciones.Add(new Notificacion
            {
                UsuarioId = usuarioId,
                Titulo = titulo,
                Mensaje = mensaje,
                Url = url,
                Fecha = fecha,
                Leida = false
            });
        }

        await db.SaveChangesAsync();
    }

    public Task<List<Notificacion>> ObtenerAsync(string usuarioId)
    {
        return db.Notificaciones
            .AsNoTracking()
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.Fecha)
            .Take(100)
            .ToListAsync();
    }

    public Task<int> ContarNoLeidasAsync(string usuarioId)
    {
        return db.Notificaciones
            .CountAsync(n =>
                n.UsuarioId == usuarioId &&
                !n.Leida);
    }

    public async Task<bool> MarcarLeidaAsync(int id, string usuarioId)
    {
        var notificacion = await db.Notificaciones
            .FirstOrDefaultAsync(n =>
                n.Id == id &&
                n.UsuarioId == usuarioId);

        if (notificacion is null)
            return false;

        notificacion.Leida = true;

        await db.SaveChangesAsync();
        return true;
    }

    public async Task MarcarTodasLeidasAsync(string usuarioId)
    {
        await db.Notificaciones
            .Where(n =>
                n.UsuarioId == usuarioId &&
                !n.Leida)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(n => n.Leida, true));
    }
}