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
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace YSA.Core.Services
{
    public interface IExchangeRateService
    {
        Task<decimal?> GetVenezuelaRateAsync();
        Task<decimal?> GetTasaToday();
        Task<IEnumerable<TasaBCV>> GetRateHistoryAsync();

    }
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiHost;
        private readonly ITasaBCVRepository _tasaBcvRepository;

        // Inyectamos HttpClient y la configuración para obtener la API Key
        public ExchangeRateService(HttpClient httpClient, IConfiguration configuration, ITasaBCVRepository tasaBcvRepository)
        {
            _httpClient = httpClient;

            // Cargar la configuración
            var apiConfig = configuration.GetSection("ExchangeRateApi");
            _apiKey = apiConfig["ApiKey"];
            _apiHost = apiConfig["ApiHost"];
            _tasaBcvRepository = tasaBcvRepository;

            // Configurar el HttpClient con los Headers comunes de RapidAPI
        }
        public async Task<IEnumerable<TasaBCV>> GetRateHistoryAsync()
        {
            // El servicio llama al repositorio para obtener los datos.
            return await _tasaBcvRepository.GetAllRatesAsync();
        }
        public async Task<decimal?> GetTasaToday()
        {
            // Llama al nuevo método que busca HOY o MAÑANA en la DB
            var tasaLocal = await _tasaBcvRepository.GetCurrentActiveRateAsync();

            // Si el valor es null, devolvemos null. Si tiene valor, lo devolvemos.
            return tasaLocal;
        }

        public async Task<decimal?> GetVenezuelaRateAsync()
        {
            try
            {
                // La URL base ya la configuramos en Program.cs o en appsettings.json si solo la pasamos como BaseAddress
                // Si la BaseUrl es completa (como en tu appsettings.json de ejemplo), podemos usarla directamente:
                var requestUrl = "USD"; // O simplemente una cadena vacía si la URL es completa.

                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode(); // Lanza una excepción si el código de estado es de error (4xx o 5xx)

                var content = await response.Content.ReadAsStringAsync();

                // Usamos el deserializador moderno de .NET
                var apiResponse = JsonSerializer.Deserialize<ExchangeRateResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Esto ayuda a mapear aunque el JSON no use la misma convención
                });

                if (apiResponse?.ConversionRates != null && apiResponse.ConversionRates.TryGetValue("VES", out decimal tasa))
                {
                    // Truncar o redondear, si es necesario, como en tu lógica original
                    tasa = Math.Truncate(tasa * 100) / 100; // Dos decimales

                    return tasa;
                }

                return null; // El formato de respuesta no fue el esperado o no incluye VES
            }
            catch (HttpRequestException)
            {
                // Error de red, timeout, o código de estado HTTP 4xx/5xx
                return null;
            }
            catch (JsonException)
            {
                // Error de deserialización (la API cambió su formato)
                return null;
            }
        }
    }
}