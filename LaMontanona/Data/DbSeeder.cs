using LaMontanona.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace LaMontanona.Data;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Guardabosque = "Guardabosque";
}

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        var db = sp.GetRequiredService<ApplicationDbContext>();
        var roles = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var users = sp.GetRequiredService<UserManager<IdentityUser>>();
        var cfg = sp.GetRequiredService<IConfiguration>();

        foreach (var r in new[] { Roles.Admin, Roles.Guardabosque })
            
           if (!await roles.RoleExistsAsync(r)) await roles.CreateAsync(new IdentityRole(r));

        await CrearUsuarioAsync(users, cfg["SeedAdmin:Email"], cfg["SeedAdmin:Password"], Roles.Admin);
        await CrearUsuarioAsync(users, cfg["SeedGuardabosque:Email"], cfg["SeedGuardabosque:Password"], Roles.Guardabosque);

        if (!await db.Pronosticos.AnyAsync())
        {
            db.Pronosticos.Add(new PronosticoObservatorio
            {
                Resumen = "Lluvias dispersas en la tarde y viento leve.",
                TempMinC = 17, TempMaxC = 27, ProbLluviaPct = 70,
                Alerta = NivelAlerta.Precaucion,
                MensajeAlerta = "Tormentas en la tarde. Evite quemas y senderos cercanos a quebradas.",
                Publicado = true
            });
            await db.SaveChangesAsync();
        }

        if (!await db.Actividades.AnyAsync())
            {
                db.Actividades.AddRange(
                    new Actividad { ProgramaId = 1, Nombre = "Patrullajes de vigilancia", Unidad = "rondas", Meta = 48, Avance = 31, Ejemplo = true },
                    new Actividad { ProgramaId = 1, Nombre = "Brigadas contra incendios formadas", Unidad = "brigadas", Meta = 6, Avance = 4, Ejemplo = true },
                    new Actividad { ProgramaId = 2, Nombre = "Familias con sistemas agropastoriles", Unidad = "familias", Meta = 120, Avance = 45, Ejemplo = true },
                    new Actividad { ProgramaId = 2, Nombre = "Superficie restaurada", Unidad = "hectáreas", Meta = 80, Avance = 22, Ejemplo = true },
                    new Actividad { ProgramaId = 3, Nombre = "Operadoras de agua (OSAS) capacitadas", Unidad = "OSAS", Meta = 50, Avance = 18, Ejemplo = true },
                    new Actividad { ProgramaId = 3, Nombre = "Mesas del agua realizadas", Unidad = "mesas", Meta = 8, Avance = 5, Ejemplo = true },
                    new Actividad { ProgramaId = 4, Nombre = "Planes y protocolos aprobados", Unidad = "documentos", Meta = 5, Avance = 2, Ejemplo = true },
                    new Actividad { ProgramaId = 4, Nombre = "Personal técnico formado", Unidad = "personas", Meta = 20, Avance = 12, Ejemplo = true },
                    new Actividad { ProgramaId = 5, Nombre = "Emprendimientos turísticos apoyados", Unidad = "emprendimientos", Meta = 10, Avance = 3, Ejemplo = true },
                    new Actividad { ProgramaId = 5, Nombre = "Guías locales certificados", Unidad = "guías", Meta = 15, Avance = 6, Ejemplo = true });
                await db.SaveChangesAsync();
            }

            if (!await db.Noticias.AnyAsync())
            {
                db.Noticias.AddRange(
                    new Noticia { Titulo = "Brigadas comunitarias refuerzan la prevención de incendios", Categoria = "Prevención", Resumen = "Familias y guardabosques coordinan rondas en las zonas más expuestas.", RutaImagen = "/img/turismo-sendero.jpg", Fecha = DateTime.UtcNow.AddDays(-2) },
                    new Noticia { Titulo = "Mesa del agua con las comunidades", Categoria = "Agua", Resumen = "Acuerdos para cuidar las fuentes de la cuenca.", RutaImagen = "/img/SRLAguayGobernanza.jpg", Fecha = DateTime.UtcNow.AddDays(-5) },
                    new Noticia { Titulo = "Nuevos sistemas agropastoriles en marcha", Categoria = "Producción", Resumen = "Más hogares mejoran sus ingresos sin talar bosque.", RutaImagen = "/img/login-bg.jpg", Fecha = DateTime.UtcNow.AddDays(-9) });
                await db.SaveChangesAsync();
            }

                if (!await db.Alianzas.AnyAsync())
                {
                    db.Alianzas.AddRange(
                        new Alianza { Nombre = "ACACYPAC", Descripcion = "Organización aliada del territorio. Descripción del convenio por confirmar.", Orden = 1 },
                        new Alianza { Nombre = "Gobiernos locales", Descripcion = "Alcaldías de los municipios que integran la Mancomunidad.", Orden = 2 },
                        new Alianza { Nombre = "Cooperación técnica", Descripcion = "Apoyo técnico y financiero para los programas de trabajo.", Orden = 3 });
                    await db.SaveChangesAsync();
                }
            }

    private static async Task CrearUsuarioAsync(UserManager<IdentityUser> users, string? email, string? pwd, string rol)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pwd)) return;
        if (await users.FindByEmailAsync(email) is not null) return;
        var u = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        if ((await users.CreateAsync(u, pwd)).Succeeded) await users.AddToRoleAsync(u, rol);
    }
}
