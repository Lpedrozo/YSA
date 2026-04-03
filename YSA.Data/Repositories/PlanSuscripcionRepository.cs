// YSA.Data.Repositories/PlanSuscripcionRepository.cs
using Microsoft.EntityFrameworkCore;
using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Data;

namespace YSA.Data.Repositories
{
    public class PlanSuscripcionRepository : IPlanSuscripcionRepository
    {
        private readonly ApplicationDbContext _context;

        public PlanSuscripcionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PlanSuscripcion>> GetAllAsync()
        {
            return await _context.PlanesSuscripcion
                .OrderBy(p => p.Orden)
                .ToListAsync();
        }

        public async Task<PlanSuscripcion> GetByIdAsync(int id)
        {
            return await _context.PlanesSuscripcion
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PlanSuscripcion> AddAsync(PlanSuscripcion plan)
        {
            await _context.PlanesSuscripcion.AddAsync(plan);
            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<PlanSuscripcion> UpdateAsync(PlanSuscripcion plan)
        {
            _context.PlanesSuscripcion.Update(plan);
            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var plan = await GetByIdAsync(id);
            if (plan == null) return false;

            _context.PlanesSuscripcion.Remove(plan);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PlanSuscripcion>> GetActiveAsync()
        {
            return await _context.PlanesSuscripcion
                .Where(p => p.Activo)
                .OrderBy(p => p.Orden)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PlanesSuscripcion.AnyAsync(p => p.Id == id);
        }

        public async Task<bool> ExistsNombreAsync(string nombre, int? excludeId = null)
        {
            var query = _context.PlanesSuscripcion.Where(p => p.Nombre == nombre);
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }
    }
}