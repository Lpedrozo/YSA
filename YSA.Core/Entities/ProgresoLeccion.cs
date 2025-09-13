using System.ComponentModel.DataAnnotations;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class ProgresoLeccion
    {
        [Key]
        public int Id { get; set; }

        public int EstudianteId { get; set; }
        public int LeccionId { get; set; }

        public bool Completado { get; set; } = false;
        public DateTime FechaCompletado { get; set; }
        public Usuario Estudiante { get; set; }
        public Leccion Leccion { get; set; }
    }
}