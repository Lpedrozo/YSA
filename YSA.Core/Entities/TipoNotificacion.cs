using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using YSA.Core.Entities;
using YSA.Core.Enums;

namespace YSA.Core.Entities
{
    public class TipoNotificacion
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(255)]
        public string Descripcion { get; set; }

        [StringLength(50)]
        public string Icono { get; set; }

        [StringLength(20)]
        public string Color { get; set; }

        public virtual ICollection<Notificacion> Notificaciones { get; set; }
    }
}