using YSA.Core.Entities;

namespace YSA.Core.Interfaces
{
    public interface IPaqueteService
    {
        Task<List<Paquete>> ObtenerTodosAsync();
        Task<Paquete> ObtenerPorIdAsync(int id);
        Task<Paquete> ObtenerPorIdConDetallesAsync(int id);
        Task<Paquete> CrearAsync(Paquete paquete, List<int> cursosIds, List<int> productosIds);
        Task<Paquete> ActualizarAsync(Paquete paquete, List<int> cursosIds, List<int> productosIds);
        Task<bool> EliminarAsync(int id);
        Task<Paquete> ActualizarSoloImagenAsync(Paquete paquete);
        Task<List<Curso>> ObtenerCursosDisponiblesAsync();
        Task<List<Producto>> ObtenerProductosDisponiblesAsync();
        Task<List<Curso>> ObtenerTodosLosCursosAsync();
        Task<List<Producto>> ObtenerTodosLosProductosAsync();
        Task<bool> DestacarAsync(int id);
        Task<bool> QuitarDestacadoAsync(int id);
        Task<bool> RecomendarAsync(int id);
        Task<bool> QuitarRecomendadoAsync(int id);
        Task<List<Paquete>> ObtenerTodosConDetallesAsync();
    }
}