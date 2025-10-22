using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Data;

namespace YSA.Data.Repositories
{
    public class ArticuloRepository : IArticuloRepository
    {
        private readonly ApplicationDbContext _context;

        public ArticuloRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Articulo> GetByIdAsync(int id)
        {
            return await _context.Articulos
                                 .Include(a => a.Fotos)
                                 .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Articulo>> GetAllAsync()
        {
            return await _context.Articulos.ToListAsync();
        }

        public async Task AddAsync(Articulo articulo)
        {
            await _context.Articulos.AddAsync(articulo);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Articulo articulo)
        {
            _context.Articulos.Update(articulo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var articulo = await _context.Articulos.FindAsync(id);
            if (articulo != null)
            {
                _context.Articulos.Remove(articulo);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ArticuloFoto> GetFotoByIdAsync(int id)
        {
            return await _context.ArticuloFotos.FindAsync(id);
        }

        public async Task DeleteFotoAsync(int id)
        {
            var foto = await _context.ArticuloFotos.FindAsync(id);
            if (foto != null)
            {
                _context.ArticuloFotos.Remove(foto);
                await _context.SaveChangesAsync();
            }
        }
    }
}
