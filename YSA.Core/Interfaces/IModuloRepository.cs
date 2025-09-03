using YSA.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Core.Interfaces
{
    public interface IModuloRepository
    {
        Task<IEnumerable<Modulo>> GetByCursoIdAsync(int cursoId);
        Task<Modulo> GetByIdAsync(int id);
        Task AddAsync(Modulo modulo);
        Task UpdateAsync(Modulo modulo);
        Task DeleteAsync(int id);
    }
}