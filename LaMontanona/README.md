# Mancomunidad La Montañona — Fase 1 (base del proyecto)

Blazor (.NET 8) + Razor Pages (Identity UI) + Entity Framework Core + SQL Server.

## Requisitos
Visual Studio 2022 17.8+ (carga de trabajo ASP.NET) o .NET SDK 8, y SQL Server (LocalDB viene con Visual Studio).

## Arranque
1. Abre `LaMontanona.csproj` en Visual Studio (o `dotnet restore`).
2. Revisa `ConnectionStrings:DefaultConnection` en `appsettings.json`.
3. Ejecuta (F5 o `dotnet run`). Al iniciar se crea la base `LaMontanonaDb`, los roles `Admin` y `Guardabosque`, el usuario administrador, los 5 programas y un pronóstico de ejemplo.
4. Ingreso: `/Identity/Account/Login` con `admin@lamontanona.local` / `Cambiar#2026!`. **Cambia esa clave** en `appsettings.json` antes de publicar.

## Migraciones (recomendado desde ya)
Borra la base creada en el primer arranque y ejecuta en la Consola del Administrador de paquetes:
`Add-Migration Inicial` y luego `Update-Database` (o `dotnet ef migrations add Inicial`). Desde ese momento `Program.cs` aplica las migraciones solo.

## Estructura
- `Models/` Participante, Programa, InscripcionPrograma, IncidenteDenuncia, ReporteGuardabosque, PronosticoObservatorio.
- `Data/` ApplicationDbContext (DUI único, N:M, datos semilla) y DbSeeder.
- `Services/` FileStorageService (fotos a `wwwroot/uploads/incidentes`, solo JPG/PNG/WEBP, máx. 5 MB).
- `Components/` App, Routes, layout y `Pages/Home.razor` (portada).
- `wwwroot/` css, js, uploads y videos de Somos Río Lempa.

## Pendiente (fase 2)
Formularios de Registro Único, Denuncias y Guardabosques; panel administrativo; Reporte Unificado con exportación a Excel y PDF; pronóstico leído desde la base de datos.
