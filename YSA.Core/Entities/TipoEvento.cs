using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace YSA.Core.Entities
{
    public class TipoEvento
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreTipo { get; set; }

        [Required]
        [StringLength(50)]
        public string Plataforma { get; set; }

        // Propiedad de navegación para la relación uno a muchos
        public ICollection<Evento> Eventos { get; set; }
    }
}