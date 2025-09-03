using YSA.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Core.Services
{
    public interface ILeccionService
    {
        Task<IEnumerable<Leccion>> ObtenerLeccionesPorModuloIdAsync(int moduloId);
        Task<Leccion> ObtenerLeccionPorIdAsync(int id);
        Task<bool> CrearLeccionAsync(Leccion leccion);
        Task<bool> ActualizarLeccionAsync(Leccion leccion);
        Task<bool> EliminarLeccionAsync(int id);
    }
}