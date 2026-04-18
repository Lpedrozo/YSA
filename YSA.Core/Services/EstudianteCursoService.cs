using System.Collections.Generic;
using System.Threading.Tasks;
using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Repositories;

namespace YSA.Core.Services
{
    public class EstudianteCursoService : IEstudianteCursoService
    {
        private readonly IEstudianteCursoRepository _estudianteCursoRepository;

        public EstudianteCursoService(IEstudianteCursoRepository estudianteCursoRepository)
        {
            _estudianteCursoRepository = estudianteCursoRepository;
        }

        public async Task<bool> TieneAccesoAlCursoAsync(int estudianteId, int cursoId)
        {
            return await _estudianteCursoRepository.TieneAccesoAlCursoAsync(estudianteId, cursoId);
        }

        public async Task OtorgarAccesoAsync(int estudianteId, int cursoId)
        {
            var existe = await _estudianteCursoRepository.TieneAccesoAlCursoAsync(estudianteId, cursoId);
            if (!existe)
            {
                var estudianteCurso = new EstudianteCurso
                {
                    EstudianteId = estudianteId,
                    CursoId = cursoId,
                    FechaAccesoOtorgado = System.DateTime.UtcNow
                };
                await _estudianteCursoRepository.AddAsync(estudianteCurso);
            }
        }

        public async Task<List<int>> GetEstudianteCursoIdsAsync(int estudianteId)
        {
            return await _estudianteCursoRepository.GetEstudianteCursoIdsAsync(estudianteId);
        }
    }
}