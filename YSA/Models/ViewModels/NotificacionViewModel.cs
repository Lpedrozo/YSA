using System.ComponentModel.DataAnnotations;

namespace YSA.Web.Models.ViewModels
{
    public class NotificacionViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public string UrlDestino { get; set; }
        public string Icono { get; set; }
        public string Color { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool EsLeida { get; set; }
        public string TipoNotificacion { get; set; }
    }

    public class CrearNotificacionViewModel
    {
        public int UsuarioId { get; set; }
        public int TipoNotificacionId { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public string UrlDestino { get; set; }
        public int? EntidadId { get; set; }
        public string TipoEntidad { get; set; }
    }
}