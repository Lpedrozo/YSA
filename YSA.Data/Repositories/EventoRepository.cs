using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace YSA.Data.Repositories
{
    public class EventoRepository : IEventoRepository
    {
        private readonly ApplicationDbContext _context;
        public EventoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Evento> GetByIdAsync(int id)
        {
            return await _context.Eventos
                                 .Include(e => e.TipoEvento)
                                 .Include(e => e.Fotos)
                                 .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Evento>> GetAllAsync()
        {
            return await _context.Eventos
                                 .Include(e => e.TipoEvento)
                                 .Include(e => e.Fotos)
                                 .ToListAsync();
        }

        public async Task AddAsync(Evento evento)
        {
            await _context.Eventos.AddAsync(evento);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Evento evento)
        {
            _context.Entry(evento).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var evento = await _context.Eventos.FindAsync(id);
            if (evento != null)
            {
                _context.Eventos.Remove(evento);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<TipoEvento>> GetTiposEventosAsync()
        {
            return await _context.TipoEventos.ToListAsync();
        }
        public async Task AddEventoFotoAsync(EventoFotos foto)
        {
            _context.EventoFotos.Add(foto);
            await _context.SaveChangesAsync();
        }
    }
}