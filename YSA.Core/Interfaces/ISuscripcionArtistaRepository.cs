// YSA.Core.Interfaces/ISuscripcionArtistaRepository.cs
using YSA.Core.Entities;

namespace YSA.Core.Interfaces
{
    public interface ISuscripcionArtistaRepository
    {
        Task<List<SuscripcionArtista>> GetAllAsync();
        Task<List<SuscripcionArtista>> GetByArtistaIdAsync(int artistaId);
        Task<List<SuscripcionArtista>> GetPendientesValidacionAsync();
        Task<SuscripcionArtista> GetByIdAsync(int id);
        Task<SuscripcionArtista> AddAsync(SuscripcionArtista suscripcion);
        Task<SuscripcionArtista> UpdateAsync(SuscripcionArtista suscripcion);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsActiveSubscriptionAsync(int artistaId);
        Task<SuscripcionArtista> GetActiveSubscriptionAsync(int artistaId);
    }
}