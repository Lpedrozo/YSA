using YSA.Core.Entities;

namespace YSA.Core.Interfaces
{
    public interface ICursoRepository
    {
        Task<List<Curso>> GetAllWithDetailsAsync();
        Task AddAsync(Curso curso);
        Task<Curso> GetByIdAsync(int id);
        Task UpdateAsync(Curso curso);
        Task DeleteAsync(int id);
        Task UpdateWithCategoriesAsync(Curso curso, int[] categoriasSeleccionadas);

    }
}