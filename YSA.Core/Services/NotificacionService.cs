using YSA.Core.Entities;
using YSA.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Core.Services
{
    public class NotificacionService : INotificacionService
    {
        private readonly INotificacionRepository _notificacionRepository;

        public NotificacionService(INotificacionRepository notificacionRepository)
        {
            _notificacionRepository = notificacionRepository;
        }

        public async Task CrearNotificacionPagoConfirmadoAsync(int usuarioId, int pedidoId, decimal monto)
        {
            var notificacion = new Notificacion
            {
                UsuarioId = usuarioId,
                TipoNotificacionId = 1, // Pago Confirmado
                Titulo = "Pago Confirmado",
                Mensaje = $"Tu pago de {monto:C} ha sido confirmado exitosamente.",
                UrlDestino = $"/MisCursos",
                EntidadId = pedidoId,
                TipoEntidad = "Pedido"
            };

            await _notificacionRepository.CreateAsync(notificacion);
        }

        public async Task CrearNotificacionPagoPendienteAsync(int usuarioId, int pedidoId, decimal monto)
        {
            var notificacion = new Notificacion
            {
                UsuarioId = usuarioId,
                TipoNotificacionId = 2, // Pago Pendiente
                Titulo = "Pago en Proceso",
                Mensaje = $"Tu pago de {monto:C} está siendo procesado. Te notificaremos cuando sea confirmado.",
                UrlDestino = $"/Pedidos/Detalles/{pedidoId}",
                EntidadId = pedidoId,
                TipoEntidad = "Pedido"
            };

            await _notificacionRepository.CreateAsync(notificacion);
        }

        public async Task CrearNotificacionCursoCompradoAsync(int usuarioId, int cursoId, string cursoTitulo)
        {
            var notificacion = new Notificacion
            {
                UsuarioId = usuarioId,
                TipoNotificacionId = 3, // Curso Comprado
                Titulo = "Curso Adquirido",
                Mensaje = $"Has adquirido el curso: {cursoTitulo}. ¡Ya puedes comenzar a aprender!",
                UrlDestino = $"/Cursos/Detalles/{cursoId}",
                EntidadId = cursoId,
                TipoEntidad = "Curso"
            };

            await _notificacionRepository.CreateAsync(notificacion);
        }

        public async Task CrearNotificacionProductoCompradoAsync(int usuarioId, int productoId, string productoTitulo)
        {
            var notificacion = new Notificacion
            {
                UsuarioId = usuarioId,
                TipoNotificacionId = 4, // Producto Comprado
                Titulo = "Producto Adquirido",
                Mensaje = $"Has adquirido el producto: {productoTitulo}. Ya está disponible en tu biblioteca.",
                UrlDestino = $"/Productos/Descargar/{productoId}",
                EntidadId = productoId,
                TipoEntidad = "Producto"
            };

            await _notificacionRepository.CreateAsync(notificacion);
        }

        public async Task<List<Notificacion>> ObtenerNotificacionesUsuarioAsync(int usuarioId)
        {
            var notificaciones = await _notificacionRepository.GetByUsuarioIdAsync(usuarioId);
            return notificaciones.Select(MapToViewModel).ToList();
        }

        public async Task<List<Notificacion>> ObtenerNotificacionesNoLeidasAsync(int usuarioId)
        {
            var notificaciones = await _notificacionRepository.GetNoLeidasByUsuarioIdAsync(usuarioId);
            return notificaciones.Select(MapToViewModel).ToList();
        }

        public async Task MarcarComoLeidaAsync(int notificacionId, int usuarioId)
        {
            var notificacion = await _notificacionRepository.GetByIdAsync(notificacionId);
            if (notificacion != null && notificacion.UsuarioId == usuarioId)
            {
                await _notificacionRepository.MarkAsReadAsync(notificacionId);
            }
        }

        public async Task MarcarTodasComoLeidasAsync(int usuarioId)
        {
            await _notificacionRepository.MarkAllAsReadAsync(usuarioId);
        }

        public async Task<int> ObtenerCantidadNoLeidasAsync(int usuarioId)
        {
            return await _notificacionRepository.GetCountNoLeidasAsync(usuarioId);
        }

        public async Task<bool> EliminarNotificacionAsync(int notificacionId, int usuarioId)
        {
            var notificacion = await _notificacionRepository.GetByIdAsync(notificacionId);
            if (notificacion != null && notificacion.UsuarioId == usuarioId)
            {
                return await _notificacionRepository.DeleteAsync(notificacionId);
            }
            return false;
        }

        private Notificacion MapToViewModel(Notificacion notificacion)
        {
            return new Notificacion
            {
                Id = notificacion.Id,
                Titulo = notificacion.Titulo,
                Mensaje = notificacion.Mensaje,
                UrlDestino = notificacion.UrlDestino,
                FechaCreacion = notificacion.FechaCreacion,
                EsLeida = notificacion.EsLeida,
                TipoNotificacionId = notificacion.TipoNotificacionId
            };
        }
    }
}