using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Data.Repositories
{
    public class ModuloRepository : IModuloRepository
    {
        private readonly ApplicationDbContext _context;

        public ModuloRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Modulo>> GetByCursoIdAsync(int cursoId)
        {
            return await _context.Modulos
                                 .Where(m => m.CursoId == cursoId)
                                 .Include(m => m.Lecciones)
                                 .ToListAsync();
        }

        public async Task<Modulo> GetByIdAsync(int id)
        {
            return await _context.Modulos.FindAsync(id);
        }

        public async Task AddAsync(Modulo modulo)
        {
            await _context.Modulos.AddAsync(modulo);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Modulo modulo)
        {
            _context.Modulos.Update(modulo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var modulo = await _context.Modulos.FindAsync(id);
            if (modulo != null)
            {
                _context.Modulos.Remove(modulo);
                await _context.SaveChangesAsync();
            }
        }
    }
}