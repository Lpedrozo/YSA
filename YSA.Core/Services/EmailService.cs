// YSA.Core.Services/EmailService.cs
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using YSA.Core.Interfaces;

namespace YSA.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml, string? copia = null)
        {
            try
            {
                var smtpConfig = _configuration.GetSection("EmailSettings");
                var host = smtpConfig["Host"];
                var port = int.Parse(smtpConfig["Port"]);
                var enableSsl = bool.Parse(smtpConfig["EnableSsl"]);
                var username = smtpConfig["Username"];
                var password = smtpConfig["Password"];
                var fromEmail = smtpConfig["FromEmail"];
                var fromName = smtpConfig["FromName"];

                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = enableSsl,
                    Credentials = new NetworkCredential(username, password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = asunto,
                    Body = cuerpoHtml,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(destinatario);

                if (!string.IsNullOrEmpty(copia))
                {
                    mailMessage.CC.Add(copia);
                }

                await client.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando correo: {ex.Message}");
                return false;
            }
        }


        public async Task<bool> EnviarCorreoCompraPendienteAsync(string destinatario, string nombreUsuario, int pedidoId, decimal total, List<string> items)
        {
            var itemsHtml = string.Join("", items.Select(item => $"<li>{item}</li>"));

            var cuerpo = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #ef8830 0%, #e07a2a 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .pedido-info {{ background: white; padding: 15px; border-radius: 8px; margin: 15px 0; }}
                        .button {{ background: #ef8830; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; display: inline-block; }}
                        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>¡Hola {nombreUsuario}!</h2>
                            <p>Tu compra está en proceso de validación</p>
                        </div>
                        <div class='content'>
                            <h3>Detalles de tu pedido #{pedidoId}</h3>
                            <div class='pedido-info'>
                                <p><strong>Productos adquiridos:</strong></p>
                                <ul>{itemsHtml}</ul>
                                <p><strong>Total pagado:</strong> ${total:F2} USD</p>
                                <p><strong>Estado:</strong> ⏳ Pendiente de validación</p>
                            </div>
                            <p>Hemos recibido un comprobante de pago. El equipo debo revisarlo lo más pronto posible
                            <br>
                        </div>
                        <div class='footer'>
                            <p>© 2024 Yo Soy Arte - Todos los derechos reservados</p>
                            <p>¿Tienes preguntas? Contáctanos a soporte@yosoyarte.com</p>
                        </div>
                    </div>
                </body>
                </html>";

            return await EnviarCorreoAsync(destinatario, $"Yo Soy Arte - Pedido #{pedidoId} pendiente de validación", cuerpo);
        }

        public async Task<bool> EnviarCorreoCompraAprobadaAsync(string destinatario, string nombreUsuario, int pedidoId, string tipoItem, string nombreItem)
        {
            var cuerpo = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #28a745 0%, #1e7e34 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .button {{ background: #28a745; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; display: inline-block; }}
                        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>¡Felicitaciones {nombreUsuario}!</h2>
                            <p>Tu compra ha sido aprobada</p>
                        </div>
                        <div class='content'>
                            <h3>¡Ya tienes acceso a tu {tipoItem}!</h3>
                            <div class='pedido-info'>
                                <p><strong>{tipoItem}:</strong> {nombreItem}</p>
                                <p><strong>Estado:</strong> ✅ Aprobado</p>
                            </div>
                            <p>Ya puedes acceder a tu contenido desde tu perfil.</p>
                            <br>
                            <a href='https://academiayosoyarte.com/' class='button'>Ver mi contenido</a>
                        </div>
                        <div class='footer'>
                            <p>© 2024 Yo Soy Arte - Todos los derechos reservados</p>
                        </div>
                    </div>
                </body>
                </html>";

            return await EnviarCorreoAsync(destinatario, $"Yo Soy Arte - ¡Tu {tipoItem} ya está disponible!", cuerpo);
        }

        public async Task<bool> EnviarCorreoCompraRechazadaAsync(string destinatario, string nombreUsuario, int pedidoId, string motivo)
        {
            var cuerpo = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #dc3545 0%, #b02a37 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Hola {nombreUsuario}</h2>
                            <p>Tu pago no pudo ser validado</p>
                        </div>
                        <div class='content'>
                            <h3>Pedido #{pedidoId}</h3>
                            <div class='pedido-info'>
                                <p><strong>Estado:</strong> ❌ Rechazado</p>
                                <p><strong>Motivo:</strong> {motivo}</p>
                            </div>
                            <p>Por favor, contacta a nuestro equipo de soporte para resolver esta situación.</p>
                            <br>
                            <a href='https://wa.me/584121463374' class='button'>Contactar soporte</a>
                        </div>
                        <div class='footer'>
                            <p>© 2024 Yo Soy Arte - Todos los derechos reservados</p>
                        </div>
                    </div>
                </body>
                </html>";

            return await EnviarCorreoAsync(destinatario, $"Yo Soy Arte - Pedido #{pedidoId} rechazado", cuerpo);
        }

        public async Task<bool> EnviarCorreoBienvenidaAsync(string destinatario, string nombreUsuario)
        {
            var cuerpo = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #ef8830 0%, #e07a2a 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>¡Bienvenido a Yo Soy Arte, {nombreUsuario}!</h2>
                        </div>
                        <div class='content'>
                            <p>Nos alegra tenerte en nuestra comunidad artística.</p>
                            <p>Explora nuestros cursos, eventos y productos exclusivos para artistas.</p>
                            <br>
                            <a href='https://academiayosoyarte.com/' class='button'>Explorar cursos</a>
                        </div>
                        <div class='footer'>
                            <p>© 2024 Yo Soy Arte - Todos los derechos reservados</p>
                        </div>
                    </div>
                </body>
                </html>";

            return await EnviarCorreoAsync(destinatario, "¡Bienvenido a Yo Soy Arte!", cuerpo);
        }

        public async Task<bool> EnviarCorreoSuscripcionActivadaAsync(string destinatario, string nombreUsuario, string planNombre, DateTime fechaInicio, DateTime fechaFin)
        {
            var cuerpo = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #28a745 0%, #1e7e34 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>¡Tu suscripción ha sido activada!</h2>
                        </div>
                        <div class='content'>
                            <p>Hola <strong>{nombreUsuario}</strong>,</p>
                            <p>Tu suscripción al plan <strong>{planNombre}</strong> ha sido activada exitosamente.</p>
                            <div class='pedido-info'>
                                <p><strong>Fecha de inicio:</strong> {fechaInicio:dd/MM/yyyy}</p>
                                <p><strong>Fecha de vencimiento:</strong> {fechaFin:dd/MM/yyyy}</p>
                            </div>
                            <p>Ya puedes comenzar a publicar tu contenido y disfrutar de los beneficios de tu plan.</p>
                            <br>
                            <a href='https://academiayosoyarte.com/' class='button'>Ir a mi panel</a>
                        </div>
                        <div class='footer'>
                            <p>© 2024 Yo Soy Arte - Todos los derechos reservados</p>
                        </div>
                    </div>
                </body>
                </html>";

            return await EnviarCorreoAsync(destinatario, $"Yo Soy Arte - Tu suscripción {planNombre} está activa", cuerpo);
        }
        // YSA.Core.Services/EmailService.cs - Agregar estos métodos

        public async Task<bool> EnviarNotificacionAdminInscripcionGratuitaAsync(string nombreUsuario, string emailUsuario, string claseTitulo, string cursoTitulo, DateTime fechaClase, string lugar)
        {
            var adminEmail = "yosoyarte.contacto@gmail.com";
            var fechaFormateada = fechaClase.ToString("dddd, dd 'de' MMMM 'de' yyyy 'a las' hh:mm tt");

            var cuerpo = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background: linear-gradient(135deg, #28a745 0%, #1e7e34 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
                .info-box {{ background: white; padding: 15px; border-radius: 8px; margin: 15px 0; border-left: 4px solid #28a745; }}
                .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>✨ NUEVA INSCRIPCIÓN GRATUITA ✨</h2>
                    <p>Un estudiante se ha inscrito a una clase presencial</p>
                </div>
                <div class='content'>
                    <div class='info-box'>
                        <h3>Información del estudiante</h3>
                        <p><strong>Nombre:</strong> {nombreUsuario}</p>
                        <p><strong>Email:</strong> {emailUsuario}</p>
                    </div>
                    
                    <div class='info-box'>
                        <h3>Información de la clase</h3>
                        <p><strong>Clase:</strong> {claseTitulo}</p>
                        <p><strong>Curso:</strong> {cursoTitulo}</p>
                        <p><strong>Fecha y hora:</strong> {fechaFormateada}</p>
                        <p><strong>Lugar:</strong> {lugar}</p>
                        <p><strong>Tipo:</strong> 🎁 GRATUITA</p>
                    </div>
                    
                    <p>El estudiante ya tiene su cupo confirmado automáticamente.</p>
                </div>
                <div class='footer'>
                    <p>© 2024 Yo Soy Arte - Sistema de gestión</p>
                </div>
            </div>
        </body>
        </html>";

            return await EnviarCorreoAsync(adminEmail, $"Yo Soy Arte - Nueva inscripción gratuita: {claseTitulo}", cuerpo);
        }

        public async Task<bool> EnviarNotificacionAdminPagoPendienteAsync(string nombreUsuario, string emailUsuario, string claseTitulo, string cursoTitulo, decimal monto, int pedidoId, string comprobanteUrl)
        {
            var adminEmail = "yosoyarte.contacto@gmail.com";
            //var adminEmail = "lpedrozohernandez@gmail.com";
            var comprobanteLink = !string.IsNullOrEmpty(comprobanteUrl) ? $"{comprobanteUrl}" : "No disponible";

            var cuerpo = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background: linear-gradient(135deg, #ef8830 0%, #e07a2a 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
                .info-box {{ background: white; padding: 15px; border-radius: 8px; margin: 15px 0; border-left: 4px solid #ef8830; }}
                .button {{ background: #ef8830; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; }}
                .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>💰 PAGO PENDIENTE DE VALIDACIÓN 💰</h2>
                    <p>Un estudiante ha subido un comprobante de pago</p>
                </div>
                <div class='content'>
                    <div class='info-box'>
                        <h3>Información del estudiante</h3>
                        <p><strong>Nombre:</strong> {nombreUsuario}</p>
                        <p><strong>Email:</strong> {emailUsuario}</p>
                    </div>
                    
                    <div class='info-box'>
                        <h3>Información de la clase</h3>
                        <p><strong>Clase:</strong> {claseTitulo}</p>
                        <p><strong>Curso:</strong> {cursoTitulo}</p>
                    </div>
                    
                    <div class='info-box'>
                        <h3>Información del pedido</h3>
                        <p><strong>Pedido #:</strong> {pedidoId}</p>
                        <p><strong>Comprobante:</strong> <a href='{comprobanteLink}' target='_blank' class='button' style='background: #ef8830; color: white; padding: 5px 10px; text-decoration: none; border-radius: 5px;'>Ver comprobante</a></p>
                    </div>
                    
                    <p>Por favor, revisa el comprobante y valida o rechaza el pago en el panel de administración.</p>
                    <br>
                    <a href='https://academiayosoyarte.com/' class='button'>Ir a validar pagos</a>
                </div>
                <div class='footer'>
                    <p>© 2024 Yo Soy Arte - Sistema de gestión</p>
                </div>
            </div>
        </body>
        </html>";

            return await EnviarCorreoAsync(adminEmail, $"Yo Soy Arte - Pago pendiente: {claseTitulo} - Pedido #{pedidoId}", cuerpo);
        }
    }
}