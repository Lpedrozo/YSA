// YSA.Core.Interfaces/ISuscripcionArtistaService.cs
using YSA.Core.DTOs;

namespace YSA.Core.Interfaces
{
    public interface ISuscripcionArtistaService
    {
        Task<List<SuscripcionArtistaDto>> GetAllSuscripcionesAsync();
        Task<List<SuscripcionArtistaDto>> GetSuscripcionesPendientesValidacionAsync();
        Task<List<SuscripcionArtistaDto>> GetSuscripcionesByArtistaAsync(int artistaId);
        Task<SuscripcionArtistaDto> GetSuscripcionByIdAsync(int id);
        Task<SuscripcionArtistaDto> CreateSuscripcionAsync(int artistaId, int planId, string comprobanteUrl);
        Task<bool> AprobarSuscripcionAsync(int id, int adminId, string notas);
        Task<bool> RechazarSuscripcionAsync(int id, int adminId, string motivo);
        Task<bool> CancelarSuscripcionAsync(int id, int adminId, string motivo);
        Task<DashboardSuscripcionesDto> GetDashboardDataAsync();
        Task<bool> TieneSuscripcionActivaAsync(int artistaId);
        Task<bool> PuedeRenovarAsync(int artistaId, int? planId = null);
        Task<SuscripcionArtistaDto> RenovarSuscripcionAsync(int suscripcionAnteriorId, int planId, string comprobanteUrl);
    }

    public class DashboardSuscripcionesDto
    {
        public int TotalPendientes { get; set; }
        public int TotalActivas { get; set; }
        public int TotalVencidas { get; set; }
        public decimal IngresosTotales { get; set; }
        public List<SuscripcionArtistaDto> PendientesValidacion { get; set; }
        public List<SuscripcionArtistaDto> Activas { get; set; }
        public List<SuscripcionArtistaDto> Historial { get; set; }
    }
}