using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using YSA.Core.Entities;
using YSA.Core.Enums;

namespace YSA.Core.Entities
{
    public class RecursoActividad
    {
        public int Id { get; set; }
        public string TipoEntidad { get; set; } = string.Empty;
        public int EntidadId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string TipoRecurso { get; set; } = string.Empty;
        public string? Url { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public bool RequiereEntrega { get; set; }
        public ICollection<EntregaActividad> Entregas { get; set; } = new List<EntregaActividad>();
    }
}