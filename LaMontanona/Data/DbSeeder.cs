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

        var email = cfg["SeedAdmin:Email"]; var pwd = cfg["SeedAdmin:Password"];
        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(pwd) && await users.FindByEmailAsync(email) is null)
        {
            var u = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            if ((await users.CreateAsync(u, pwd)).Succeeded) await users.AddToRoleAsync(u, Roles.Admin);
        }

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
}
