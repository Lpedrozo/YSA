using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using YSA.Core.Entities;

namespace YSA.Web.Models.ViewModels
{
    public class ArticuloViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El Título es obligatorio.")]
        [StringLength(255)]
        public string Titulo { get; set; }

        [StringLength(500)]
        public string Resumen { get; set; }

        [Required(ErrorMessage = "El Contenido es obligatorio.")]
        public string ContenidoTexto { get; set; }

        [StringLength(50)]
        public string Categoria { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } = "Borrador"; // Por ejemplo: Borrador, Publicado

        // --- Datos de la Persona Destacada ---
        [Required(ErrorMessage = "El Nombre de la persona destacada es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre del Destacado")]
        public string NombrePersonaDestacada { get; set; }

        [Display(Name = "Biografía Corta")]
        public string BiografiaCortaDestacado { get; set; }

        // Campo para mostrar la URL actual (en edición)
        public string? UrlFotoDestacado { get; set; }

        // Campo para la subida del nuevo archivo
        [Display(Name = "Foto del Destacado")]
        public IFormFile? FotoDestacadoFile { get; set; }

        // --- Imagen Principal del Artículo ---
        // Campo para mostrar la URL actual (en edición)
        public string? UrlImagenPrincipal { get; set; }

        // Campo para la subida del nuevo archivo
        [Display(Name = "Imagen Principal del Artículo")]
        public IFormFile? ImagenPrincipalFile { get; set; }

        // --- Fotos de Contenido (Portafolio, etc.) ---
        [Display(Name = "Fotos de Contenido Adicionales")]
        public List<IFormFile>? FotosContenidoFiles { get; set; }

        // Para mostrar y gestionar las fotos ya existentes al editar
        public List<ArticuloFoto> FotosExistentes { get; set; } = new List<ArticuloFoto>();
    }
    public class ArticuloDetalleViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Resumen { get; set; } = string.Empty;
        public string ContenidoTexto { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public DateTime FechaPublicacion { get; set; }
        public string UrlImagenPrincipal { get; set; } = string.Empty;

        // Información del autor/persona destacada (UrlFotoDestacado se usa aquí como foto del entrevistado)
        public string? NombrePersonaDestacada { get; set; }
        public string? BiografiaCortaDestacado { get; set; }
        public string? UrlFotoDestacado { get; set; }

        // Fotos adicionales
        public List<ArticuloFoto> FotosContenido { get; set; } = new List<ArticuloFoto>();
    }
}
