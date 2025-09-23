// En YSA.Core/Services/EventoService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using YSA.Core.Entities;
using YSA.Core.Interfaces;

namespace YSA.Core.Services
{
    public class EventoService : IEventoService
    {
        private readonly IEventoRepository _eventoRepository;

        public EventoService(IEventoRepository eventoRepository)
        {
            _eventoRepository = eventoRepository;
        }

        public async Task<Evento> GetEventoByIdAsync(int id)
        {
            return await _eventoRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Evento>> GetEventosAsync()
        {
            return await _eventoRepository.GetAllAsync();
        }

        public async Task AddEventoAsync(Evento evento)
        {
            await _eventoRepository.AddAsync(evento);
        }

        public async Task UpdateEventoAsync(Evento evento)
        {
            await _eventoRepository.UpdateAsync(evento);
        }

        public async Task DeleteEventoAsync(int id)
        {
            await _eventoRepository.DeleteAsync(id);
        }
        public async Task<IEnumerable<TipoEvento>> GetTiposEventosAsync()
        {
            return await _eventoRepository.GetTiposEventosAsync();
        }
        public async Task AddEventoFotoAsync(EventoFotos foto)
        {
            await _eventoRepository.AddEventoFotoAsync(foto);
        }
        public async Task<IEnumerable<Evento>> GetEventosByTipoIdAsync(int tipoEventoId)
        {
            return await _eventoRepository.GetEventosByTipoIdAsync(tipoEventoId);
        }
    }
}