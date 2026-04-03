// YSA.Data.Repositories/SuscripcionArtistaRepository.cs
using Microsoft.EntityFrameworkCore;
using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Data;

namespace YSA.Data.Repositories
{
    public class SuscripcionArtistaRepository : ISuscripcionArtistaRepository
    {
        private readonly ApplicationDbContext _context;

        public SuscripcionArtistaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SuscripcionArtista>> GetAllAsync()
        {
            return await _context.SuscripcionesArtistas
                .Include(s => s.Artista)
                    .ThenInclude(a => a.Usuario)
                .Include(s => s.Plan)
                .Include(s => s.ValidadoPor)
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();
        }

        public async Task<List<SuscripcionArtista>> GetByArtistaIdAsync(int artistaId)
        {
            return await _context.SuscripcionesArtistas
                .Include(s => s.Artista)
                    .ThenInclude(a => a.Usuario)
                .Include(s => s.Plan)
                .Include(s => s.ValidadoPor)
                .Where(s => s.ArtistaId == artistaId)
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();
        }

        public async Task<List<SuscripcionArtista>> GetPendientesValidacionAsync()
        {
            return await _context.SuscripcionesArtistas
                .Include(s => s.Artista)
                    .ThenInclude(a => a.Usuario)
                .Include(s => s.Plan)
                .Where(s => s.Estado == "PagadoValidacion")
                .OrderBy(s => s.FechaCreacion)
                .ToListAsync();
        }

        public async Task<SuscripcionArtista> GetByIdAsync(int id)
        {
            return await _context.SuscripcionesArtistas
                .Include(s => s.Artista)
                    .ThenInclude(a => a.Usuario)
                .Include(s => s.Plan)
                .Include(s => s.ValidadoPor)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<SuscripcionArtista> AddAsync(SuscripcionArtista suscripcion)
        {
            await _context.SuscripcionesArtistas.AddAsync(suscripcion);
            await _context.SaveChangesAsync();
            return suscripcion;
        }

        public async Task<SuscripcionArtista> UpdateAsync(SuscripcionArtista suscripcion)
        {
            _context.SuscripcionesArtistas.Update(suscripcion);
            await _context.SaveChangesAsync();
            return suscripcion;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var suscripcion = await GetByIdAsync(id);
            if (suscripcion == null) return false;

            _context.SuscripcionesArtistas.Remove(suscripcion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsActiveSubscriptionAsync(int artistaId)
        {
            return await _context.SuscripcionesArtistas
                .AnyAsync(s => s.ArtistaId == artistaId &&
                              s.Estado == "Activa" &&
                              s.FechaFin > DateTime.UtcNow);
        }

        public async Task<SuscripcionArtista> GetActiveSubscriptionAsync(int artistaId)
        {
            return await _context.SuscripcionesArtistas
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.ArtistaId == artistaId &&
                                         s.Estado == "Activa" &&
                                         s.FechaFin > DateTime.UtcNow);
        }
    }
}