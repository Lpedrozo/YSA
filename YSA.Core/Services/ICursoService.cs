using System.Collections.Generic;
using System.Threading.Tasks;
using YSA.Core.DTOs;
using YSA.Core.Entities;

namespace YSA.Core.Services
{
    public interface ICursoService
    {

        Task<InscripcionClase> GetInscripcionByClaseAndEstudianteAsync(int clasePresencialId, int estudianteId);
        Task<List<Categoria>> ObtenerTodasLasCategoriasAsync();
        Task CrearCategoriaAsync(Categoria categoria);
        Task<Categoria> ObtenerCategoriaPorIdAsync(int id);
        Task ActualizarCategoriaAsync(Categoria categoria);
        Task EliminarCategoriaAsync(int id);
        Task<List<Curso>> ObtenerTodosLosCursosAsync();
        Task<List<Curso>> ObtenerTodosLosCursosDigitalesAsync();
        Task<List<Curso>> ObtenerTodosLosCursosPresencialesAsync();
        Task<Curso> ObtenerCursoPorIdAsync(int id);
        Task CrearCursoAsync(Curso curso, int[] categoriasSeleccionadas);
        Task ActualizarCursoAsync(Curso curso, int[] categoriasSeleccionadas);
        Task EliminarCursoAsync(int id);
        Task<ResultDto> DestacarCursoAsync(int cursoId);
        Task<ResultDto> QuitarDestacadoAsync(int cursoId);
        Task<ResultDto> RecomendarCursoAsync(int cursoId);
        Task<ResultDto> QuitarRecomendadoAsync(int cursoId);
        Task CrearResenaAsync(Resena resena);
        Task<List<Resena>> ObtenerResenasPorCursoAsync(int cursoId);
        Task CrearPreguntaAsync(int cursoId, int estudianteId, string pregunta);
        Task<List<PreguntaRespuesta>> ObtenerPreguntasPorCursoAsync(int cursoId);
        Task<List<Anuncio>> ObtenerAnunciosPorCursoAsync(int cursoId);
        Task CrearAnuncioAsync(Anuncio model);
        Task EditarAnuncioAsync(Anuncio model);
        Task EliminarAnuncioAsync(int id);
        Task<List<Curso>> ObtenerCursosDelEstudianteAsync(int estudianteId);
        Task AsociarArtistaACursoAsync(int cursoId, int instructorId);
        Task<bool> ResponderPreguntaAsync(int preguntaId, string respuesta, int instructorId);
        Task<List<PreguntaRespuesta>> ObtenerPreguntasPendientesParaInstructorAsync(int instructorId);
        Task<List<Artista>> ObtenerArtistasAsociadosACursoAsync(int cursoId);
        Task DesasociarArtistaACursoAsync(int cursoId, int instructorId);
        Task<int> GetTotalCursosAsync();
        Task<Curso> CrearCursoPresencialAsync(Curso curso, int[] categoriasSeleccionadas, List<ClasePresencial> clases);
        Task<List<ClasePresencial>> ObtenerClasesPorCursoIdAsync(int cursoId);
        Task<ClasePresencial> ObtenerClasePorIdAsync(int id);
        Task<bool> CrearClaseAsync(ClasePresencial clase);
        Task<bool> ActualizarClaseAsync(ClasePresencial clase);
        Task<bool> EliminarClaseAsync(int id);
        Task<bool> InscribirEstudianteAClaseAsync(int clasePresencialId, int estudianteId);
        Task<bool> CancelarInscripcionClaseAsync(int clasePresencialId, int estudianteId);
        Task<List<InscripcionClase>> ObtenerInscripcionesPorClaseIdAsync(int clasePresencialId);
        Task<List<InscripcionClase>> ObtenerInscripcionesPorEstudianteIdAsync(int estudianteId);
        Task<bool> ActualizarAsistenciaAsync(int inscripcionId, string estadoAsistencia);

        // Método para obtener cursos presenciales
        Task<List<Curso>> ObtenerCursosPresencialesAsync();
    }
}