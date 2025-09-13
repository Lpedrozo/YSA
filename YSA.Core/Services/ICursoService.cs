using System.Collections.Generic;
using System.Threading.Tasks;
using YSA.Core.DTOs;
using YSA.Core.Entities;

namespace YSA.Core.Services
{
    public interface ICursoService
    {
        Task<List<Categoria>> ObtenerTodasLasCategoriasAsync();
        Task CrearCategoriaAsync(Categoria categoria);
        Task<Categoria> ObtenerCategoriaPorIdAsync(int id);
        Task ActualizarCategoriaAsync(Categoria categoria);
        Task EliminarCategoriaAsync(int id);
        Task<List<Curso>> ObtenerTodosLosCursosAsync();
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
    }
}