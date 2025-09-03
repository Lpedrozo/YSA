using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class UsuarioRol : IdentityUserRole<int>
    {

        public virtual Usuario Usuario { get; set; }

        public virtual Rol Rol { get; set; }
    }
}