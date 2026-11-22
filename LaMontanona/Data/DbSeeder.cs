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
    }

    private static async Task CrearUsuarioAsync(UserManager<IdentityUser> users, string? email, string? pwd, string rol)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pwd)) return;
        if (await users.FindByEmailAsync(email) is not null) return;
        var u = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        if ((await users.CreateAsync(u, pwd)).Succeeded) await users.AddToRoleAsync(u, rol);
    }
}
