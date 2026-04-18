using YSA.Core.Entities;
using YSA.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace YSA.Core.Services
{
    public interface IEstudianteCursoService
    {
        Task<bool> TieneAccesoAlCursoAsync(int estudianteId, int cursoId);
        Task<List<int>> GetEstudianteCursoIdsAsync(int estudianteId);
        Task OtorgarAccesoAsync(int estudianteId, int cursoId);

    }
}