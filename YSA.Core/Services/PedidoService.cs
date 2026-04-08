// YSA.Core.Interfaces/IEmailService.cs
namespace YSA.Core.Interfaces
{
    public interface IEmailService
    {
        Task<bool> EnviarNotificacionAdminInscripcionGratuitaAsync(string nombreUsuario, string emailUsuario, string claseTitulo, string cursoTitulo, DateTime fechaClase, string lugar);
        Task<bool> EnviarNotificacionAdminPagoPendienteAsync(string nombreUsuario, string emailUsuario, string claseTitulo, string cursoTitulo, decimal monto, int pedidoId, string comprobanteUrl);
        Task<bool> EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml, string? copia = null);
        Task<bool> EnviarCorreoCompraPendienteAsync(string destinatario, string nombreUsuario, int pedidoId, decimal total, List<string> items);
        Task<bool> EnviarCorreoCompraAprobadaAsync(string destinatario, string nombreUsuario, int pedidoId, string tipoItem, string nombreItem);
        Task<bool> EnviarCorreoCompraRechazadaAsync(string destinatario, string nombreUsuario, int pedidoId, string motivo);
        Task<bool> EnviarCorreoBienvenidaAsync(string destinatario, string nombreUsuario);
        Task<bool> EnviarCorreoSuscripcionActivadaAsync(string destinatario, string nombreUsuario, string planNombre, DateTime fechaInicio, DateTime fechaFin);
    }
}