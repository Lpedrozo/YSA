using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class CursoInstructor
    {
        public int CursoId { get; set; }
        public Curso Curso { get; set; }
        public int ArtistaId { get; set; }
        public Artista Artista { get; set; }

    }
}