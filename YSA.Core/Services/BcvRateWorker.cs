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

                // **CAMBIO CLAVE: La fecha objetivo es MAÑANA**
                var tomorrow = DateTime.Today.AddDays(1);

                try
                {
                    // 1. Verificar si la tasa de MAÑANA ya está registrada
                    // Debes asegurarte de que tu método ExistsRateForDateAsync funcione correctamente,
                    // comparando solo la fecha (como ya lo tienes implementado).
                    var existingRate = await tasaRepository.ExistsRateForDateAsync(tomorrow, stoppingToken);

                    if (existingRate)
                    {
                        _logger?.LogInformation($"La tasa de cambio para {tomorrow:d} ya existe. Saltando la consulta.");
                        return;
                    }

                    // 2. Obtener la tasa actual del API (esta es la tasa que se aplicará MAÑANA)
                    var currentRate = await exchangeService.GetVenezuelaRateAsync();

                    if (currentRate.HasValue)
                    {
                        // 3. Crear el nuevo registro con la fecha de MAÑANA
                        var newRate = new TasaBCV
                        {
                            // Asumo que tu propiedad se llama 'Tasa' o 'Valor'. Usaré 'Tasa' si es más correcto.
                            Valor = currentRate.Value,
                            Fecha = tomorrow, // <-- Guardamos con la fecha de MAÑANA
                            FechaCreacion = DateTime.Now
                        };

                        // 4. Guardar en la base de datos
                        await tasaRepository.AddRateAsync(newRate);
                        _logger?.LogInformation($"Tasa de cambio guardada con éxito para {tomorrow:d}. Valor: {currentRate.Value:N2}");
                    }
                    else
                    {
                        _logger?.LogWarning($"No se pudo obtener la tasa de cambio del API para {tomorrow:d}. El servicio retornó un valor nulo.");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Error crítico al intentar guardar la tasa diaria para {tomorrow:d}.");
                }
            }
        }
    }
}