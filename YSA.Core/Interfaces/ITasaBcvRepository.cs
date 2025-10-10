using YSA.Core.Entities;

namespace YSA.Core.Interfaces
{
    public interface ITasaBCVRepository
    {
        Task<bool> ExistsRateForDateAsync(DateTime date, CancellationToken cancellationToken);

        Task AddRateAsync(TasaBCV tasa);
        Task<decimal?> GetTodayRateAsync();
        Task<decimal?> GetCurrentActiveRateAsync();
        Task<IEnumerable<TasaBCV>> GetAllRatesAsync();

    }
}