using Microsoft.AspNetCore.Identity;

namespace YSA.Core.Entities
{
    public class Usuario : IdentityUser<int>
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateTime FechaCreacion { get; set; }

        public virtual Artista Artista { get; set; }
        public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; }
        public virtual ICollection<Pedido> Pedidos { get; set; }
    }
}