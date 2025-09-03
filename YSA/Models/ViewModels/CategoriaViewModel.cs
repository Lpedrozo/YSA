using System.ComponentModel.DataAnnotations;

namespace YSA.Web.Models.ViewModels
{
    public class CategoriaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(255, ErrorMessage = "El nombre no puede exceder los 255 caracteres.")]
        [Display(Name = "Nombre de la Categoría")]
        public string NombreCategoria { get; set; }
    }
}