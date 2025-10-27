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
            var searchDate = today;

            if (today.DayOfWeek == DayOfWeek.Saturday)
            {
                searchDate = today.AddDays(2);
            }
            else if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                searchDate = today.AddDays(1);
            }
            var activeRate = await _context.TasasBCV
                .AsNoTracking()
                .Where(t => t.Fecha.Date == searchDate)
                .FirstOrDefaultAsync();

            if (activeRate != null)
            {
                return activeRate.Valor;
            }

            var lastPublishedRate = await _context.TasasBCV
                .AsNoTracking()
                .Where(t => t.Fecha.Date < today)
                .OrderByDescending(t => t.Fecha)
                .FirstOrDefaultAsync();

            if (lastPublishedRate != null)
            {
                return lastPublishedRate.Valor;
            }

            return null;
        }
        public async Task<IEnumerable<TasaBCV>> GetAllRatesAsync()
        {
            return await _context.TasasBCV
                .AsNoTracking()
                .OrderByDescending(t => t.Fecha)
                .ToListAsync();
        }
        public async Task<TasaBCV> GetLastRateAsync()
        {
            return await _context.TasasBCV
                .OrderByDescending(t => t.FechaCreacion) 
                .FirstOrDefaultAsync();
        }

        public async Task UpdateRateAsync(TasaBCV tasa)
        {
            _context.TasasBCV.Update(tasa);
            await _context.SaveChangesAsync();
        }
    }
}