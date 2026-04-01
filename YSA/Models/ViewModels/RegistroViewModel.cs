using System.ComponentModel.DataAnnotations;

namespace YSA.Web.Models.ViewModels
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100)]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [StringLength(255)]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; }

        [DataType(DataType.Password)]
        [Compare("Contrasena", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
        public string ConfirmarContrasena { get; set; }
        public string ReturnUrl { get; set; }
        [Required(ErrorMessage = "Debes seleccionar un tipo de cuenta")]
        [Display(Name = "Tipo de cuenta")]
        public string TipoCuenta { get; set; } // "Estudiante" o "Artista"

        [StringLength(255)]
        [Display(Name = "Nombre artístico")]
        public string? NombreArtistico { get; set; }

        [Display(Name = "Biografía")]
        public string? Biografia { get; set; }

        [StringLength(255)]
        [Display(Name = "Estilo principal")]
        public string? EstiloPrincipal { get; set; }
    }
}