using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace YSA.Data.Repositories
{
    public class ArtistaRepository : IArtistaRepository
    {
        private readonly ApplicationDbContext _context;

        public ArtistaRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Artista> GetByUsuarioIdAsync(string userId)
        {
            return await _context.Artistas.Include(a => a.Usuario).FirstOrDefaultAsync(a => a.UsuarioId.ToString() == userId);
        }
        public async Task<Artista> GetByIdAsync(int id)
        {
            return await _context.Artistas.Include(a => a.Usuario).FirstOrDefaultAsync(a => a.Id == id);
        }
        public async Task<List<Curso>> GetCursosByArtistaAsync(int id)
        {
            return await _context.CursoInstructores
                .Where(ci => ci.ArtistaId == id) // Filtra por el Artista/Instructor
                .Select(ci => ci.Curso)          // Selecciona el objeto Curso asociado
                .ToListAsync();
        }

        public async Task<List<Artista>> GetAllAsync()
        {
            return await _context.Artistas.Include(a => a.Usuario).ToListAsync();
        }

        public async Task AddAsync(Artista artista)
        {
            await _context.Artistas.AddAsync(artista);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Artista artista)
        {
            _context.Artistas.Update(artista);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Artista artista)
        {
            _context.Artistas.Remove(artista);
            await _context.SaveChangesAsync();
        }
        
    }
}