using YSA.Core.Entities;

namespace YSA.Core.Interfaces
{
    public interface INotificacionRepository
    {
        Task<Notificacion> GetByIdAsync(int id);
        Task<List<Notificacion>> GetByUsuarioIdAsync(int usuarioId);
        Task<List<Notificacion>> GetNoLeidasByUsuarioIdAsync(int usuarioId);
        Task<Notificacion> CreateAsync(Notificacion notificacion);
        Task<Notificacion> UpdateAsync(Notificacion notificacion);
        Task MarkAsReadAsync(int notificacionId);
        Task MarkAllAsReadAsync(int usuarioId);
        Task<bool> DeleteAsync(int id);
        Task<int> GetCountNoLeidasAsync(int usuarioId);
        Task<List<TipoNotificacion>> GetTiposNotificacionAsync();
        Task<TipoNotificacion> GetTipoNotificacionByIdAsync(int id);
    }
}