using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YSA.Core.Entities
{
    /// <summary>
    /// Planes de suscripción para artistas
    /// </summary>
    public class PlanSuscripcion
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nombre del plan (Ej: Básico, Pro, Premium)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        /// <summary>
        /// Descripción detallada del plan
        /// </summary>
        [StringLength(500)]
        public string Descripcion { get; set; }

        /// <summary>
        /// Precio del plan en USD
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        /// <summary>
        /// Duración en días (30, 90, 365, etc.)
        /// </summary>
        [Required]
        public int DuracionDias { get; set; }

        /// <summary>
        /// Límite de publicaciones permitidas en este plan
        /// (0 = ilimitado)
        /// </summary>
        [Required]
        public int LimitePublicaciones { get; set; }

        /// <summary>
        /// Porcentaje de comisión que cobra la academia por cada venta
        /// (Ej: 15 = 15%)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal ComisionPorcentaje { get; set; }

        /// <summary>
        /// Indica si el plan tiene visibilidad prioritaria en el feed
        /// </summary>
        public bool TieneVisibilidadPrioritaria { get; set; }

        /// <summary>
        /// Orden de visualización (para mostrar planes en orden específico)
        /// </summary>
        public int Orden { get; set; }

        /// <summary>
        /// Estado del plan: Activo o Inactivo
        /// </summary>
        public bool Activo { get; set; }

        /// <summary>
        /// Fecha de creación del plan
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Fecha de última modificación
        /// </summary>
        public DateTime? FechaModificacion { get; set; }

        /// <summary>
        /// Usuario que creó/modificó el plan (opcional, para auditoría)
        /// </summary>
        public int? ModificadoPorId { get; set; }
        public Usuario ModificadoPor { get; set; }

        // ========== PROPIEDADES ADICIONALES PARA FUTURA EXPANSIÓN ==========

        /// <summary>
        /// ¿Permite promociones pagadas adicionales?
        /// </summary>
        public bool PermitePromocionesExtras { get; set; }

        /// <summary>
        /// Número máximo de promociones simultáneas permitidas
        /// </summary>
        public int MaxPromocionesSimultaneas { get; set; }

        /// <summary>
        /// Descuento en comisión para este plan (porcentaje adicional)
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? DescuentoComisionAdicional { get; set; }

        // ========== RELACIONES ==========

        /// <summary>
        /// Suscripciones asociadas a este plan
        /// </summary>
        public virtual ICollection<SuscripcionArtista> Suscripciones { get; set; }
    }
}