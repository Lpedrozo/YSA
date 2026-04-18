using YSA.Core.Entities;

namespace YSA.Data.Repositories
{
    public interface IPaqueteRepository
    {
        // CRUD Básico
        Task<Paquete> GetByIdAsync(int id);
        Task<List<Paquete>> GetAllAsync();
        Task<Paquete> GetByIdWithDetailsAsync(int id);
        Task<List<Paquete>> GetAllWithDetailsAsync();
        Task AddAsync(Paquete paquete);
        Task UpdateAsync(Paquete paquete);
        Task DeleteAsync(int id);
        Task SaveChangesAsync();
        Task<bool> ExisteAsync(int id);
        // Relaciones
        Task AddCursoToPaqueteAsync(int paqueteId, int cursoId);
        Task AddProductoToPaqueteAsync(int paqueteId, int productoId);
        Task RemoveAllCursosFromPaqueteAsync(int paqueteId);
        Task RemoveAllProductosFromPaqueteAsync(int paqueteId);
        Task<List<Curso>> GetCursosNoAsociadosAsync(int paqueteId);
        Task<List<Producto>> GetProductosNoAsociadosAsync(int paqueteId);

        // Genéricos para obtener listas
        Task<List<Curso>> GetAllCursosAsync();
        Task<List<Producto>> GetAllProductosAsync();
    }
}