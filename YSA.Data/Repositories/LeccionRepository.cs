using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Data.Repositories
{
    public class LeccionRepository : ILeccionRepository
    {
        private readonly ApplicationDbContext _context;

        public LeccionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Leccion>> GetByModuloIdAsync(int moduloId)
        {
            return await _context.Lecciones
                                 .Where(l => l.ModuloId == moduloId)
                                 .ToListAsync();
        }

        public async Task<Leccion> GetByIdAsync(int id)
        {
            return await _context.Lecciones.FindAsync(id);
        }

        public async Task AddAsync(Leccion leccion)
        {
            await _context.Lecciones.AddAsync(leccion);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Leccion leccion)
        {
            _context.Lecciones.Update(leccion);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var leccion = await _context.Lecciones.FindAsync(id);
            if (leccion != null)
            {
                _context.Lecciones.Remove(leccion);
                await _context.SaveChangesAsync();
            }
        }
    }
}