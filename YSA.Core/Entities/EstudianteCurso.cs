using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class EstudianteCurso
    {
        public int Id { get; set; }
        public int EstudianteId { get; set; }
        public Usuario Estudiante { get; set; } 
        public int CursoId { get; set; }
        public Curso Curso { get; set; } 
        public DateTime FechaAccesoOtorgado { get; set; }
    }
}