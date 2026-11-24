using LaMontanona.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace LaMontanona.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Participante> Participantes => Set<Participante>();
    public DbSet<Programa> Programas => Set<Programa>();
    public DbSet<InscripcionPrograma> Inscripciones => Set<InscripcionPrograma>();
    public DbSet<IncidenteDenuncia> Incidentes => Set<IncidenteDenuncia>();
    public DbSet<ReporteGuardabosque> ReportesGuardabosque => Set<ReporteGuardabosque>();
    public DbSet<PronosticoObservatorio> Pronosticos => Set<PronosticoObservatorio>();
    public DbSet<Actividad> Actividades => Set<Actividad>();
    public DbSet<Noticia> Noticias => Set<Noticia>();
    public DbSet<Alianza> Alianzas => Set<Alianza>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Registro Único: un DUI = un participante, aunque esté en varios ejes
        b.Entity<Participante>().HasIndex(p => p.DUI).IsUnique();

        // N:M explícita; una familia no se inscribe dos veces al mismo programa
        b.Entity<InscripcionPrograma>().HasIndex(i => new { i.ParticipanteId, i.ProgramaId }).IsUnique();
        b.Entity<InscripcionPrograma>().HasOne(i => i.Participante).WithMany(p => p.Inscripciones)
            .HasForeignKey(i => i.ParticipanteId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<InscripcionPrograma>().HasOne(i => i.Programa).WithMany(p => p.Inscripciones)
            .HasForeignKey(i => i.ProgramaId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<ReporteGuardabosque>().HasOne(r => r.Guardabosque).WithMany()
            .HasForeignKey(r => r.GuardabosqueId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ReporteGuardabosque>().Property(r => r.KmRecorridos).HasPrecision(6, 2);

        b.Entity<Actividad>().Property(a => a.Meta).HasPrecision(12, 2);
        b.Entity<Actividad>().Property(a => a.Avance).HasPrecision(12, 2);
        b.Entity<Actividad>().HasOne(a => a.Programa).WithMany()
            .HasForeignKey(a => a.ProgramaId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Programa>().HasData(
            new Programa { Id = 1, Orden = 1, Nombre = "Protección y conservación del bosque y la biodiversidad" },
            new Programa { Id = 2, Orden = 2, Nombre = "Restauración y producción sostenible", Descripcion = "Incluye sistemas agropastoriles que generan ingresos familiares." },
            new Programa { Id = 3, Orden = 3, Nombre = "Gobernanza para la gestión hídrica" },
            new Programa { Id = 4, Orden = 4, Nombre = "Fortalecimiento institucional" },
            new Programa { Id = 5, Orden = 5, Nombre = "Desarrollo económico local y turismo" });
    }
}
