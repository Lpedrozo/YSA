using System.ComponentModel.DataAnnotations;
using YSA.Core.Entities;

namespace YSA.Web.Models.ViewModels
{
    public class EventoHomeViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string TipoEventoNombre { get; set; }
        public string UrlImagen { get; set; }
        public DateTime FechaEvento { get; set; }
        public string Lugar { get; set; }
        public string Descripcion { get; set; }
    }
    public class ClaseHomeViewModel
    {
        public int Id { get; set; }
        public int CursoId { get; set; }
        public string CursoTitulo { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public string Lugar { get; set; }
        public int CapacidadMaxima { get; set; }
        public int VacantesDisponibles { get; set; }
        public string UrlImagen { get; set; }
        public decimal Precio { get; set; }
    }

    public class HomeViewModel
    {
        public Dictionary<string, List<EventoHomeViewModel>> EventosCategorizados { get; set; } = new Dictionary<string, List<EventoHomeViewModel>>();
        public IEnumerable<Artista> Artistas { get; set; }
        public List<ClaseHomeViewModel> ClasesDisponibles { get; set; } = new List<ClaseHomeViewModel>(); // Nueva propiedad
    }
}
