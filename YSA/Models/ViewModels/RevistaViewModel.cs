using System.ComponentModel.DataAnnotations;
using YSA.Core.Entities;
namespace YSA.Web.Models.ViewModels
{
    public class RevistaViewModel
    {
        public string TituloRevista { get; set; }
        public string Descripcion { get; set; }
        public List<ArtistaRevistaViewModel> ArtistasDestacados { get; set; }
    }

    public class ArtistaRevistaViewModel
    {
        public string NombreArtistico { get; set; }
        public string Biografia { get; set; }
        public string EstiloPrincipal { get; set; }
        public string UrlImagenPerfil { get; set; } // URL de la imagen que quieres mostrar
    }
    public class RevistaIndexViewModel
    {
        public IEnumerable<Articulo> ArticulosDestacados { get; set; } = new List<Articulo>();

        public IEnumerable<Articulo> ArticulosRecientes { get; set; } = new List<Articulo>();

    }
}