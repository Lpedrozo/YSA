using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Data.Repositories
{
    public class VentaItemRepository : IVentaItemRepository
    {
        private readonly ApplicationDbContext _context;

        public VentaItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VentaItem> GetByIdAsync(int id)
        {
            return await _context.VentaItems
                                 .Include(vi => vi.Curso)
                                 .Include(vi => vi.Producto)
                                 .FirstOrDefaultAsync(vi => vi.Id == id);
        }

        public async Task<VentaItem> GetByCursoIdAsync(int cursoId)
        {
            return await _context.VentaItems.FirstOrDefaultAsync(vi => vi.CursoId == cursoId);
        }

        public async Task<VentaItem> GetByProductoIdAsync(int productoId)
        {
            return await _context.VentaItems.FirstOrDefaultAsync(vi => vi.ProductoId == productoId);
        }

        public async Task<IEnumerable<VentaItem>> GetByIdsAsync(IEnumerable<int> ids)
        {
            return await _context.VentaItems
                                 .Where(vi => ids.Contains(vi.Id))
                                 .Include(vi => vi.Curso)
                                 .Include(vi => vi.Producto)
                                 .ToListAsync();
        }

        public async Task<VentaItem> AddAsync(VentaItem ventaItem)
        {
            _context.VentaItems.Add(ventaItem);
            await _context.SaveChangesAsync();
            return ventaItem;
        }
        public async Task UpdateAsync(VentaItem ventaItem)
        {
            _context.Entry(ventaItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<VentaItem>> GetItemsByPedidoIdAsync(int pedidoId)
        {
            return await _context.VentaItems
                .Where(vi => vi.PedidoItems.Any(pi => pi.PedidoId == pedidoId))
                .Include(vi => vi.Curso) // Incluir el curso para obtener el título
                .ToListAsync();
        }

    }
}