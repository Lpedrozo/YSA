using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class ArtistaFoto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string UrlImagen { get; set; }

        [StringLength(255)]
        public string Titulo { get; set; } 

        public int ArtistaId { get; set; } 
        public Artista Artista { get; set; } 
    }
}