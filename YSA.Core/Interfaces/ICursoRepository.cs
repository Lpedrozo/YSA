using YSA.Core.Entities;
using YSA.Core.Enums;

namespace YSA.Core.Interfaces
{
    public interface ICursoRepository
    {
        Task<List<Curso>> GetAllDigitalsAsync();
        Task<List<Curso>> GetAllPresencialsAsync();
        Task<List<Curso>> GetAll();
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
        Task<int> GetTotalCursosAsync();
        Task AddClasePresencialAsync(ClasePresencial clase);
        Task<List<ClasePresencial>> GetClasesByCursoIdAsync(int cursoId);
        Task<ClasePresencial> GetClaseByIdAsync(int id);
        void UpdateClasePresencial(ClasePresencial clase);
        Task DeleteClasePresencialAsync(int id);

        Task AddInscripcionClaseAsync(InscripcionClase inscripcion);
        Task<InscripcionClase> GetInscripcionByClaseAndEstudianteAsync(int clasePresencialId, int estudianteId);
        Task<List<InscripcionClase>> GetInscripcionesByClaseIdAsync(int clasePresencialId);
        Task<List<InscripcionClase>> GetInscripcionesByEstudianteIdAsync(int estudianteId);
        Task<InscripcionClase> GetInscripcionByIdAsync(int id);
        void UpdateInscripcionClase(InscripcionClase inscripcion);
        Task DeleteInscripcionClaseAsync(int id);

        Task<List<Curso>> GetCursosByTipoAsync(TipoCurso tipo);
        Task AddCursoCategoriaAsync(CursoCategoria cursoCategoria);
        Task UpdateClasePresencialAsync(ClasePresencial clase);
        Task AddCursoCategoriasAsync(List<CursoCategoria> cursoCategorias);
        Task UpdateInscripcionClaseAsync(InscripcionClase inscripcion);
    }
}