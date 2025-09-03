using YSA.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Core.Services
{
    public interface IModuloService
    {
        Task<IEnumerable<Modulo>> ObtenerModulosPorCursoIdAsync(int cursoId);
        Task<Modulo> ObtenerModuloPorIdAsync(int id);
        Task<bool> CrearModuloAsync(Modulo modulo);
        Task<bool> ActualizarModuloAsync(Modulo modulo);
        Task<bool> EliminarModuloAsync(int id);
    }
}