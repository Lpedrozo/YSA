using YSA.Core.Entities;

namespace YSA.Core.Interfaces
{
    public interface IArtistaRepository
    {
        Task<Artista> GetByIdAsync(int id);
        Task<List<Artista>> GetAllAsync();
        Task AddAsync(Artista artista);
        Task UpdateAsync(Artista artista);
        Task DeleteAsync(Artista artista);
        Task<Artista> GetByUsuarioIdAsync(string userId);
        Task<List<Curso>> GetCursosByArtistaAsync(int id);
    }
}