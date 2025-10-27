using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using YSA.Core.Entities;
using YSA.Core.Interfaces;

namespace YSA.Core.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly UserManager<Usuario> _userManager;

        public UsuarioService(IUsuarioRepository usuarioRepository, UserManager<Usuario> userManager)
        {
            _usuarioRepository = usuarioRepository;
            _userManager = userManager;
        }
        public async Task<int> GetTotalEstudiantesAsync()
        {
            return await _usuarioRepository.GetTotalEstudiantesAsync();
        }
        public async Task<Usuario> GetUsuarioByIdAsync(string id)
        {
            return await _usuarioRepository.GetByIdAsync(id);
        }

        public async Task<IdentityResult> UpdateUsuarioAsync(Usuario usuario)
        {
            return await _userManager.UpdateAsync(usuario);
        }
        public async Task<IdentityResult> ChangePasswordAsync(Usuario usuario, string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(usuario, currentPassword, newPassword);
        }
    }
}