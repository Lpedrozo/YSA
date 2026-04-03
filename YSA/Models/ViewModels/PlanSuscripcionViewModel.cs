// YSA.Web.Models.ViewModels/PlanSuscripcionViewModel.cs
namespace YSA.Web.Models.ViewModels
{
    public class PlanSuscripcionViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int DuracionDias { get; set; }
        public int LimitePublicaciones { get; set; }
        public decimal ComisionPorcentaje { get; set; }
        public bool TieneVisibilidadPrioritaria { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
        public bool PermitePromocionesExtras { get; set; }
        public int MaxPromocionesSimultaneas { get; set; }
        public decimal? DescuentoComisionAdicional { get; set; }

        // Propiedades de UI
        public string DuracionTexto => DuracionDias switch
        {
            30 => "Mensual",
            90 => "Trimestral",
            180 => "Semestral",
            365 => "Anual",
            _ => $"{DuracionDias} días"
        };

        public string PrecioFormateado => $"${Precio:F2} USD";

        public string ComisionFormateada => $"{ComisionPorcentaje:F0}%";

        public string EstadoTexto => Activo ? "Activo" : "Inactivo";
        public string EstadoBadge => Activo ? "bg-success" : "bg-secondary";
    }
}