using System.ComponentModel.DataAnnotations;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class Leccion
    {
        [Key]
        public int Id { get; set; }

        public int ModuloId { get; set; }
        public Modulo Modulo { get; set; }

        [Required]
        [StringLength(255)]
        public string Titulo { get; set; }

        public string Contenido { get; set; }

        [StringLength(255)]
        public string UrlVideo { get; set; }

        [Required]
        public int Orden { get; set; }
    }
}