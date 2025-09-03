using YSA.Core.Entities;
using YSA.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Core.Services
{
    public class LeccionService : ILeccionService
    {
        private readonly ILeccionRepository _leccionRepository;

        public LeccionService(ILeccionRepository leccionRepository)
        {
            _leccionRepository = leccionRepository;
        }

        public async Task<IEnumerable<Leccion>> ObtenerLeccionesPorModuloIdAsync(int moduloId)
        {
            return await _leccionRepository.GetByModuloIdAsync(moduloId);
        }

        public async Task<Leccion> ObtenerLeccionPorIdAsync(int id)
        {
            return await _leccionRepository.GetByIdAsync(id);
        }

        public async Task<bool> CrearLeccionAsync(Leccion leccion)
        {
            await _leccionRepository.AddAsync(leccion);
            return true;
        }

        public async Task<bool> ActualizarLeccionAsync(Leccion leccion)
        {
            await _leccionRepository.UpdateAsync(leccion);
            return true;
        }

        public async Task<bool> EliminarLeccionAsync(int id)
        {
            await _leccionRepository.DeleteAsync(id);
            return true;
        }
    }
}