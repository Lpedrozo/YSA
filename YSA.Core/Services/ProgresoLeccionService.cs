using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using YSA.Core.DTOs;
using YSA.Core.Entities;
using YSA.Core.Interfaces;

namespace YSA.Core.Services
{
    public class ProgresoLeccionService : IProgresoLeccionService
    {
        private readonly IProgresoLeccionRepository _progresoLeccionRepository;
        private readonly ILeccionRepository _leccionRepository; // Necesitas un repo de Lecciones para validar

        public ProgresoLeccionService(IProgresoLeccionRepository progresoLeccionRepository, ILeccionRepository leccionRepository)
        {
            _progresoLeccionRepository = progresoLeccionRepository;
            _leccionRepository = leccionRepository;
        }

        public async Task<List<ProgresoLeccion>> GetProgresoLeccionesPorEstudianteYCursoAsync(int estudianteId, int cursoId)
        {
            return await _progresoLeccionRepository.GetProgresoLeccionesByEstudianteAndCursoAsync(estudianteId, cursoId);
        }

        public async Task<(bool success, string message)> MarcarLeccionComoCompletadaAsync(int leccionId, int estudianteId)
        {
            var leccion = await _leccionRepository.GetByIdAsync(leccionId);
            if (leccion == null)
            {
                return (false, "Lección no encontrada.");
            }

            var progreso = await _progresoLeccionRepository.GetProgresoLeccionAsync(estudianteId, leccionId);

            if (progreso == null)
            {
                // Si no existe, crea un nuevo registro
                progreso = new ProgresoLeccion
                {
                    EstudianteId = estudianteId,
                    LeccionId = leccionId,
                    Completado = true,
                    FechaCompletado = DateTime.UtcNow
                };
                await _progresoLeccionRepository.AddAsync(progreso);
            }
            else
            {
                // Si ya existe, actualiza el estado si es necesario
                if (!progreso.Completado)
                {
                    progreso.Completado = true;
                    progreso.FechaCompletado = DateTime.UtcNow;
                    await _progresoLeccionRepository.UpdateAsync(progreso);
                }
            }
            return (true, "Progreso de lección actualizado.");
        }
    }
}