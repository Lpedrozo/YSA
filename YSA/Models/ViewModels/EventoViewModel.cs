using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using YSA.Core.Entities;
namespace YSA.Web.Models.ViewModels
{
    public class EventoViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [Display(Name = "Fecha del evento")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime FechaEvento { get; set; }

        [Display(Name = "Lugar")]
        public string Lugar { get; set; }

        [Display(Name = "Tipo de evento")]
        public string TipoEvento { get; set; }

        public string Plataforma { get; set; }

        [Display(Name = "Activo")]
        public bool EstaActivo { get; set; }
        public string UrlImagen { get; set; }
    }
    public class CrearEventoViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(255, ErrorMessage = "El título no puede exceder los 255 caracteres.")]
        [Display(Name = "Título del evento")]
        public string Titulo { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha y hora del evento son obligatorias.")]
        [Display(Name = "Fecha y hora del evento")]
        [DataType(DataType.DateTime)]
        public DateTime FechaEvento { get; set; }

        [Required(ErrorMessage = "El lugar es obligatorio.")]
        [StringLength(255, ErrorMessage = "El lugar no puede exceder los 255 caracteres.")]
        [Display(Name = "Lugar")]
        public string Lugar { get; set; }

        [Display(Name = "Imagen de portada")]
        public IFormFile? ImagenPortada { get; set; }

        [Display(Name = "Destacar en la página principal")]
        public bool EsDestacado { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo de evento.")]
        [Display(Name = "Tipo de evento")]
        public int TipoEventoId { get; set; }
        public string? UrlImagenExistente { get; set; }

        public IEnumerable<SelectListItem>? TiposEventoDisponibles { get; set; }
    }
    public class SubirEventoFotosViewModel
    {
        public int EventoId { get; set; }

        [Required(ErrorMessage = "Debes seleccionar un archivo de imagen.")]
        [Display(Name = "Seleccionar imagen")]
        public IFormFile Foto { get; set; }
    }
    public class EventoDetalleViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } // Contendrá el HTML enriquecido

        [Display(Name = "Fecha y hora")]
        [DataType(DataType.DateTime)]
        public DateTime FechaEvento { get; set; }

        [Display(Name = "Lugar / enlace")]
        public string Lugar { get; set; }

        [Display(Name = "Imagen de portada")]
        public string UrlImagen { get; set; }

        [Display(Name = "Destacado")]
        public bool EsDestacado { get; set; }

        [Display(Name = "Tipo de Evento")]
        public string TipoEventoNombre { get; set; }

        [Display(Name = "Plataforma")]
        public string TipoEventoPlataforma { get; set; }
    }
}