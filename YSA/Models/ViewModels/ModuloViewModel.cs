using System.ComponentModel.DataAnnotations;

namespace YSA.Web.Models.ViewModels
{
    public class ModuloViewModel
    {
        public int Id { get; set; }
        public int CursoId { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(255)]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "El orden es obligatorio.")]
        public int Orden { get; set; }
    }
    public class ModuloConLeccionesViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public List<LeccionViewModel> Lecciones { get; set; }
    }
}