using System.Collections.Generic;
using System.Threading.Tasks;
using YSA.Core.Entities;

namespace YSA.Core.Interfaces
{
    public interface IEventoService
    {
        Task<Evento> GetEventoByIdAsync(int id);
        Task<IEnumerable<Evento>> GetEventosAsync();
        Task AddEventoAsync(Evento evento);
        Task UpdateEventoAsync(Evento evento);
        Task DeleteEventoAsync(int id);
        Task<IEnumerable<TipoEvento>> GetTiposEventosAsync();
        Task AddEventoFotoAsync(EventoFotos foto);
        Task<IEnumerable<Evento>> GetEventosByTipoIdAsync(int tipoEventoId);
    }
}