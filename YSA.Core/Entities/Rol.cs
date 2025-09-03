using Microsoft.AspNetCore.Identity;

namespace YSA.Core.Entities
{
    public class Rol : IdentityRole<int>
    {
        public virtual ICollection<UsuarioRol> UsuarioRoles { get; set; }
    }
}