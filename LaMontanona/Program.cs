using LaMontanona.Components;
using LaMontanona.Data;
using LaMontanona.Services;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("DefaultConnection")
         ?? throw new InvalidOperationException("Falta la cadena de conexión DefaultConnection.");
builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(cs));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(o => o.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Google Login - va antes del Build
var auth = builder.Services.AddAuthentication();
var gId = builder.Configuration["Authentication:Google:ClientId"];
if (!string.IsNullOrEmpty(gId))
    auth.AddGoogle(o =>
    {
        o.ClientId = gId;
        o.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    });

builder.Services.AddRazorComponents().AddInteractiveServerComponents(); // Blazor
builder.Services.AddRazorPages();                                        // Razor Pages (Identity UI)
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IParticipanteService, ParticipanteService>();
builder.Services.AddScoped<IReporteGuardabosqueService, ReporteGuardabosqueService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Fase 1: si aún no hay migraciones se crea el esquema; con migraciones se aplican.
    if (db.Database.GetMigrations().Any()) await db.Database.MigrateAsync();
    else await db.Database.EnsureCreatedAsync();
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment()) app.UseMigrationsEndPoint();
else { app.UseExceptionHandler("/Error"); app.UseHsts(); }

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapRazorPages();
app.Run();