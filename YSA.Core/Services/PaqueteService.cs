using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Repositories;

namespace YSA.Core.Services
{
    public class PaqueteService : IPaqueteService
    {
        private readonly IPaqueteRepository _paqueteRepository;

        public PaqueteService(IPaqueteRepository paqueteRepository)
        {
            _paqueteRepository = paqueteRepository;
        }

        public async Task<List<Paquete>> ObtenerTodosAsync()
        {
            return await _paqueteRepository.GetAllAsync();
        }

        public async Task<Paquete> ObtenerPorIdAsync(int id)
        {
            return await _paqueteRepository.GetByIdAsync(id);
        }
        public async Task<Paquete> ActualizarSoloImagenAsync(Paquete paquete)
        {
            await _paqueteRepository.UpdateAsync(paquete);
            await _paqueteRepository.SaveChangesAsync();
            return paquete;
        }
        public async Task<Paquete> ObtenerPorIdConDetallesAsync(int id)
        {
            return await _paqueteRepository.GetByIdWithDetailsAsync(id);
        }

        public async Task<Paquete> CrearAsync(Paquete paquete, List<int> cursosIds, List<int> productosIds)
        {
            paquete.FechaPublicacion = DateTime.UtcNow;

            await _paqueteRepository.AddAsync(paquete);
            await _paqueteRepository.SaveChangesAsync();

            if (cursosIds != null && cursosIds.Any())
            {
                foreach (var cursoId in cursosIds)
                {
                    await _paqueteRepository.AddCursoToPaqueteAsync(paquete.Id, cursoId);
                }
            }

            if (productosIds != null && productosIds.Any())
            {
                foreach (var productoId in productosIds)
                {
                    await _paqueteRepository.AddProductoToPaqueteAsync(paquete.Id, productoId);
                }
            }

            await _paqueteRepository.SaveChangesAsync();

            return paquete;
        }

        public async Task<Paquete> ActualizarAsync(Paquete paquete, List<int> cursosIds, List<int> productosIds)
        {
            await _paqueteRepository.RemoveAllCursosFromPaqueteAsync(paquete.Id);
            await _paqueteRepository.RemoveAllProductosFromPaqueteAsync(paquete.Id);

            if (cursosIds != null && cursosIds.Any())
            {
                foreach (var cursoId in cursosIds)
                {
                    await _paqueteRepository.AddCursoToPaqueteAsync(paquete.Id, cursoId);
                }
            }

            if (productosIds != null && productosIds.Any())
            {
                foreach (var productoId in productosIds)
                {
                    await _paqueteRepository.AddProductoToPaqueteAsync(paquete.Id, productoId);
                }
            }

            await _paqueteRepository.UpdateAsync(paquete);
            await _paqueteRepository.SaveChangesAsync();

            return paquete;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            if (!await _paqueteRepository.ExisteAsync(id))
                return false;

            await _paqueteRepository.DeleteAsync(id);
            await _paqueteRepository.SaveChangesAsync();
            return true;
        }

        public async Task<List<Curso>> ObtenerCursosDisponiblesAsync()
        {
            return await _paqueteRepository.GetAllCursosAsync();
        }

        public async Task<List<Producto>> ObtenerProductosDisponiblesAsync()
        {
            return await _paqueteRepository.GetAllProductosAsync();
        }

        public async Task<List<Curso>> ObtenerTodosLosCursosAsync()
        {
            return await _paqueteRepository.GetAllCursosAsync();
        }

        public async Task<List<Producto>> ObtenerTodosLosProductosAsync()
        {
            return await _paqueteRepository.GetAllProductosAsync();
        }

        public async Task<bool> DestacarAsync(int id)
        {
            var paquete = await _paqueteRepository.GetByIdAsync(id);
            if (paquete == null) return false;

            paquete.EsDestacado = true;
            await _paqueteRepository.UpdateAsync(paquete);
            await _paqueteRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> QuitarDestacadoAsync(int id)
        {
            var paquete = await _paqueteRepository.GetByIdAsync(id);
            if (paquete == null) return false;

            paquete.EsDestacado = false;
            await _paqueteRepository.UpdateAsync(paquete);
            await _paqueteRepository.SaveChangesAsync();
            return true;
        }
        public async Task<List<Paquete>> ObtenerTodosConDetallesAsync()
        {
            return await _paqueteRepository.GetAllWithDetailsAsync();
        }
        public async Task<bool> RecomendarAsync(int id)
        {
            var paquete = await _paqueteRepository.GetByIdAsync(id);
            if (paquete == null) return false;

            paquete.EsRecomendado = true;
            await _paqueteRepository.UpdateAsync(paquete);
            await _paqueteRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> QuitarRecomendadoAsync(int id)
        {
            var paquete = await _paqueteRepository.GetByIdAsync(id);
            if (paquete == null) return false;

            paquete.EsRecomendado = false;
            await _paqueteRepository.UpdateAsync(paquete);
            await _paqueteRepository.SaveChangesAsync();
            return true;
        }
    }
}