using System.ComponentModel.DataAnnotations;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class ProductoCategoria
    {
        [Key]
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }

        [Key]
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
    }
}