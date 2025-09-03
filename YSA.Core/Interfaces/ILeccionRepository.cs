using YSA.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Core.Interfaces
{
    public interface ILeccionRepository
    {
        Task<IEnumerable<Leccion>> GetByModuloIdAsync(int moduloId);
        Task<Leccion> GetByIdAsync(int id);
        Task AddAsync(Leccion leccion);
        Task UpdateAsync(Leccion leccion);
        Task DeleteAsync(int id);
    }
}