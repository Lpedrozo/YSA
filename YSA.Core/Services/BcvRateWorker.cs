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
        private readonly IExchangeRateService _exchangeService;
        private readonly ITasaBCVRepository _tasaRepository;
        private readonly ILogger<BcvRateWorker> _logger;

        private readonly IServiceProvider _serviceProvider;

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
                var nextRun = now.Date.AddDays(1).AddHours(2);
                var timeToWait = nextRun - now;

                if (timeToWait < TimeSpan.Zero)
                {
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

                var today = DateTime.Today;
                if (today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday)
                {
                    _logger?.LogInformation($"Hoy es {today.DayOfWeek}. No se consulta ni se guarda una nueva tasa.");
                    return;
                }

                var nextApplicableDate = today.AddDays(1);

                if (nextApplicableDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    nextApplicableDate = nextApplicableDate.AddDays(2);
                    _logger?.LogInformation("Detectado Viernes. La tasa actual aplicará el próximo Lunes.");
                }
                else if (nextApplicableDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    nextApplicableDate = nextApplicableDate.AddDays(1);
                    _logger?.LogInformation("Detectado Sábado. La tasa actual aplicará el próximo Lunes.");
                }

                try
                {
                    var lastSavedRate = await tasaRepository.GetLastRateAsync();
                    var currentRate = await exchangeService.GetVenezuelaRateAsync();
                    if (!currentRate.HasValue)
                    {
                        _logger?.LogWarning($"No se pudo obtener la tasa de cambio del API. El servicio retornó un valor nulo.");
                        return;
                    }

                    if (lastSavedRate != null && lastSavedRate.Valor == currentRate.Value)
                    {
                        _logger?.LogInformation($"La tasa obtenida ({currentRate.Value:N2}) es igual a la última guardada ({lastSavedRate.Valor:N2}). Saltando registro para evitar duplicación.");
                        return;
                    }

                    if (lastSavedRate != null && lastSavedRate.Fecha.Date == nextApplicableDate.Date)
                    {
                        lastSavedRate.Valor = currentRate.Value;
                        lastSavedRate.FechaCreacion = DateTime.Now; // Marcar con la hora de la corrección

                        await tasaRepository.UpdateRateAsync(lastSavedRate);
                        _logger?.LogInformation($"Tasa de cambio para {nextApplicableDate:d} ACTUAZLIZADA. Nuevo Valor: {currentRate.Value:N2} (Corrección/Tasa de la tarde).");
                    }
                    else
                    {
                        var existingRate = await tasaRepository.ExistsRateForDateAsync(nextApplicableDate, stoppingToken);

                        if (existingRate)
                        {
                            _logger?.LogInformation($"La tasa para {nextApplicableDate:d} existe pero no fue el último registro. Debe revisarse la lógica del Worker o la API.");
                            return;
                        }

                        var newRate = new TasaBCV
                        {
                            Valor = currentRate.Value,
                            Fecha = nextApplicableDate, 
                            FechaCreacion = DateTime.Now
                        };

                        await tasaRepository.AddRateAsync(newRate);
                        _logger?.LogInformation($"Tasa de cambio guardada con éxito como NUEVA para {nextApplicableDate:d}. Valor: {currentRate.Value:N2}");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Error crítico al intentar guardar/actualizar la tasa diaria para {nextApplicableDate:d}.");
                }
            }
        }
    }
}