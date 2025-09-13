using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace YSA.Data.Repositories
{
    public class ProgresoLeccionRepository : IProgresoLeccionRepository
    {
        private readonly ApplicationDbContext _context;

        public ProgresoLeccionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProgresoLeccion> GetProgresoLeccionAsync(int estudianteId, int leccionId)
        {
            return await _context.ProgresoLecciones
                .FirstOrDefaultAsync(pl => pl.EstudianteId == estudianteId && pl.LeccionId == leccionId);
        }

        public async Task<List<ProgresoLeccion>> GetProgresoLeccionesByEstudianteAndCursoAsync(int estudianteId, int cursoId)
        {
            return await _context.ProgresoLecciones
                .Include(pl => pl.Leccion)
                .ThenInclude(l => l.Modulo)
                .Where(pl => pl.EstudianteId == estudianteId && pl.Leccion.Modulo.CursoId == cursoId)
                .ToListAsync();
        }

        public async Task AddAsync(ProgresoLeccion progreso)
        {
            await _context.ProgresoLecciones.AddAsync(progreso);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProgresoLeccion progreso)
        {
            _context.ProgresoLecciones.Update(progreso);
            await _context.SaveChangesAsync();
        }
    }
}