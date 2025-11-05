using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Data.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly ApplicationDbContext _context;

        public PedidoRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<int> GetPedidosPendientesAsync()
        {
            return await _context.Pedidos
                .Where(p => p.Estado == "Validando")
                .CountAsync();
        }
        public async Task<Pedido> GetByIdAsync(int id)
        {
            return await _context.Pedidos.FindAsync(id);
        }
        public async Task<Pedido> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Pedidos
                .Include(p => p.PedidoItems)
                    .ThenInclude(pi => pi.VentaItem)
                        .ThenInclude(vi => vi.Curso)
                .Include(p => p.PedidoItems)
                    .ThenInclude(pi => pi.VentaItem)
                        .ThenInclude(vi => vi.Producto)
                .Include(p => p.Estudiante) // Por si necesitas info del estudiante
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<IEnumerable<Pedido>> GetAllAsync()
        {
            return await _context.Pedidos.ToListAsync();
        }

        public async Task<Pedido> AddAsync(Pedido pedido)
        {
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            return pedido;
        }

        public async Task<bool> UpdateAsync(Pedido pedido)
        {
            _context.Entry(pedido).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return false;

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();
            return true;
        }

        // El nombre del método y la lógica de inclusión ahora coinciden
        public async Task<Pedido> GetPedidoWithItemsAndVentaItemsAsync(int id)
        {
            return await _context.Pedidos
                                 .Include(p => p.PedidoItems)
                                     .ThenInclude(pi => pi.VentaItem)
                                         .ThenInclude(vi => vi.Curso) // Inclusión del curso
                                 .Include(p => p.PedidoItems)
                                     .ThenInclude(pi => pi.VentaItem)
                                         .ThenInclude(vi => vi.Producto) // Inclusión del producto
                                 .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<Pago> GetPagoWithPedido(int id)
        {
            return await _context.Pagos
                                 .Where(p => p.PedidoId == id)
                                 .FirstOrDefaultAsync();
        }
        public async Task<PedidoItem> AddPedidoItemAsync(PedidoItem item)
        {
            _context.PedidoItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<Pago> AddPagoAsync(Pago pago)
        {
            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();
            return pago;
        }
        public async Task<IEnumerable<Pedido>> GetPedidosByEstadoAsync(string estado)
        {
            return await _context.Pedidos
                                 .Include(p => p.Estudiante)
                                 .Where(p => p.Estado == estado)
                                 .ToListAsync();
        }
        public async Task<Pedido> ObtenerPedidoConItemsYVentaItemsAsync(int id)
        {
            return await _context.Pedidos
                                 .Include(p => p.PedidoItems)
                                     .ThenInclude(pi => pi.VentaItem)
                                 .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<bool> ExistePedidoEnEstadoParaCursoAsync(int estudianteId, int cursoId, string estado)
        {
            return await _context.Pedidos
                .Include(p => p.PedidoItems)
                .ThenInclude(pi => pi.VentaItem)
                .AnyAsync(p => p.EstudianteId == estudianteId && p.Estado == estado &&
                               p.PedidoItems.Any(pi => pi.VentaItem.CursoId == cursoId));
        }
        public async Task<IEnumerable<Pedido>> GetPedidosByUsuarioAndEstadoAsync(int estudianteId, string estado)
        {
            return await _context.Pedidos
                .Include(p => p.PedidoItems)
                    .ThenInclude(pi => pi.VentaItem)
                .Where(p => p.EstudianteId == estudianteId && p.Estado == estado)
                .ToListAsync();
        }
        public async Task<IEnumerable<VentaItem>> GetItemsByPedidoIdAsync(int pedidoId)
        {
            return await _context.VentaItems
                .Where(vi => vi.PedidoItems.Any(pi => pi.PedidoId == pedidoId))
                .Include(vi => vi.Curso) 
                .ToListAsync();
        }
    }
}