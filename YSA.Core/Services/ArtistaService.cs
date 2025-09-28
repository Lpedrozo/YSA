using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using YSA.Core.DTOs;
using YSA.Core.Entities;
using YSA.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace YSA.Core.Services
{
    public class ArtistaService : IArtistaService
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IArtistaRepository _artistaRepository;
        IArtistaFotoRepository _artistaFotoRepository;

        public ArtistaService(UserManager<Usuario> userManager, IArtistaRepository artistaRepository, IArtistaFotoRepository artistaFotoRepository)
        {
            _userManager = userManager;
            _artistaRepository = artistaRepository;
            _artistaFotoRepository = artistaFotoRepository;
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

                var resultadoRol = await _userManager.AddToRoleAsync(usuario, "ARTISTA");

                if (!resultadoRol.Succeeded)
                {
                    // Si falla la asignación del rol, se debe eliminar el usuario para evitar inconsistencias.
                    await _userManager.DeleteAsync(usuario);
                    var errores = string.Join(", ", resultadoRol.Errors.Select(e => e.Description));
                    return (false, $"Usuario creado, pero falló la asignación del rol ARTISTA: {errores}");
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
        public async Task<List<ArtistaFoto>> ObtenerFotosPortafolioAsync(int artistaId)
        {
            return await _artistaFotoRepository.GetByArtistaIdAsync(artistaId);
        }

        public async Task<ArtistaFoto> ObtenerFotoPorIdAsync(int fotoId)
        {
            return await _artistaFotoRepository.GetByIdAsync(fotoId); // Corregido: usa el repositorio de fotos
        }

        public async Task AgregarFotoPortafolioAsync(int artistaId, Stream fileStream, string fileName, string titulo)
        {
            var urlImagen = Path.Combine("/Artista", artistaId.ToString(), fileName).Replace("\\", "/");

            var foto = new ArtistaFoto
            {
                ArtistaId = artistaId,
                UrlImagen = urlImagen,
                Titulo = titulo
            };

            await _artistaFotoRepository.AddAsync(foto);
            await _artistaFotoRepository.SaveChangesAsync();
        }


        public async Task<string> EliminarFotoPortafolioAsync(int fotoId)
        {
            var foto = await _artistaFotoRepository.GetByIdAsync(fotoId);
            if (foto == null)
            {
                throw new InvalidOperationException("La foto no fue encontrada.");
            }

            // Devuelve la URL para que el controlador la use para borrar el archivo
            await _artistaFotoRepository.DeleteAsync(foto);
            await _artistaFotoRepository.SaveChangesAsync();

            return foto.UrlImagen;
        }

        // Este método estaba bien, solo asegúrate de que exista en el repositorio.
        public async Task<Artista> ObtenerArtistaPorUsuarioIdAsync(string userId)
        {
            return await _artistaRepository.GetByUsuarioIdAsync(userId);
        }
        public async Task<(Artista Artista, List<ArtistaFoto> Fotos)> ObtenerArtistaYPortafolioAsync(int artistaId)
        {
            var artista = await _artistaRepository.GetByIdAsync(artistaId);

            if (artista == null)
            {
                return (null, null);
            }

            var fotos = await _artistaFotoRepository.GetByArtistaIdAsync(artistaId);

            return (artista, fotos);
        }
    }
}