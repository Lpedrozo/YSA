using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YSA.Core.DTOs;
using YSA.Core.Entities;
using YSA.Core.Interfaces;

namespace YSA.Core.Services
{
    public class CursoService : ICursoService
    {
        private readonly ICursoRepository _cursoRepository;
        private readonly ICategoriaRepository _categoriaRepository;

        public CursoService(ICursoRepository cursoRepository, ICategoriaRepository categoriaRepository)
        {
            _cursoRepository = cursoRepository;
            _categoriaRepository = categoriaRepository;
        }

        // Métodos para Categorías (sin cambios)
        public async Task<List<Categoria>> ObtenerTodasLasCategoriasAsync()
        {
            return await _categoriaRepository.GetAllAsync();
        }

        public async Task CrearCategoriaAsync(Categoria categoria)
        {
            await _categoriaRepository.AddAsync(categoria);
        }

        public async Task<Categoria> ObtenerCategoriaPorIdAsync(int id)
        {
            return await _categoriaRepository.GetByIdAsync(id);
        }

        public async Task ActualizarCategoriaAsync(Categoria categoria)
        {
            await _categoriaRepository.UpdateAsync(categoria);
        }

        public async Task EliminarCategoriaAsync(int id)
        {
            await _categoriaRepository.DeleteAsync(id);
        }

        // Implementación de métodos para Cursos
        public async Task<List<Curso>> ObtenerTodosLosCursosAsync()
        {
            return await _cursoRepository.GetAllWithDetailsAsync();
        }

        public async Task<Curso> ObtenerCursoPorIdAsync(int id)
        {
            return await _cursoRepository.GetByIdAsync(id);
        }

        public async Task CrearCursoAsync(Curso curso, int[] categoriasSeleccionadas)
        {
            // El repositorio se encarga de la lógica de guardado, incluyendo las relaciones
            await _cursoRepository.AddAsync(curso);

            if (categoriasSeleccionadas != null && categoriasSeleccionadas.Any())
            {
                var cursoCategorias = categoriasSeleccionadas.Select(catId => new CursoCategoria { CursoId = curso.Id, CategoriaId = catId }).ToList();
                // Aquí podrías agregar un método en el repositorio para añadir las relaciones
                // Por simplicidad, se puede asumir que el AddAsync del curso con las propiedades de navegación ya las maneja
            }
        }

        public async Task ActualizarCursoAsync(Curso curso, int[] categoriasSeleccionadas)
        {
            // La lógica compleja de transacciones y relaciones ahora se maneja en el repositorio
            await _cursoRepository.UpdateWithCategoriesAsync(curso, categoriasSeleccionadas);
        }

        public async Task EliminarCursoAsync(int id)
        {
            await _cursoRepository.DeleteAsync(id);
        }
        public async Task<ResultDto> DestacarCursoAsync(int cursoId)
        {
            var curso = await _cursoRepository.GetByIdAsync(cursoId);
            if (curso == null)
            {
                return new ResultDto { Success = false, Message = "Curso no encontrado." };
            }

            if (curso.EsRecomendado)
            {
                return new ResultDto { Success = false, Message = "No se puede destacar un curso que ya está recomendado." };
            }

            curso.EsDestacado = true;
            await _cursoRepository.ActualizarCursoAsync(curso);
            return new ResultDto { Success = true, Message = "Curso destacado con éxito." };
        }

        public async Task<ResultDto> QuitarDestacadoAsync(int cursoId)
        {
            var curso = await _cursoRepository.GetByIdAsync(cursoId);
            if (curso == null)
            {
                return new ResultDto { Success = false, Message = "Curso no encontrado." };
            }

            curso.EsDestacado = false;
            await _cursoRepository.ActualizarCursoAsync(curso);
            return new ResultDto { Success = true, Message = "Se ha quitado el estado de destacado al curso." };
        }

        public async Task<ResultDto> RecomendarCursoAsync(int cursoId)
        {
            var curso = await _cursoRepository.GetByIdAsync(cursoId);
            if (curso == null)
            {
                return new ResultDto { Success = false, Message = "Curso no encontrado." };
            }

            if (curso.EsDestacado)
            {
                return new ResultDto { Success = false, Message = "No se puede recomendar un curso que ya está destacado." };
            }

            curso.EsRecomendado = true;
            await _cursoRepository.ActualizarCursoAsync(curso);
            return new ResultDto { Success = true, Message = "Curso recomendado con éxito." };
        }

        public async Task<ResultDto> QuitarRecomendadoAsync(int cursoId)
        {
            var curso = await _cursoRepository.GetByIdAsync(cursoId);
            if (curso == null)
            {
                return new ResultDto { Success = false, Message = "Curso no encontrado." };
            }

            curso.EsRecomendado = false;
            await _cursoRepository.ActualizarCursoAsync(curso);
            return new ResultDto { Success = true, Message = "Se ha quitado el estado de recomendado al curso." };
        }
        public async Task CrearResenaAsync(Resena resena)
        {
            await _cursoRepository.CrearResenaAsync(resena);
        }

        public async Task<List<Resena>> ObtenerResenasPorCursoAsync(int cursoId)
        {
            var resenas = await _cursoRepository.GetResenasPorCursoAsync(cursoId);
            return resenas;
        }
        public async Task CrearPreguntaAsync(int cursoId, int estudianteId, string pregunta)
        {
            var nuevaPregunta = new PreguntaRespuesta
            {
                CursoId = cursoId,
                EstudianteId = estudianteId,
                Pregunta = pregunta,
                Respuesta = "Gracias por preguntar. Ahora, espera la respuesta de tus instructores, ellos no tardan en responder.",
                FechaPregunta = DateTime.UtcNow,
                FechaRespuesta = DateTime.UtcNow
            };
            await _cursoRepository.CrearPreguntaAsync(nuevaPregunta);
        }

        public async Task<List<PreguntaRespuesta>> ObtenerPreguntasPorCursoAsync(int cursoId)
        {
            var preguntas = await _cursoRepository.GetPreguntasPorCursoAsync(cursoId);

            return preguntas;
        }
        public async Task<List<Anuncio>> ObtenerAnunciosPorCursoAsync(int cursoId)
        {
            var anuncios = await _cursoRepository.GetAnunciosPorCursoAsync(cursoId);
            return anuncios;
        }

        public async Task CrearAnuncioAsync(Anuncio model)
        {
            var anuncio = new Anuncio
            {
                CursoId = model.CursoId,
                Titulo = model.Titulo,
                Contenido = model.Contenido,
                FechaPublicacion = DateTime.UtcNow
            };
            await _cursoRepository.CrearAnuncioAsync(anuncio);
        }

        public async Task EditarAnuncioAsync(Anuncio model)
        {
            var anuncio = await _cursoRepository.GetAnuncioByIdAsync(model.Id);
            if (anuncio != null)
            {
                anuncio.Titulo = model.Titulo;
                anuncio.Contenido = model.Contenido;
                await _cursoRepository.UpdateAnuncioAsync(anuncio);
            }
        }

        public async Task EliminarAnuncioAsync(int id)
        {
            await _cursoRepository.DeleteAnuncioAsync(id);
        }
        public async Task<List<Curso>> ObtenerCursosDelEstudianteAsync(int estudianteId)
        {
            return await _cursoRepository.GetCursosByEstudianteIdAsync(estudianteId);
        }
        public async Task AsociarArtistaACursoAsync(int cursoId, int instructorId)
        {
            var curso = await _cursoRepository.GetByIdAsync(cursoId);
            if (curso == null)
            {
                throw new InvalidOperationException("Curso no encontrado.");
            }

            // Asigna el nuevo InstructorId
            curso.InstructorId = instructorId;
            await _cursoRepository.UpdateAsync(curso);
        }
    }
}