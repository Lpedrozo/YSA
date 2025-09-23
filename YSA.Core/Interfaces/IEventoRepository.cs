using YSA.Core.Entities;

namespace YSA.Core.Interfaces
{
    public interface IEventoRepository
    {
        Task<Evento> GetByIdAsync(int id);
        Task<IEnumerable<Evento>> GetAllAsync();
        Task AddAsync(Evento evento);
        Task UpdateAsync(Evento evento);
        Task DeleteAsync(int id);
        Task<IEnumerable<TipoEvento>> GetTiposEventosAsync();
        Task AddEventoFotoAsync(EventoFotos foto);
        Task<IEnumerable<Evento>> GetEventosByTipoIdAsync(int tipoEventoId);
    }
}