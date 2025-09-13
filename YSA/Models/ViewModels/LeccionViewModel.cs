using System.ComponentModel.DataAnnotations;

namespace YSA.Web.Models.ViewModels
{
    public class LeccionViewModel
    {
        public int Id { get; set; }
        public int ModuloId { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(255)]
        public string Titulo { get; set; }

        public string Contenido { get; set; }

        [StringLength(255)]
        public string UrlVideo { get; set; }

        [Required(ErrorMessage = "El orden es obligatorio.")]
        public int Orden { get; set; }
        public bool CompletadaPorEstudiante { get; set; } // Nuevo campo

    }
}