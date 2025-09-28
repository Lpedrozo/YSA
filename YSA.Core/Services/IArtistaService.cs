using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        Task<List<ArtistaFoto>> ObtenerFotosPortafolioAsync(int artistaId);
        Task<ArtistaFoto> ObtenerFotoPorIdAsync(int fotoId);
        Task AgregarFotoPortafolioAsync(int artistaId, Stream fileStream, string fileName, string titulo);
        Task<string> EliminarFotoPortafolioAsync(int fotoId);
        Task<Artista> ObtenerArtistaPorUsuarioIdAsync(string userId);
        Task<(Artista Artista, List<ArtistaFoto> Fotos)> ObtenerArtistaYPortafolioAsync(int artistaId);

    }
}