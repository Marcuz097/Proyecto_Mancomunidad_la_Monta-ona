using System.Globalization;
using System.Text;
using LaMontanona.Data;
using Microsoft.EntityFrameworkCore;
namespace LaMontanona.Endpoints;

public static class AvanceEndpoints
{
    public static void MapAvanceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/avance/exportar", async (ApplicationDbContext db, CancellationToken ct) =>
        {
            var acts = await db.Actividades.AsNoTracking().Include(a => a.Programa)
                .OrderBy(a => a.Programa.Orden).ThenBy(a => a.Nombre).ToListAsync(ct);
            var inscritos = await db.Inscripciones.AsNoTracking().GroupBy(i => i.ProgramaId)
                .Select(g => new { g.Key, N = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.N, ct);

            static string Q(string s) => "\"" + s.Replace("\"", "\"\"") + "\"";
            static string N(decimal d) => d.ToString("0.##", CultureInfo.InvariantCulture);

            var sb = new StringBuilder();
            sb.Append("sep=;\r\n");
            sb.Append("Programa;Actividad;Unidad;Meta;Avance;% de avance;Participantes inscritos en el programa\r\n");
            foreach (var a in acts)
            {
                var pct = a.Meta > 0 ? Math.Round(a.Avance / a.Meta * 100, 1) : 0;
                sb.Append(string.Join(';', Q(a.Programa.Nombre), Q(a.Nombre), Q(a.Unidad),
                    N(a.Meta), N(a.Avance), N(pct), inscritos.GetValueOrDefault(a.ProgramaId)));
                sb.Append("\r\n");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return Results.File(bytes, "text/csv; charset=utf-8", $"avance-programas-{DateTime.Now:yyyyMMdd}.csv");
        }).RequireAuthorization(p => p.RequireRole(Roles.Admin));
    }
}
