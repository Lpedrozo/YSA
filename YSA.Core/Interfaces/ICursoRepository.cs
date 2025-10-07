using YSA.Core.Entities;

namespace YSA.Core.Interfaces
{
    public interface ICursoRepository
    {
        Task<List<Curso>> GetAllWithDetailsAsync();
        Task AddAsync(Curso curso);
        Task<Curso> GetByIdAsync(int id);
        Task UpdateAsync(Curso curso);
        Task DeleteAsync(int id);
        Task UpdateWithCategoriesAsync(Curso curso, int[] categoriasSeleccionadas);
        Task ActualizarCursoAsync(Curso curso);
        Task CrearResenaAsync(Resena resena);
        Task<List<Resena>> GetResenasPorCursoAsync(int cursoId);
        Task CrearPreguntaAsync(PreguntaRespuesta pregunta);
        Task<List<PreguntaRespuesta>> GetPreguntasPorCursoAsync(int cursoId);
        Task<List<Anuncio>> GetAnunciosPorCursoAsync(int cursoId);
        Task<Anuncio> GetAnuncioByIdAsync(int id);
        Task CrearAnuncioAsync(Anuncio anuncio);
        Task UpdateAnuncioAsync(Anuncio anuncio);
        Task DeleteAnuncioAsync(int id);
        Task<List<Curso>> GetCursosByEstudianteIdAsync(int estudianteId);
        Task<bool> ResponderPreguntaAsync(int preguntaId, string respuesta, int instructorId);
        Task<List<PreguntaRespuesta>> ObtenerPreguntasPendientesPorInstructorAsync(int instructorId);
        Task CrearAsociacionInstructor(int cursoId, int artistaId);
        Task<List<Artista>> ObtenerArtistasAsociadosACursoAsync(int cursoId);
        Task DesasociarArtistaACursoAsync(int cursoId, int instructorId);
    }
}