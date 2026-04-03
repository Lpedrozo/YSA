// YSA.Core.Interfaces/IPlanSuscripcionService.cs
using YSA.Core.DTOs;

namespace YSA.Core.Interfaces
{
    public interface IPlanSuscripcionService
    {
        Task<List<PlanSuscripcionDto>> GetAllPlanesAsync();
        Task<PlanSuscripcionDto> GetPlanByIdAsync(int id);
        Task<PlanSuscripcionDto> CreatePlanAsync(PlanSuscripcionDto planDto, int usuarioId);
        Task<PlanSuscripcionDto> UpdatePlanAsync(PlanSuscripcionDto planDto, int usuarioId);
        Task<bool> DeletePlanAsync(int id);
        Task<bool> TogglePlanStatusAsync(int id, int usuarioId);
        Task<List<PlanSuscripcionDto>> GetActivePlanesAsync();
    }
}