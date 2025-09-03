using YSA.Core.Entities;
using YSA.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Core.Services
{
    public class ModuloService : IModuloService
    {
        private readonly IModuloRepository _moduloRepository;

        public ModuloService(IModuloRepository moduloRepository)
        {
            _moduloRepository = moduloRepository;
        }

        public async Task<IEnumerable<Modulo>> ObtenerModulosPorCursoIdAsync(int cursoId)
        {
            return await _moduloRepository.GetByCursoIdAsync(cursoId);
        }

        public async Task<Modulo> ObtenerModuloPorIdAsync(int id)
        {
            return await _moduloRepository.GetByIdAsync(id);
        }

        public async Task<bool> CrearModuloAsync(Modulo modulo)
        {
            await _moduloRepository.AddAsync(modulo);
            return true;
        }

        public async Task<bool> ActualizarModuloAsync(Modulo modulo)
        {
            await _moduloRepository.UpdateAsync(modulo);
            return true;
        }

        public async Task<bool> EliminarModuloAsync(int id)
        {
            await _moduloRepository.DeleteAsync(id);
            return true;
        }
    }
}