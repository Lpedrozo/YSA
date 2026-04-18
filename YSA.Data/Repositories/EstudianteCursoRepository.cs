using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Data.Repositories
{
    public class EstudianteCursoRepository : IEstudianteCursoRepository
    {
        private readonly ApplicationDbContext _context;

        public EstudianteCursoRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<List<int>> GetEstudianteCursoIdsAsync(int estudianteId)
        {
            return await _context.EstudianteCursos
                .Where(ec => ec.EstudianteId == estudianteId)
                .Select(ec => ec.CursoId)
                .ToListAsync();
        }
        public async Task<bool> TieneAccesoAlCursoAsync(int estudianteId, int cursoId)
        {
            return await _context.EstudianteCursos
                .AnyAsync(ec => ec.EstudianteId == estudianteId && ec.CursoId == cursoId);
        }

        public async Task<EstudianteCurso> AddAsync(EstudianteCurso estudianteCurso)
        {
            _context.EstudianteCursos.Add(estudianteCurso);
            await _context.SaveChangesAsync();
            return estudianteCurso;
        }

        public async Task<EstudianteCurso> GetByEstudianteIdAndCursoIdAsync(int estudianteId, int cursoId)
        {
            return await _context.EstudianteCursos
                                 .FirstOrDefaultAsync(ec => ec.EstudianteId == estudianteId && ec.CursoId == cursoId);
        }

        public async Task<bool> ExisteAccesoAsync(int estudianteId, int cursoId)
        {
            return await _context.EstudianteCursos.AnyAsync(ec => ec.EstudianteId == estudianteId && ec.CursoId == cursoId);
        }
    }
}