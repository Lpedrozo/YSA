using Microsoft.EntityFrameworkCore;
using YSA.Core.Entities;
using YSA.Data.Data;

namespace YSA.Data.Repositories
{
    public class PaqueteRepository : IPaqueteRepository
    {
        private readonly ApplicationDbContext _context;

        public PaqueteRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        
        // CRUD Básico
        public async Task<Paquete> GetByIdAsync(int id)
        {
            return await _context.Paquetes.FindAsync(id);
        }

        public async Task<List<Paquete>> GetAllAsync()
        {
            return await _context.Paquetes.ToListAsync();
        }

        public async Task<Paquete> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Paquetes
                .Include(p => p.PaqueteCursos)
                    .ThenInclude(pc => pc.Curso)
                .Include(p => p.PaqueteProductos)
                    .ThenInclude(pp => pp.Producto)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Paquete>> GetAllWithDetailsAsync()
        {
            return await _context.Paquetes
                .Include(p => p.PaqueteCursos)
                    .ThenInclude(pc => pc.Curso)
                .Include(p => p.PaqueteProductos)
                    .ThenInclude(pp => pp.Producto)
                .OrderByDescending(p => p.FechaPublicacion)
                .ToListAsync();
        }

        public async Task AddAsync(Paquete paquete)
        {
            await _context.Paquetes.AddAsync(paquete);
        }

        public async Task UpdateAsync(Paquete paquete)
        {
            _context.Paquetes.Update(paquete);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var paquete = await GetByIdAsync(id);
            if (paquete != null)
            {
                _context.Paquetes.Remove(paquete);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Paquetes.AnyAsync(p => p.Id == id);
        }

        // Relaciones
        public async Task AddCursoToPaqueteAsync(int paqueteId, int cursoId)
        {
            var existe = await _context.PaqueteCursos
                .AnyAsync(pc => pc.PaqueteId == paqueteId && pc.CursoId == cursoId);

            if (!existe)
            {
                _context.PaqueteCursos.Add(new PaqueteCurso
                {
                    PaqueteId = paqueteId,
                    CursoId = cursoId
                });
                // IMPORTANTE: No llamar a SaveChanges aquí, se llamará después en el servicio
            }
        }

        public async Task AddProductoToPaqueteAsync(int paqueteId, int productoId)
        {
            var existe = await _context.PaqueteProductos
                .AnyAsync(pp => pp.PaqueteId == paqueteId && pp.ProductoId == productoId);

            if (!existe)
            {
                _context.PaqueteProductos.Add(new PaqueteProducto
                {
                    PaqueteId = paqueteId,
                    ProductoId = productoId
                });
                // IMPORTANTE: No llamar a SaveChanges aquí, se llamará después en el servicio
            }
        }

        public async Task RemoveAllCursosFromPaqueteAsync(int paqueteId)
        {
            var cursos = await _context.PaqueteCursos
                .Where(pc => pc.PaqueteId == paqueteId)
                .ToListAsync();

            _context.PaqueteCursos.RemoveRange(cursos);
        }

        public async Task RemoveAllProductosFromPaqueteAsync(int paqueteId)
        {
            var productos = await _context.PaqueteProductos
                .Where(pp => pp.PaqueteId == paqueteId)
                .ToListAsync();

            _context.PaqueteProductos.RemoveRange(productos);
        }

        public async Task<List<Curso>> GetCursosNoAsociadosAsync(int paqueteId)
        {
            var cursosAsociadosIds = await _context.PaqueteCursos
                .Where(pc => pc.PaqueteId == paqueteId)
                .Select(pc => pc.CursoId)
                .ToListAsync();

            return await _context.Cursos
                .Where(c => !cursosAsociadosIds.Contains(c.Id))
                .ToListAsync();
        }

        public async Task<List<Producto>> GetProductosNoAsociadosAsync(int paqueteId)
        {
            var productosAsociadosIds = await _context.PaqueteProductos
                .Where(pp => pp.PaqueteId == paqueteId)
                .Select(pp => pp.ProductoId)
                .ToListAsync();

            return await _context.Productos
                .Where(p => !productosAsociadosIds.Contains(p.Id))
                .ToListAsync();
        }

        public async Task<List<Curso>> GetAllCursosAsync()
        {
            return await _context.Cursos.ToListAsync();
        }

        public async Task<List<Producto>> GetAllProductosAsync()
        {
            return await _context.Productos.ToListAsync();
        }
    }
}