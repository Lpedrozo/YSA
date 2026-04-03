// YSA.Core.DTOs/SuscripcionArtistaDto.cs
namespace YSA.Core.DTOs
{
    public class SuscripcionArtistaDto
    {
        public int Id { get; set; }
        public int ArtistaId { get; set; }
        public string ArtistaNombre { get; set; }
        public string ArtistaNombreArtistico { get; set; }
        public string ArtistaEmail { get; set; }
        public string ArtistaUrlImagen { get; set; }
        public int PlanId { get; set; }
        public string PlanNombre { get; set; }

        // Snapshot del plan al momento de compra
        public string SnapshotNombre { get; set; }
        public decimal SnapshotPrecio { get; set; }
        public int SnapshotDuracionDias { get; set; }
        public int SnapshotLimitePublicaciones { get; set; }
        public decimal SnapshotComisionPorcentaje { get; set; }
        public bool SnapshotTieneVisibilidadPrioritaria { get; set; }

        // Datos de la suscripción
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; }
        public string ComprobanteUrl { get; set; }
        public DateTime? FechaPago { get; set; }
        public int? ValidadoPorId { get; set; }
        public string ValidadoPorNombre { get; set; }
        public DateTime? FechaValidacion { get; set; }
        public string NotasAdmin { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int PublicacionesUsadas { get; set; }

        // Propiedades calculadas
        public string PrecioFormateado => $"${SnapshotPrecio:F2} USD";
        public string DuracionTexto => SnapshotDuracionDias switch
        {
            30 => "Mensual",
            90 => "Trimestral",
            180 => "Semestral",
            365 => "Anual",
            _ => $"{SnapshotDuracionDias} días"
        };
        public string EstadoBadge => Estado switch
        {
            "Activa" => "bg-success",
            "PendientePago" => "bg-warning text-dark",
            "PagadoValidacion" => "bg-info",
            "Vencida" => "bg-secondary",
            "Cancelada" => "bg-danger",
            _ => "bg-secondary"
        };
        public string EstadoTexto => Estado switch
        {
            "Activa" => "Activa",
            "PendientePago" => "Pendiente de pago",
            "PagadoValidacion" => "Pago en validación",
            "Vencida" => "Vencida",
            "Cancelada" => "Cancelada",
            _ => Estado
        };
        public bool EsPendienteValidacion => Estado == "PagadoValidacion";
        public bool EstaActiva => Estado == "Activa";
    }
}