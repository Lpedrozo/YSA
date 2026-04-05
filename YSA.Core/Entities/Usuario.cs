using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace YSA.Core.Entities
{
    public class Usuario : IdentityUser<int>
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateTime FechaCreacion { get; set; }

        [StringLength(255)]
        public string UrlImagen { get; set; }

        // NUEVOS CAMPOS PARA EL FORMULARIO DE INSCRIPCIÓN
        [StringLength(20)]
        public string? Cedula { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [StringLength(20)]
        public string? WhatsApp { get; set; }

        [StringLength(500)]
        public string? ExperienciaTatuaje { get; set; } // Para el curso de tatuajes

        // Datos del representante (para menores de edad)
        public bool EsMenorEdad { get; set; }

        [StringLength(255)]
        public string? NombreRepresentante { get; set; }

        [StringLength(20)]
        public string? CedulaRepresentante { get; set; }

        // Quién atendió al estudiante
        [StringLength(100)]
        public string? AtendidoPor { get; set; }

        // Relaciones existentes
        public virtual Artista Artista { get; set; }
        public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; }
        public virtual ICollection<Pedido> Pedidos { get; set; }
        public ICollection<MetodoPago> MetodosPago { get; set; }
        public ICollection<PreguntaRespuesta> PreguntasRespuestas { get; set; }
        public ICollection<Resena> Resenas { get; set; }

        // Nueva relación: Inscripciones a clases presenciales
        public virtual ICollection<InscripcionClase> InscripcionesClases { get; set; } = new List<InscripcionClase>();
    }
}