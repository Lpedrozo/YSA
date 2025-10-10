using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace YSA.Data.Repositories
{
    public class TasaBCVRepository : ITasaBCVRepository
    {
        private readonly ApplicationDbContext _context;

        public TasaBCVRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsRateForDateAsync(DateTime date, CancellationToken cancellationToken)
        {
            return await _context.TasasBCV
                .AnyAsync(t => t.Fecha.Date == date.Date, cancellationToken);
        }

        public async Task AddRateAsync(TasaBCV tasa)
        {
            _context.TasasBCV.Add(tasa);
            await _context.SaveChangesAsync();
        }
        public async Task<decimal?> GetTodayRateAsync()
        {
            var tasa = await _context.TasasBCV
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Fecha == DateTime.Today);

            return tasa?.Valor;
        }
        public async Task<decimal?> GetCurrentActiveRateAsync()
        {
            var today = DateTime.Today.Date;

            // Busca la tasa con la fecha más cercana al día de hoy, priorizando fechas iguales o futuras cercanas
            var tasa = await _context.TasasBCV
                .AsNoTracking()
                .Where(t => t.Fecha.Date >= today) // Trae la tasa de hoy o de mañana
                .OrderBy(t => t.Fecha)             // Ordena para que la más cercana a hoy esté primero (hoy, luego mañana)
                .FirstOrDefaultAsync();

            if (tasa != null)
            {
                // Si encontramos la de hoy o la de mañana, la devolvemos.
                return tasa.Valor;
            }

            // Si no hay ninguna tasa para hoy o mañana, devuelve la última guardada como fallback.
            var lastRate = await _context.TasasBCV
                .AsNoTracking()
                .OrderByDescending(t => t.Fecha)
                .FirstOrDefaultAsync();

            return lastRate?.Valor;
        }
        public async Task<IEnumerable<TasaBCV>> GetAllRatesAsync()
        {
            return await _context.TasasBCV
                .AsNoTracking()
                .OrderByDescending(t => t.Fecha)
                .ToListAsync();
        }
    }
}