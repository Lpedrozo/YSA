// YSA.Core.Services/PlanSuscripcionService.cs
using YSA.Core.DTOs;
using YSA.Core.Entities;
using YSA.Core.Interfaces;

namespace YSA.Core.Services
{
    public class PlanSuscripcionService : IPlanSuscripcionService
    {
        private readonly IPlanSuscripcionRepository _repository;

        public PlanSuscripcionService(IPlanSuscripcionRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<PlanSuscripcionDto>> GetAllPlanesAsync()
        {
            var planes = await _repository.GetAllAsync();
            return planes.Select(MapToDto).ToList();
        }

        public async Task<PlanSuscripcionDto> GetPlanByIdAsync(int id)
        {
            var plan = await _repository.GetByIdAsync(id);
            return plan != null ? MapToDto(plan) : null;
        }

        public async Task<PlanSuscripcionDto> CreatePlanAsync(PlanSuscripcionDto planDto, int usuarioId)
        {
            // Validar nombre único
            if (await _repository.ExistsNombreAsync(planDto.Nombre))
            {
                throw new InvalidOperationException($"Ya existe un plan con el nombre '{planDto.Nombre}'.");
            }

            var plan = new PlanSuscripcion
            {
                Nombre = planDto.Nombre,
                Descripcion = planDto.Descripcion,
                Precio = planDto.Precio,
                DuracionDias = planDto.DuracionDias,
                LimitePublicaciones = planDto.LimitePublicaciones,
                ComisionPorcentaje = planDto.ComisionPorcentaje,
                TieneVisibilidadPrioritaria = planDto.TieneVisibilidadPrioritaria,
                Orden = planDto.Orden,
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                PermitePromocionesExtras = planDto.PermitePromocionesExtras,
                MaxPromocionesSimultaneas = planDto.MaxPromocionesSimultaneas,
                DescuentoComisionAdicional = planDto.DescuentoComisionAdicional,
                ModificadoPorId = usuarioId,
                FechaModificacion = DateTime.UtcNow
            };

            var created = await _repository.AddAsync(plan);
            return MapToDto(created);
        }

        public async Task<PlanSuscripcionDto> UpdatePlanAsync(PlanSuscripcionDto planDto, int usuarioId)
        {
            var plan = await _repository.GetByIdAsync(planDto.Id);
            if (plan == null)
            {
                throw new InvalidOperationException("Plan no encontrado.");
            }

            // Validar nombre único (excluyendo el actual)
            if (await _repository.ExistsNombreAsync(planDto.Nombre, planDto.Id))
            {
                throw new InvalidOperationException($"Ya existe un plan con el nombre '{planDto.Nombre}'.");
            }

            plan.Nombre = planDto.Nombre;
            plan.Descripcion = planDto.Descripcion;
            plan.Precio = planDto.Precio;
            plan.DuracionDias = planDto.DuracionDias;
            plan.LimitePublicaciones = planDto.LimitePublicaciones;
            plan.ComisionPorcentaje = planDto.ComisionPorcentaje;
            plan.TieneVisibilidadPrioritaria = planDto.TieneVisibilidadPrioritaria;
            plan.Orden = planDto.Orden;
            plan.PermitePromocionesExtras = planDto.PermitePromocionesExtras;
            plan.MaxPromocionesSimultaneas = planDto.MaxPromocionesSimultaneas;
            plan.DescuentoComisionAdicional = planDto.DescuentoComisionAdicional;
            plan.ModificadoPorId = usuarioId;
            plan.FechaModificacion = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(plan);
            return MapToDto(updated);
        }

        public async Task<bool> DeletePlanAsync(int id)
        {
            var plan = await _repository.GetByIdAsync(id);
            if (plan == null)
            {
                return false;
            }

            // Verificar si tiene suscripciones activas
            // Nota: Necesitarías un repositorio de SuscripcionesArtistas para esto
            // Por ahora solo eliminamos si no tiene suscripciones

            return await _repository.DeleteAsync(id);
        }

        public async Task<bool> TogglePlanStatusAsync(int id, int usuarioId)
        {
            var plan = await _repository.GetByIdAsync(id);
            if (plan == null)
            {
                return false;
            }

            plan.Activo = !plan.Activo;
            plan.ModificadoPorId = usuarioId;
            plan.FechaModificacion = DateTime.UtcNow;

            await _repository.UpdateAsync(plan);
            return true;
        }

        public async Task<List<PlanSuscripcionDto>> GetActivePlanesAsync()
        {
            var planes = await _repository.GetActiveAsync();
            return planes.Select(MapToDto).ToList();
        }

        // Mapeo de entidad a DTO
        private PlanSuscripcionDto MapToDto(PlanSuscripcion plan)
        {
            return new PlanSuscripcionDto
            {
                Id = plan.Id,
                Nombre = plan.Nombre,
                Descripcion = plan.Descripcion,
                Precio = plan.Precio,
                DuracionDias = plan.DuracionDias,
                LimitePublicaciones = plan.LimitePublicaciones,
                ComisionPorcentaje = plan.ComisionPorcentaje,
                TieneVisibilidadPrioritaria = plan.TieneVisibilidadPrioritaria,
                Orden = plan.Orden,
                Activo = plan.Activo,
                FechaCreacion = plan.FechaCreacion,
                FechaModificacion = plan.FechaModificacion,
                ModificadoPorId = plan.ModificadoPorId,
                PermitePromocionesExtras = plan.PermitePromocionesExtras,
                MaxPromocionesSimultaneas = plan.MaxPromocionesSimultaneas,
                DescuentoComisionAdicional = plan.DescuentoComisionAdicional
            };
        }
    }
}