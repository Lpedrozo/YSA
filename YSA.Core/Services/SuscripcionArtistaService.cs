// YSA.Core.Services/SuscripcionArtistaService.cs
using YSA.Core.DTOs;
using YSA.Core.Entities;
using YSA.Core.Interfaces;

namespace YSA.Core.Services
{
    public class SuscripcionArtistaService : ISuscripcionArtistaService
    {
        private readonly ISuscripcionArtistaRepository _repository;
        private readonly IArtistaService _artistaService;
        private readonly IPlanSuscripcionService _planService;

        public SuscripcionArtistaService(
            ISuscripcionArtistaRepository repository,
            IArtistaService artistaService,
            IPlanSuscripcionService planService)
        {
            _repository = repository;
            _artistaService = artistaService;
            _planService = planService;
        }

        public async Task<List<SuscripcionArtistaDto>> GetAllSuscripcionesAsync()
        {
            var suscripciones = await _repository.GetAllAsync();
            return suscripciones.Select(MapToDto).ToList();
        }
        public async Task<bool> TieneSuscripcionActivaAsync(int artistaId)
        {
            var suscripcion = await _repository.GetActiveSubscriptionAsync(artistaId);
            return suscripcion != null;
        }
        public async Task<List<SuscripcionArtistaDto>> GetSuscripcionesPendientesValidacionAsync()
        {
            var suscripciones = await _repository.GetPendientesValidacionAsync();
            return suscripciones.Select(MapToDto).ToList();
        }

        public async Task<List<SuscripcionArtistaDto>> GetSuscripcionesByArtistaAsync(int artistaId)
        {
            var suscripciones = await _repository.GetByArtistaIdAsync(artistaId);
            return suscripciones.Select(MapToDto).ToList();
        }

        public async Task<SuscripcionArtistaDto> GetSuscripcionByIdAsync(int id)
        {
            var suscripcion = await _repository.GetByIdAsync(id);
            return suscripcion != null ? MapToDto(suscripcion) : null;
        }

        public async Task<SuscripcionArtistaDto> CreateSuscripcionAsync(int artistaId, int planId, string comprobanteUrl)
        {
            var plan = await _planService.GetPlanByIdAsync(planId);
            if (plan == null)
            {
                throw new InvalidOperationException("Plan no encontrado.");
            }

            var suscripcion = new SuscripcionArtista
            {
                ArtistaId = artistaId,
                PlanId = planId,
                SnapshotNombre = plan.Nombre,
                SnapshotPrecio = plan.Precio,
                SnapshotDuracionDias = plan.DuracionDias,
                SnapshotLimitePublicaciones = plan.LimitePublicaciones,
                SnapshotComisionPorcentaje = plan.ComisionPorcentaje,
                SnapshotTieneVisibilidadPrioritaria = plan.TieneVisibilidadPrioritaria,
                Estado = "PagadoValidacion",
                ComprobanteUrl = comprobanteUrl,
                FechaCreacion = DateTime.UtcNow,
                PublicacionesUsadas = 0,
                NotasAdmin = "Ninguna"
            };

            var created = await _repository.AddAsync(suscripcion);
            return MapToDto(created);
        }

        public async Task<bool> AprobarSuscripcionAsync(int id, int adminId, string notas)
        {
            var suscripcion = await _repository.GetByIdAsync(id);
            if (suscripcion == null) return false;

            var plan = await _planService.GetPlanByIdAsync(suscripcion.PlanId);
            if (plan == null) return false;

            suscripcion.Estado = "Activa";
            suscripcion.FechaInicio = DateTime.UtcNow;
            suscripcion.FechaFin = DateTime.UtcNow.AddDays(suscripcion.SnapshotDuracionDias);
            suscripcion.FechaPago = DateTime.UtcNow;
            suscripcion.ValidadoPorId = adminId;
            suscripcion.FechaValidacion = DateTime.UtcNow;
            suscripcion.NotasAdmin = notas;

            await _repository.UpdateAsync(suscripcion);
            return true;
        }
        // YSA.Core.Services/SuscripcionArtistaService.cs
        public async Task<bool> PuedeRenovarAsync(int artistaId, int? planId = null)
        {
            // Verificar si hay una suscripción activa (que no esté vencida)
            var suscripcionActiva = await _repository.GetActiveSubscriptionAsync(artistaId);

            if (suscripcionActiva != null)
            {
                // Si tiene una activa, solo puede renovar si está por vencer (menos de 15 días)
                var diasRestantes = (int)(suscripcionActiva.FechaFin - DateTime.UtcNow).TotalDays;
                return diasRestantes <= 15;
            }

            // Si no tiene activa, puede renovar/contratar
            return true;
        }

        public async Task<SuscripcionArtistaDto> RenovarSuscripcionAsync(int suscripcionAnteriorId, int planId, string comprobanteUrl)
        {
            var suscripcionAnterior = await _repository.GetByIdAsync(suscripcionAnteriorId);
            if (suscripcionAnterior == null)
            {
                throw new InvalidOperationException("Suscripción anterior no encontrada.");
            }

            var plan = await _planService.GetPlanByIdAsync(planId);
            if (plan == null)
            {
                throw new InvalidOperationException("Plan no encontrado.");
            }

            var nuevaSuscripcion = new SuscripcionArtista
            {
                ArtistaId = suscripcionAnterior.ArtistaId,
                PlanId = planId,
                SnapshotNombre = plan.Nombre,
                SnapshotPrecio = plan.Precio,
                SnapshotDuracionDias = plan.DuracionDias,
                SnapshotLimitePublicaciones = plan.LimitePublicaciones,
                SnapshotComisionPorcentaje = plan.ComisionPorcentaje,
                SnapshotTieneVisibilidadPrioritaria = plan.TieneVisibilidadPrioritaria,
                Estado = "PagadoValidacion",  // Pendiente de validación del admin
                ComprobanteUrl = comprobanteUrl,
                FechaCreacion = DateTime.UtcNow,
                PublicacionesUsadas = 0,
                NotasAdmin = $"Renovación de suscripción #{suscripcionAnteriorId}"
            };

            var created = await _repository.AddAsync(nuevaSuscripcion);
            return MapToDto(created);
        }
        public async Task<bool> RechazarSuscripcionAsync(int id, int adminId, string motivo)
        {
            var suscripcion = await _repository.GetByIdAsync(id);
            if (suscripcion == null) return false;

            suscripcion.Estado = "Cancelada";
            suscripcion.ValidadoPorId = adminId;
            suscripcion.FechaValidacion = DateTime.UtcNow;
            suscripcion.NotasAdmin = motivo;

            await _repository.UpdateAsync(suscripcion);
            return true;
        }

        public async Task<bool> CancelarSuscripcionAsync(int id, int adminId, string motivo)
        {
            var suscripcion = await _repository.GetByIdAsync(id);
            if (suscripcion == null) return false;

            suscripcion.Estado = "Cancelada";
            suscripcion.ValidadoPorId = adminId;
            suscripcion.FechaValidacion = DateTime.UtcNow;
            suscripcion.NotasAdmin = motivo;

            await _repository.UpdateAsync(suscripcion);
            return true;
        }

        public async Task<DashboardSuscripcionesDto> GetDashboardDataAsync()
        {
            var todas = await _repository.GetAllAsync();
            var pendientes = todas.Where(s => s.Estado == "PagadoValidacion").ToList();
            var activas = todas.Where(s => s.Estado == "Activa" && s.FechaFin > DateTime.UtcNow).ToList();
            var vencidas = todas.Where(s => s.Estado == "Vencida" || (s.Estado == "Activa" && s.FechaFin <= DateTime.UtcNow)).ToList();

            // Actualizar suscripciones vencidas
            foreach (var vencida in vencidas.Where(v => v.Estado == "Activa"))
            {
                vencida.Estado = "Vencida";
                await _repository.UpdateAsync(vencida);
            }

            return new DashboardSuscripcionesDto
            {
                TotalPendientes = pendientes.Count,
                TotalActivas = activas.Count,
                TotalVencidas = vencidas.Count,
                IngresosTotales = todas.Where(s => s.Estado == "Activa").Sum(s => s.SnapshotPrecio),
                PendientesValidacion = pendientes.Select(MapToDto).ToList(),
                Activas = activas.Select(MapToDto).ToList(),
                Historial = todas.OrderByDescending(s => s.FechaCreacion).Take(50).Select(MapToDto).ToList()
            };
        }

        private SuscripcionArtistaDto MapToDto(SuscripcionArtista s)
        {
            return new SuscripcionArtistaDto
            {
                Id = s.Id,
                ArtistaId = s.ArtistaId,
                ArtistaNombre = s.Artista?.Usuario?.Nombre + " " + s.Artista?.Usuario?.Apellido,
                ArtistaNombreArtistico = s.Artista?.NombreArtistico,
                ArtistaEmail = s.Artista?.Usuario?.Email,
                ArtistaUrlImagen = s.Artista?.Usuario?.UrlImagen,
                PlanId = s.PlanId,
                PlanNombre = s.Plan?.Nombre,
                SnapshotNombre = s.SnapshotNombre,
                SnapshotPrecio = s.SnapshotPrecio,
                SnapshotDuracionDias = s.SnapshotDuracionDias,
                SnapshotLimitePublicaciones = s.SnapshotLimitePublicaciones,
                SnapshotComisionPorcentaje = s.SnapshotComisionPorcentaje,
                SnapshotTieneVisibilidadPrioritaria = s.SnapshotTieneVisibilidadPrioritaria,
                FechaInicio = s.FechaInicio,
                FechaFin = s.FechaFin,
                Estado = s.Estado,
                ComprobanteUrl = s.ComprobanteUrl,
                FechaPago = s.FechaPago,
                ValidadoPorId = s.ValidadoPorId,
                ValidadoPorNombre = s.ValidadoPor?.Nombre + " " + s.ValidadoPor?.Apellido,
                FechaValidacion = s.FechaValidacion,
                NotasAdmin = s.NotasAdmin,
                FechaCreacion = s.FechaCreacion,
                PublicacionesUsadas = s.PublicacionesUsadas
            };
        }
    }
}