// YSA.Core.Entities.Artista

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class Artista
    {
        [Key]
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        [StringLength(255)]
        public string NombreArtistico { get; set; }
        public string Biografia { get; set; }
        [StringLength(255)]
        public string EstiloPrincipal { get; set; }
        public bool EsAcademia { get; set; }
        [StringLength(50)]
        public string EstadoAprobacion { get; set; }
        public DateTime? FechaSolicitud { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public int? AprobadoPorId { get; set; }
        public Usuario AprobadoPor { get; set; }

        [StringLength(500)]
        public string MotivoRechazo { get; set; }
        public virtual ICollection<Producto> Productos { get; set; }
        public ICollection<PreguntaRespuesta> PreguntasRespuestas { get; set; }
        public virtual ICollection<ArtistaFoto> Portafolio { get; set; }
        public virtual ICollection<CursoInstructor> CursosInstructores { get; set; }
    }
}