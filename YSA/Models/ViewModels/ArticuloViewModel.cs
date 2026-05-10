using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using YSA.Core.Entities;

namespace YSA.Web.Models.ViewModels
{
    public class ArticuloViewModel
    {
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public string? Resumen { get; set; }
        public string? ContenidoTexto { get; set; }
        public string? Categoria { get; set; }
        public string Estado { get; set; }

        // Persona destacada (opcional)
        public string? NombrePersonaDestacada { get; set; }
        public string? BiografiaCortaDestacado { get; set; }

        // URLs de imágenes
        public string? UrlImagenPortada { get; set; }
        public string? UrlFotoDestacado { get; set; }
        public string? UrlImagenPrincipal { get; set; }

        // Archivos para subir
        public IFormFile? ImagenPortadaFile { get; set; }
        public List<IFormFile>? FotosGaleriaFiles { get; set; }

        // Para mostrar en la vista
        public int CantidadFotos { get; set; }
        public List<ArticuloFoto>? FotosExistentes { get; set; }

        // Campos obsoletos (mantener por compatibilidad)
        public IFormFile? FotoDestacadoFile { get; set; }
        public IFormFile? ImagenPrincipalFile { get; set; }
        public List<IFormFile>? FotosContenidoFiles { get; set; }
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
