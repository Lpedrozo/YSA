using System.Collections.Generic;
using System.Threading.Tasks;
using YSA.Core.DTOs;
using YSA.Core.Entities;

namespace YSA.Core.Services
{
    public interface IArtistaService
    {
        Task<(bool success, string message)> CrearArtistaAsync(Usuario usuario, string password, Artista artista);
        Task<(bool success, string message)> UpdateArtistaAsync(Usuario usuario, string? newPassword, Artista artista, string? nuevaUrlImagen);
        Task<(bool success, string message)> DeleteArtistaAsync(int id);
        Task<List<Artista>> GetAllArtistasAsync();
        Task<Artista> GetByIdAsync(int id);
    }
}