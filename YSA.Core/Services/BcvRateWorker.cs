using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using YSA.Core.DTOs;
using YSA.Core.Entities;
using YSA.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace YSA.Core.Services
{
    public class BcvRateWorker : BackgroundService
    {
        // No necesitamos IServiceProvider si inyectamos directamente las dependencias Scoped
        private readonly IExchangeRateService _exchangeService;
        private readonly ITasaBCVRepository _tasaRepository;
        private readonly ILogger<BcvRateWorker> _logger; // Opcional, para registrar errores

        // Inyección directa de las dependencias requeridas
        // Nota: Las dependencias Scoped (como los repositorios con DbContext) son seguras
        // en un BackgroundService solo si se inyectan como Transient o se gestionan manualmente
        // el scope. Al inyectar directamente aquí, el worker es un Singleton, por lo que 
        // ¡DEBEMOS usar IServiceProvider y CreateScope! 
        // 
        // PERO, si tu Worker es la única clase que usa el repositorio y es Scoped, 
        // la mejor práctica es volver a usar IServiceProvider para resolver el scope
        // dentro del ExecuteAsync, como estaba antes, pero inyectando el ServiceProvider
        // y resolviendo el Repositorio, NO el DbContext.

        private readonly IServiceProvider _serviceProvider;

        // Constructor usando IServiceProvider para resolver dependencias Scoped (como Repositorios)
        public BcvRateWorker(IServiceProvider serviceProvider, ILogger<BcvRateWorker> logger = null)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger?.LogInformation("BcvRateWorker iniciado.");

            // Ejecutar la primera vez inmediatamente
            await GuardarTasaDiaria(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                // Intentamos ejecutarlo a las 2:00 AM para asegurar que la tasa del día se haya consolidado
                var nextRun = now.Date.AddDays(1).AddHours(2);
                var timeToWait = nextRun - now;

                if (timeToWait < TimeSpan.Zero)
                {
                    // Si ya pasó la hora de hoy, saltamos al día siguiente.
                    nextRun = now.Date.AddDays(2).AddHours(2);
                    timeToWait = nextRun - now;
                }

                _logger?.LogInformation($"Próxima ejecución de tasa BCV: {nextRun}. Esperando {timeToWait.TotalMinutes:N0} minutos.");

                await Task.Delay(timeToWait, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    await GuardarTasaDiaria(stoppingToken);
                }
            }
            _logger?.LogInformation("BcvRateWorker detenido.");
        }

        private async Task GuardarTasaDiaria(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var tasaRepository = scope.ServiceProvider.GetRequiredService<ITasaBCVRepository>();
                var exchangeService = scope.ServiceProvider.GetRequiredService<IExchangeRateService>();

                // --- REGLA 1: No consultar Sábado ni Domingo ---
                var today = DateTime.Today;
                if (today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday)
                {
                    _logger?.LogInformation($"Hoy es {today.DayOfWeek}. No se consulta ni se guarda una nueva tasa.");
                    return;
                }

                // --- REGLA 2: Calcular la próxima fecha de aplicación (Salto de fin de semana) ---
                var nextApplicableDate = today.AddDays(1);

                // Si hoy es Viernes, la tasa aplicará el Lunes.
                // Si mañana (nextApplicableDate) es Sábado o Domingo, lo movemos al Lunes.
                if (nextApplicableDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    // Si mañana es Sábado, la fecha de aplicación es el Lunes (+2 días)
                    nextApplicableDate = nextApplicableDate.AddDays(2);
                    _logger?.LogInformation("Detectado Viernes. La tasa actual aplicará el próximo Lunes.");
                }
                else if (nextApplicableDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    // Si mañana fuera Domingo (lógica de seguridad, no debería pasar si se salta el Sábado),
                    // la fecha de aplicación es el Lunes (+1 día)
                    nextApplicableDate = nextApplicableDate.AddDays(1);
                    _logger?.LogInformation("Detectado Sábado/Domingo. La tasa actual aplicará el próximo Lunes.");
                }

                try
                {
                    // 1. Verificar si la tasa de la Fecha de Aplicación ya está registrada
                    var existingRate = await tasaRepository.ExistsRateForDateAsync(nextApplicableDate, stoppingToken);

                    if (existingRate)
                    {
                        _logger?.LogInformation($"La tasa de cambio para {nextApplicableDate:d} ya existe. Saltando la consulta.");
                        return;
                    }

                    // 2. Obtener la tasa actual del API (esta es la tasa que se aplicará en nextApplicableDate)
                    var currentRate = await exchangeService.GetVenezuelaRateAsync();

                    if (currentRate.HasValue)
                    {
                        // 3. Crear el nuevo registro con la fecha de Aplicación
                        var newRate = new TasaBCV
                        {
                            Valor = currentRate.Value,
                            Fecha = nextApplicableDate, // <-- Guardamos con la fecha de aplicación calculada
                            FechaCreacion = DateTime.Now
                        };

                        // 4. Guardar en la base de datos
                        await tasaRepository.AddRateAsync(newRate);
                        _logger?.LogInformation($"Tasa de cambio guardada con éxito para {nextApplicableDate:d}. Valor: {currentRate.Value:N2}");
                    }
                    else
                    {
                        _logger?.LogWarning($"No se pudo obtener la tasa de cambio del API para {nextApplicableDate:d}. El servicio retornó un valor nulo.");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Error crítico al intentar guardar la tasa diaria para {nextApplicableDate:d}.");
                }
            }
        }
    }
}