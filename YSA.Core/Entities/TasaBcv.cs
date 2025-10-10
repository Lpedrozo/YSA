using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace YSA.Core.Entities
{
    [Table("TasaBCV")]
    public class TasaBCV
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 4)")] 
        public decimal Valor { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Today;

        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}