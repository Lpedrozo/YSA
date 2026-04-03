using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YSA.Core.Entities
{
    /// <summary>
    /// Registro de suscripciones de artistas a planes
    /// </summary>
    public class SuscripcionArtista
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Artista suscrito
        /// </summary>
        [Required]
        public int ArtistaId { get; set; }
        public Artista Artista { get; set; }

        /// <summary>
        /// Plan adquirido (referencia al plan actual, puede cambiar con el tiempo)
        /// </summary>
        [Required]
        public int PlanId { get; set; }
        public PlanSuscripcion Plan { get; set; }

        // ========== SNAPSHOT DE CONDICIONES AL MOMENTO DE COMPRA ==========
        // Estos campos NO cambian aunque el plan se modifique después

        /// <summary>
        /// Nombre del plan en el momento de la compra (snapshot)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string SnapshotNombre { get; set; }

        /// <summary>
        /// Precio pagado en USD (snapshot)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal SnapshotPrecio { get; set; }

        /// <summary>
        /// Duración en días en el momento de la compra (snapshot)
        /// </summary>
        [Required]
        public int SnapshotDuracionDias { get; set; }

        /// <summary>
        /// Límite de publicaciones en el momento de la compra (snapshot)
        /// </summary>
        [Required]
        public int SnapshotLimitePublicaciones { get; set; }

        /// <summary>
        /// Comisión aplicable en el momento de la compra (snapshot)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal SnapshotComisionPorcentaje { get; set; }

        /// <summary>
        /// Visibilidad prioritaria en el momento de la compra (snapshot)
        /// </summary>
        [Required]
        public bool SnapshotTieneVisibilidadPrioritaria { get; set; }

        // ========== DATOS DE LA SUSCRIPCIÓN ==========

        /// <summary>
        /// Fecha de inicio de la suscripción
        /// </summary>
        [Required]
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Fecha de fin de la suscripción
        /// </summary>
        [Required]
        public DateTime FechaFin { get; set; }

        /// <summary>
        /// Estado de la suscripción:
        /// - PendientePago: Esperando comprobante
        /// - PagadoValidacion: Pago subido, pendiente validación admin
        /// - Activa: Pago validado y suscripción activa
        /// - Vencida: Suscripción expirada
        /// - Cancelada: Cancelada por admin
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Estado { get; set; }

        /// <summary>
        /// URL del comprobante de pago subido por el artista
        /// </summary>
        [StringLength(500)]
        public string ComprobanteUrl { get; set; }

        /// <summary>
        /// Fecha en que se realizó el pago (cuando se validó)
        /// </summary>
        public DateTime? FechaPago { get; set; }

        /// <summary>
        /// Administrador que validó el pago
        /// </summary>
        public int? ValidadoPorId { get; set; }
        public Usuario ValidadoPor { get; set; }

        /// <summary>
        /// Fecha en que fue validado
        /// </summary>
        public DateTime? FechaValidacion { get; set; }

        /// <summary>
        /// Notas adicionales del admin (ej: motivo de rechazo)
        /// </summary>
        [StringLength(500)]
        public string NotasAdmin { get; set; }

        /// <summary>
        /// Fecha de creación del registro
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Número de publicaciones usadas durante esta suscripción
        /// </summary>
        public int PublicacionesUsadas { get; set; }
    }
}