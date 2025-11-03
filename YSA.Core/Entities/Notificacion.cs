using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using YSA.Core.Entities;
using YSA.Core.Enums;

namespace YSA.Core.Entities
{
    public class Notificacion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int TipoNotificacionId { get; set; }

        [Required]
        [StringLength(255)]
        public string Titulo { get; set; }

        [Required]
        public string Mensaje { get; set; }

        [StringLength(500)]
        public string UrlDestino { get; set; }

        public int? EntidadId { get; set; }

        [StringLength(50)]
        public string TipoEntidad { get; set; }

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaLeida { get; set; }
        public bool EsLeida { get; set; }
        public bool EsEnviada { get; set; }

        // Navigation properties
        public virtual Usuario Usuario { get; set; }
        public virtual TipoNotificacion TipoNotificacion { get; set; }
    }
}