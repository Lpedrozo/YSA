using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using YSA.Core.DTOs;
using YSA.Core.Entities;
using YSA.Core.Interfaces;

namespace YSA.Core.Services
{
    public class ArtistaService : IArtistaService
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IArtistaRepository _artistaRepository;

        public ArtistaService(UserManager<Usuario> userManager, IArtistaRepository artistaRepository)
        {
            _userManager = userManager;
            _artistaRepository = artistaRepository;
        }

        public async Task<(bool success, string message)> CrearArtistaAsync(Usuario usuario, string password, Artista artista)
        {
            try
            {
                // 1. Crear el usuario en la base de datos con su password.
                var resultado = await _userManager.CreateAsync(usuario, password);
                if (!resultado.Succeeded)
                {
                    var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                    return (false, $"Error al crear el usuario: {errores}");
                }

                // 2. Vincular el artista con el usuario recién creado.
                artista.UsuarioId = usuario.Id;
                await _artistaRepository.AddAsync(artista);

                return (true, "Artista creado con éxito.");
            }
            catch (Exception ex)
            {
                // Devolver un mensaje de error genérico para evitar exponer detalles.
                return (false, "Ocurrió un error al crear el artista.");
            }
        }

        public async Task<(bool success, string message)> UpdateArtistaAsync(Usuario usuario, string? newPassword, Artista artista, string? nuevaUrlImagen)
        {
            if (!string.IsNullOrEmpty(newPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                var resultadoCambio = await _userManager.ResetPasswordAsync(usuario, token, newPassword);
                if (!resultadoCambio.Succeeded)
                {
                    var errores = string.Join(", ", resultadoCambio.Errors.Select(e => e.Description));
                    return (false, $"Error al cambiar la contraseña: {errores}");
                }
            }

            // Asignar la nueva URL de la imagen que el controlador ya procesó
            usuario.UrlImagen = nuevaUrlImagen ?? usuario.UrlImagen;
            await _userManager.UpdateAsync(usuario);

            await _artistaRepository.UpdateAsync(artista);

            return (true, "Artista actualizado con éxito.");
        }

        public async Task<(bool success, string message)> DeleteArtistaAsync(int id)
        {
            var artista = await _artistaRepository.GetByIdAsync(id);
            if (artista == null)
            {
                return (false, "Artista no encontrado.");
            }

            var usuario = artista.Usuario;
            if (usuario == null)
            {
                return (false, "Usuario asociado no encontrado.");
            }

            // La eliminación del archivo ya no es responsabilidad del servicio
            // Se hace en el controlador antes de llamar a este método.

            await _artistaRepository.DeleteAsync(artista);
            await _userManager.DeleteAsync(usuario);

            return (true, "Artista eliminado con éxito.");
        }

        public async Task<List<Artista>> GetAllArtistasAsync()
        {
            return await _artistaRepository.GetAllAsync();
        }
        public async Task<Artista> GetByIdAsync(int id)
        {
            return await _artistaRepository.GetByIdAsync(id);
        }
    }
}