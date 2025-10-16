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

        public ExchangeRateService(HttpClient httpClient, IConfiguration configuration, ITasaBCVRepository tasaBcvRepository)
        {
            _httpClient = httpClient;

            var apiConfig = configuration.GetSection("ExchangeRateApi");
            _apiKey = apiConfig["ApiKey"];
            _apiHost = apiConfig["ApiHost"];
            _tasaBcvRepository = tasaBcvRepository;

        }
        public async Task<IEnumerable<TasaBCV>> GetRateHistoryAsync()
        {
            return await _tasaBcvRepository.GetAllRatesAsync();
        }
        public async Task<decimal?> GetTasaToday()
        {
            var tasaLocal = await _tasaBcvRepository.GetCurrentActiveRateAsync();
            return tasaLocal;
        }

        public async Task<decimal?> GetVenezuelaRateAsync()
        {
            try
            {
                var requestUrl = "USD";

                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode(); 

                var content = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonSerializer.Deserialize<ExchangeRateResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true 
                });

                if (apiResponse?.ConversionRates != null && apiResponse.ConversionRates.TryGetValue("VES", out decimal tasa))
                {
                    tasa = Math.Truncate(tasa * 100) / 100;

                    return tasa;
                }

                return null; 
            }
            catch (HttpRequestException)
            {
                return null;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}