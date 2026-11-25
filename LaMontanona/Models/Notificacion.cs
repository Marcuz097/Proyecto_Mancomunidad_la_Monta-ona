    using System.ComponentModel.DataAnnotations;

    namespace LaMontanona.Models;

    public class Notificacion
    {
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = "";

        [Required, StringLength(120)]
        public string Titulo { get; set; } = "";

        [Required, StringLength(600)]
        public string Mensaje { get; set; } = "";

        [StringLength(300)]
        public string? Url { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public bool Leida { get; set; } = false;
    }
