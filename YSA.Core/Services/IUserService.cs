using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using YSA.Core.Entities;

namespace YSA.Core.Services
{
    public interface IUsuarioService
    {
        Task<Usuario> GetUsuarioByIdAsync(string id);
        Task<IdentityResult> UpdateUsuarioAsync(Usuario usuario);
        Task<IdentityResult> ChangePasswordAsync(Usuario usuario, string currentPassword, string newPassword);

    }
}