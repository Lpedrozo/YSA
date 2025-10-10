using System.Text.Json.Serialization;

namespace YSA.Core.DTOs
{
    public class ExchangeRateResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; set; }

        [JsonPropertyName("base_code")] // Se llama "base_code" en tu JSON, no solo "base"
        public string BaseCurrency { get; set; }

        [JsonPropertyName("time_last_update_unix")] // Asumiendo que quieres el timestamp
        public long UpdatedTimestamp { get; set; }

        // ESTE ES EL CAMBIO CLAVE: Coincidir con la propiedad "conversion_rates"
        [JsonPropertyName("conversion_rates")]
        public Dictionary<string, decimal> ConversionRates { get; set; } = new Dictionary<string, decimal>();
    }
}