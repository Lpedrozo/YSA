using YSA.Core.Entities;

namespace YSA.Core.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<List<Categoria>> GetAllAsync();
        Task AddAsync(Categoria categoria);
        Task<Categoria> GetByIdAsync(int id);
        Task UpdateAsync(Categoria categoria);
        Task DeleteAsync(int id);
    }
}