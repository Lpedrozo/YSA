using YSA.Core.Entities;
using YSA.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;

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
            return await _estudianteCursoRepository.ExisteAccesoAsync(estudianteId, cursoId);
        }
    }
}