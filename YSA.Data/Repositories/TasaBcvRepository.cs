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

            // 1. INTENTO DE BÚSQUEDA DEL DÍA ACTIVO
            // Esta búsqueda sigue siendo útil, ya que si hoy es Lunes, queremos la tasa del Lunes
            // (que fue guardada el domingo o el mismo lunes muy temprano).
            // O si es jueves, queremos la tasa del jueves (que fue guardada el miércoles).
            var activeRate = await _context.TasasBCV
                .AsNoTracking()
                .Where(t => t.Fecha.Date == today) // Busca solo la tasa con fecha de HOY
                .FirstOrDefaultAsync();

            if (activeRate != null)
            {
                return activeRate.Valor;
            }

            // 2. LÓGICA DE FALLBACK (Prioridad de búsqueda hacia el pasado)
            // Si no se encuentra una tasa con la fecha de hoy, buscamos hacia atrás.

            // Creamos una lista de posibles fechas de búsqueda hacia atrás
            var datesToSearch = new List<DateTime>();

            // Si hoy es Sábado o Domingo, el día más reciente publicado sería el Viernes.
            if (today.DayOfWeek == DayOfWeek.Saturday)
            {
                datesToSearch.Add(today.AddDays(-1)); // Viernes
                datesToSearch.Add(today.AddDays(-2)); // Jueves (Fallback)
            }
            else if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                datesToSearch.Add(today.AddDays(-2)); // Viernes
                datesToSearch.Add(today.AddDays(-3)); // Jueves (Fallback)
            }
            else // Para cualquier otro día (Lunes a Viernes), buscamos el día anterior inmediato.
            {
                datesToSearch.Add(today.AddDays(-1));
                datesToSearch.Add(today.AddDays(-2)); // Segundo día anterior como seguridad
            }

            // Hacemos la búsqueda retroactiva
            var fallbackRate = await _context.TasasBCV
                .AsNoTracking()
                .Where(t => datesToSearch.Contains(t.Fecha.Date))
                .OrderByDescending(t => t.Fecha) // La tasa más reciente de las fechas de búsqueda
                .FirstOrDefaultAsync();

            if (fallbackRate != null)
            {
                return fallbackRate.Valor;
            }


            // 3. FALLBACK GENERAL (Última Tasa Guardada)
            // Si ni la tasa de hoy ni la retroactiva funcionaron, devolver la última guardada, sin importar la fecha.
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
        public async Task<TasaBCV> GetLastRateAsync()
        {
            return await _context.TasasBCV
                .OrderByDescending(t => t.FechaCreacion) // Ordenar por la hora real de creación
                .FirstOrDefaultAsync();
        }

        // 2. Nuevo método para actualizar una tasa existente
        public async Task UpdateRateAsync(TasaBCV tasa)
        {
            // Adjuntar la entidad al contexto y marcarla como modificada, o solo usar SaveChanges si ya está adjunta
            _context.TasasBCV.Update(tasa);
            await _context.SaveChangesAsync();
        }
    }
}