using System.ComponentModel.DataAnnotations;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class CursoCategoria
    {
        [Key]
        public int CursoId { get; set; }
        public Curso Curso { get; set; }

        [Key]
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
    }
}