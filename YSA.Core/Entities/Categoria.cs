using System.ComponentModel.DataAnnotations;

namespace YSA.Core.Entities
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string NombreCategoria { get; set; }

        // Relaciones de navegación
        public virtual ICollection<ProductoCategoria> ProductoCategorias { get; set; }
        public virtual ICollection<CursoCategoria> CursoCategorias { get; set; }
    }
}