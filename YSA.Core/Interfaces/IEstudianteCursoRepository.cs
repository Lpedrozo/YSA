using YSA.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Core.Interfaces
{
    public interface IEstudianteCursoRepository
    {
        Task<EstudianteCurso> AddAsync(EstudianteCurso estudianteCurso);
        Task<EstudianteCurso> GetByEstudianteIdAndCursoIdAsync(int estudianteId, int cursoId);
        Task<bool> ExisteAccesoAsync(int estudianteId, int cursoId);
    }
}