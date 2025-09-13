using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class MetodoPago
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EstudianteId { get; set; }
        [ForeignKey("EstudianteId")]
        public Usuario Estudiante { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoMetodo { get; set; }

        [StringLength(4)]
        public string UltimosCuatroDigitos { get; set; }

        [Required]
        public string TokenPago { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}