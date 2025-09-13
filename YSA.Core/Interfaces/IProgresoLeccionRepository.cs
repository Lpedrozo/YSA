using YSA.Core.Entities;

namespace YSA.Core.Interfaces
{
    public interface IProgresoLeccionRepository
    {
        Task<ProgresoLeccion> GetProgresoLeccionAsync(int estudianteId, int leccionId);
        Task<List<ProgresoLeccion>> GetProgresoLeccionesByEstudianteAndCursoAsync(int estudianteId, int cursoId);
        Task AddAsync(ProgresoLeccion progreso);
        Task UpdateAsync(ProgresoLeccion progreso);
    }
}