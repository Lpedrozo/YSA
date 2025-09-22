using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace YSA.Data.Repositories
{
    public class ArtistaFotoRepository : IArtistaFotoRepository
    {
        private readonly ApplicationDbContext _context;

        public ArtistaFotoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ArtistaFoto> GetByIdAsync(int id)
        {
            return await _context.ArtistaFotos.FindAsync(id);
        }

        public async Task<List<ArtistaFoto>> GetByArtistaIdAsync(int artistaId)
        {
            return await _context.ArtistaFotos
                                 .Where(f => f.ArtistaId == artistaId)
                                 .ToListAsync();
        }

        public async Task AddAsync(ArtistaFoto foto)
        {
            await _context.ArtistaFotos.AddAsync(foto);
        }

        public async Task DeleteAsync(ArtistaFoto foto)
        {
            _context.ArtistaFotos.Remove(foto);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}