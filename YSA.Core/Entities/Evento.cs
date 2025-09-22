using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace YSA.Core.Entities
{
    public class Evento
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        [Required]
        public DateTime FechaEvento { get; set; }

        [Required]
        [StringLength(255)]
        public string Lugar { get; set; }

        public string UrlImagen { get; set; }

        public bool EsDestacado { get; set; } = false;

        public bool EstaActivo { get; set; } = true;

        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Clave foránea para la relación con TipoEvento
        public int TipoEventoId { get; set; }

        // Propiedad de navegación para la relación uno a muchos
        [ForeignKey("TipoEventoId")]
        public TipoEvento TipoEvento { get; set; }

        // Propiedad de navegación para la relación uno a muchos con EventoFotos
        public ICollection<EventoFotos> Fotos { get; set; }
    }
}