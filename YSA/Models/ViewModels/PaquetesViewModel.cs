using YSA.Core.Entities;

namespace YSA.Web.Models.ViewModels
{
    public class PaquetesIndexViewModel
    {
        public List<Paquete> Paquetes { get; set; } = new List<Paquete>();
        public List<int> PaquetesCompradosIds { get; set; } = new List<int>();
        public List<int> PaquetesEnValidacionIds { get; set; } = new List<int>();
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public string SearchString { get; set; }
    }
    public class PaqueteDetalleViewModel
    {
        public string? UsuarioNombre { get; set; }
        public string? UsuarioApellido { get; set; }
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string DescripcionCorta { get; set; }
        public int? PedidoPendienteId { get; set; }  // Nueva propiedad
        public string DescripcionLarga { get; set; }
        public decimal Precio { get; set; }
        public string UrlImagen { get; set; }
        public decimal PrecioTotalItems { get; set; }
        public decimal Ahorro { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public bool EsDestacado { get; set; }
        public bool EsRecomendado { get; set; }

        public List<PaqueteCursoViewModel> Cursos { get; set; } = new List<PaqueteCursoViewModel>();
        public List<PaqueteProductoViewModel> Productos { get; set; } = new List<PaqueteProductoViewModel>();

        // Estados del usuario
        public bool UsuarioLogueado { get; set; }
        public bool PerfilCompleto { get; set; }
        public bool TieneAcceso { get; set; }
        public bool EstaEnValidacion { get; set; }
        public bool TienePedidoPendiente { get; set; }
    }
    public class PaqueteVerViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string DescripcionLarga { get; set; }
        public string UrlImagen { get; set; }

        public List<PaqueteCursoConAccesoViewModel> Cursos { get; set; } = new List<PaqueteCursoConAccesoViewModel>();
        public List<PaqueteProductoConAccesoViewModel> Productos { get; set; } = new List<PaqueteProductoConAccesoViewModel>();
    }

    public class PaqueteCursoConAccesoViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string DescripcionCorta { get; set; }
        public string UrlImagen { get; set; }
        public bool TieneAcceso { get; set; }
        public string UrlAcceso { get; set; }
    }

    public class PaqueteProductoConAccesoViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string DescripcionCorta { get; set; }
        public string UrlImagen { get; set; }
        public string TipoProducto { get; set; }
        public string UrlDescarga { get; set; }
    }
    public class PagoPaqueteViewModel
    {
        public int PedidoId { get; set; }
        public decimal Total { get; set; }
        public decimal TasaBCV { get; set; }
        public string MetodoPago { get; set; }
        public string ReferenciaPago { get; set; }
        public IFormFile ComprobanteArchivo { get; set; }

        // Información adicional para mostrar
        public string? PaqueteTitulo { get; set; }
        public string? PaqueteImagen { get; set; }
        public int CantidadItems { get; set; }
    }
    public class PaqueteCursoViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string DescripcionCorta { get; set; }
        public string UrlImagen { get; set; }
        public decimal Precio { get; set; }
    }

    public class PaqueteProductoViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string DescripcionCorta { get; set; }
        public string UrlImagen { get; set; }
        public decimal Precio { get; set; }
        public string TipoProducto { get; set; }
    }
}