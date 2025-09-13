using YSA.Core.Entities;
using YSA.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace YSA.Core.Services
{
    public interface IProgresoLeccionService
    {
        Task<List<ProgresoLeccion>> GetProgresoLeccionesPorEstudianteYCursoAsync(int estudianteId, int cursoId);
        Task<(bool success, string message)> MarcarLeccionComoCompletadaAsync(int leccionId, int estudianteId);
    }
}