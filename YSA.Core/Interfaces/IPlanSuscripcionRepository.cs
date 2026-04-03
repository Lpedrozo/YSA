// YSA.Core.Interfaces/IPlanSuscripcionRepository.cs
using YSA.Core.Entities;

namespace YSA.Core.Interfaces
{
    public interface IPlanSuscripcionRepository
    {
        Task<List<PlanSuscripcion>> GetAllAsync();
        Task<PlanSuscripcion> GetByIdAsync(int id);
        Task<PlanSuscripcion> AddAsync(PlanSuscripcion plan);
        Task<PlanSuscripcion> UpdateAsync(PlanSuscripcion plan);
        Task<bool> DeleteAsync(int id);
        Task<List<PlanSuscripcion>> GetActiveAsync();
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsNombreAsync(string nombre, int? excludeId = null);
    }
}