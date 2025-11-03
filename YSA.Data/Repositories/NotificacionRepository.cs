using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace YSA.Data.Repositories
{
    public class NotificacionRepository : INotificacionRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificacionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Notificacion> GetByIdAsync(int id)
        {
            return await _context.Notificaciones
                .Include(n => n.TipoNotificacion)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<List<Notificacion>> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _context.Notificaciones
                .Include(n => n.TipoNotificacion)
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();
        }

        public async Task<List<Notificacion>> GetNoLeidasByUsuarioIdAsync(int usuarioId)
        {
            return await _context.Notificaciones
                .Include(n => n.TipoNotificacion)
                .Where(n => n.UsuarioId == usuarioId && !n.EsLeida)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();
        }

        public async Task<Notificacion> CreateAsync(Notificacion notificacion)
        {
            notificacion.FechaCreacion = DateTime.UtcNow;
            notificacion.EsLeida = false;
            notificacion.EsEnviada = false;

            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();
            return notificacion;
        }

        public async Task<Notificacion> UpdateAsync(Notificacion notificacion)
        {
            _context.Notificaciones.Update(notificacion);
            await _context.SaveChangesAsync();
            return notificacion;
        }

        public async Task MarkAsReadAsync(int notificacionId)
        {
            var notificacion = await _context.Notificaciones.FindAsync(notificacionId);
            if (notificacion != null)
            {
                notificacion.EsLeida = true;
                notificacion.FechaLeida = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int usuarioId)
        {
            var notificaciones = await _context.Notificaciones
                .Where(n => n.UsuarioId == usuarioId && !n.EsLeida)
                .ToListAsync();

            foreach (var notificacion in notificaciones)
            {
                notificacion.EsLeida = true;
                notificacion.FechaLeida = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var notificacion = await _context.Notificaciones.FindAsync(id);
            if (notificacion == null)
                return false;

            _context.Notificaciones.Remove(notificacion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCountNoLeidasAsync(int usuarioId)
        {
            return await _context.Notificaciones
                .CountAsync(n => n.UsuarioId == usuarioId && !n.EsLeida);
        }

        public async Task<List<TipoNotificacion>> GetTiposNotificacionAsync()
        {
            return await _context.TipoNotificaciones.ToListAsync();
        }

        public async Task<TipoNotificacion> GetTipoNotificacionByIdAsync(int id)
        {
            return await _context.TipoNotificaciones.FindAsync(id);
        }
    }
}