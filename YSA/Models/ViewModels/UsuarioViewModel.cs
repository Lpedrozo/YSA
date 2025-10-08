using System.ComponentModel.DataAnnotations;
using YSA.Core.Entities;
namespace YSA.Web.Models.ViewModels
{
    public class UserViewModel
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Email { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña Actual")]
        public string? CurrentPassword { get; set; }

        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y un máximo de {1} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nueva Contraseña")]
        [Compare("NewPassword", ErrorMessage = "La nueva contraseña y la de confirmación no coinciden.")]
        public string? ConfirmPassword { get; set; }
        public ICollection<CursoViewModel> CursosComprados { get; set; } = new List<CursoViewModel>();
        public string? UrlImagen { get; set; }
        public IFormFile? File { get; set; }

        public IEnumerable<ProductoViewModel> ProductosComprados { get; set; } // <--- Nueva propiedad

    }
    public class PerfilViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required(ErrorMessage = "El Nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El {0} debe tener al menos {2} y un máximo de {1} caracteres de longitud.", MinimumLength = 2)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El Apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El {0} debe tener al menos {2} y un máximo de {1} caracteres de longitud.", MinimumLength = 2)]
        public string Apellido { get; set; }
    }
    public class EstudianteConCursosViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public List<string> CursosComprados { get; set; }
    }
    public class ArtistaViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "El nombre artístico es obligatorio.")]
        public string NombreArtistico { get; set; }

        public string Biografia { get; set; }

        [Required(ErrorMessage = "El estilo principal es obligatorio.")]
        public string EstiloPrincipal { get; set; }

        public IFormFile? ImagenPerfil { get; set; }
    }
    public class ArtistaAdminViewModel
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "El nombre artístico es obligatorio.")]
        public string NombreArtistico { get; set; }

        public string Biografia { get; set; }

        [Required(ErrorMessage = "El estilo principal es obligatorio.")]
        public string EstiloPrincipal { get; set; }

        public IFormFile? ImagenPerfil { get; set; }
        public string? UrlImagenExistente { get; set; }
    }
    public class ArtistaListViewModel
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string NombreArtistico { get; set; }
        public string EstiloPrincipal { get; set; }
        public string? UrlImagen { get; set; }
        public string? Biografia { get; set; }
    }
    public class PortafolioViewModel
    {
        public int ArtistaId { get; set; }
        public List<ArtistaFoto> Fotos { get; set; }
    }
    public class ArtistaDetallesViewModel
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Biografia { get; set; }
        public string? UrlFotoPerfil { get; set; }
        public IEnumerable<ArtistaFoto> Portafolio { get; set; } = new List<ArtistaFoto>();
        public IEnumerable<Curso> Cursos { get; set; } = new List<Curso>();
        // Si tienes cursos, los agregas aquí
    }
}