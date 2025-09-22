using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace YSA.Core.Entities
{
    public class EventoFotos
    {
        public int Id { get; set; }

        [Required]
        public string UrlImagen { get; set; }

        [StringLength(255)]
        public string Titulo { get; set; }

        public DateTime FechaSubida { get; set; } = DateTime.Now;

        // Clave foránea para la relación con Evento
        public int EventoId { get; set; }

        // Propiedad de navegación para la relación con Evento
        [ForeignKey("EventoId")]
        public Evento Evento { get; set; }
    }
}