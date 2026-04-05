namespace YSA.Core.DTOs
{
    public class EstudianteDetalleDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Cedula { get; set; }
        public string WhatsApp { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public bool EsMenorEdad { get; set; }
        public string NombreRepresentante { get; set; }
        public string CedulaRepresentante { get; set; }
        public string ExperienciaTatuaje { get; set; }
        public string AtendidoPor { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UrlImagen { get; set; }
        public List<string> CursosComprados { get; set; }
        public List<string> ClasesInscritas { get; set; }
        public List<PedidoResumenDto> Pedidos { get; set; }
    }

    public class PedidoResumenDto
    {
        public int Id { get; set; }
        public DateTime FechaPedido { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
        public string MetodoPago { get; set; }
        public string ReferenciaPago { get; set; }
        public DateTime? FechaPago { get; set; }
    }
}