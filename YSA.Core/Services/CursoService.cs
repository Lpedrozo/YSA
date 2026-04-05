using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YSA.Core.DTOs;
using YSA.Core.Entities;
using YSA.Core.Enums;
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
        public async Task<InscripcionClase> GetInscripcionByClaseAndEstudianteAsync(int clasePresencialId, int estudianteId)
        {
            return await _cursoRepository.GetInscripcionByClaseAndEstudianteAsync(clasePresencialId, estudianteId);
        }
        public async Task<int> GetTotalCursosAsync()
        {
            return await _cursoRepository.GetTotalCursosAsync();
        }

        // Métodos para Categorías
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

        // Métodos para Cursos
        public async Task<List<Curso>> ObtenerTodosLosCursosAsync()
        {
            return await _cursoRepository.GetAll();
        }
        public async Task<List<Curso>> ObtenerTodosLosCursosDigitalesAsync()
        {
            return await _cursoRepository.GetAllDigitalsAsync();
        }
        public async Task<List<Curso>> ObtenerTodosLosCursosPresencialesAsync()
        {
            return await _cursoRepository.GetAllPresencialsAsync();
        }



        public async Task<Curso> ObtenerCursoPorIdAsync(int id)
        {
            return await _cursoRepository.GetByIdAsync(id);
        }

        public async Task CrearCursoAsync(Curso curso, int[] categoriasSeleccionadas)
        {
            await _cursoRepository.AddAsync(curso);

            if (categoriasSeleccionadas != null && categoriasSeleccionadas.Any())
            {
                var cursoCategorias = categoriasSeleccionadas.Select(catId => new CursoCategoria { CursoId = curso.Id, CategoriaId = catId }).ToList();
                await _cursoRepository.AddCursoCategoriasAsync(cursoCategorias);
            }
        }

        public async Task ActualizarCursoAsync(Curso curso, int[] categoriasSeleccionadas)
        {
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
            return await _cursoRepository.GetResenasPorCursoAsync(cursoId);
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
            return await _cursoRepository.GetPreguntasPorCursoAsync(cursoId);
        }

        public async Task<List<Anuncio>> ObtenerAnunciosPorCursoAsync(int cursoId)
        {
            return await _cursoRepository.GetAnunciosPorCursoAsync(cursoId);
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
            await _cursoRepository.CrearAsociacionInstructor(cursoId, instructorId);
        }

        public async Task<bool> ResponderPreguntaAsync(int preguntaId, string respuesta, int instructorId)
        {
            if (string.IsNullOrWhiteSpace(respuesta))
            {
                return false;
            }
            return await _cursoRepository.ResponderPreguntaAsync(preguntaId, respuesta, instructorId);
        }

        public async Task<List<PreguntaRespuesta>> ObtenerPreguntasPendientesParaInstructorAsync(int instructorId)
        {
            return await _cursoRepository.ObtenerPreguntasPendientesPorInstructorAsync(instructorId);
        }

        public async Task<List<Artista>> ObtenerArtistasAsociadosACursoAsync(int cursoId)
        {
            return await _cursoRepository.ObtenerArtistasAsociadosACursoAsync(cursoId);
        }

        public Task DesasociarArtistaACursoAsync(int cursoId, int instructorId)
        {
            return _cursoRepository.DesasociarArtistaACursoAsync(cursoId, instructorId);
        }

        // ==================== NUEVOS MÉTODOS PARA CURSOS PRESENCIALES ====================

        public async Task<Curso> CrearCursoPresencialAsync(Curso curso, int[] categoriasSeleccionadas, List<ClasePresencial> clases)
        {
            curso.TipoCurso = TipoCurso.Presencial;
            curso.FechaPublicacion = DateTime.UtcNow;

            // Guardar curso
            await _cursoRepository.AddAsync(curso);

            // Asociar categorías
            if (categoriasSeleccionadas != null && categoriasSeleccionadas.Any())
            {
                var cursoCategorias = categoriasSeleccionadas.Select(catId => new CursoCategoria
                {
                    CursoId = curso.Id,
                    CategoriaId = catId
                }).ToList();
                await _cursoRepository.AddCursoCategoriasAsync(cursoCategorias);
            }

            // Crear las clases presenciales
            if (clases != null && clases.Any())
            {
                foreach (var clase in clases)
                {
                    clase.CursoId = curso.Id;
                    await _cursoRepository.AddClasePresencialAsync(clase);
                }
            }

            return curso;
        }

        public async Task<List<ClasePresencial>> ObtenerClasesPorCursoIdAsync(int cursoId)
        {
            return await _cursoRepository.GetClasesByCursoIdAsync(cursoId);
        }

        public async Task<ClasePresencial> ObtenerClasePorIdAsync(int id)
        {
            return await _cursoRepository.GetClaseByIdAsync(id);
        }

        public async Task<bool> CrearClaseAsync(ClasePresencial clase)
        {
            await _cursoRepository.AddClasePresencialAsync(clase);
            return true;
        }

        public async Task<bool> ActualizarClaseAsync(ClasePresencial clase)
        {
            await _cursoRepository.UpdateClasePresencialAsync(clase);
            return true;
        }

        public async Task<bool> EliminarClaseAsync(int id)
        {
            await _cursoRepository.DeleteClasePresencialAsync(id);
            return true;
        }

        public async Task<bool> InscribirEstudianteAClaseAsync(int clasePresencialId, int estudianteId)
        {
            // Verificar si ya está inscrito
            var existe = await _cursoRepository.GetInscripcionByClaseAndEstudianteAsync(clasePresencialId, estudianteId);
            if (existe != null)
                return false;

            var inscripcion = new InscripcionClase
            {
                ClasePresencialId = clasePresencialId,
                EstudianteId = estudianteId,
                FechaInscripcion = DateTime.UtcNow,
                EstadoAsistencia = "Pendiente"
            };

            await _cursoRepository.AddInscripcionClaseAsync(inscripcion);
            return true;
        }

        public async Task<bool> CancelarInscripcionClaseAsync(int clasePresencialId, int estudianteId)
        {
            var inscripcion = await _cursoRepository.GetInscripcionByClaseAndEstudianteAsync(clasePresencialId, estudianteId);
            if (inscripcion == null)
                return false;

            await _cursoRepository.DeleteInscripcionClaseAsync(inscripcion.Id);
            return true;
        }

        public async Task<List<InscripcionClase>> ObtenerInscripcionesPorClaseIdAsync(int clasePresencialId)
        {
            return await _cursoRepository.GetInscripcionesByClaseIdAsync(clasePresencialId);
        }

        public async Task<List<InscripcionClase>> ObtenerInscripcionesPorEstudianteIdAsync(int estudianteId)
        {
            return await _cursoRepository.GetInscripcionesByEstudianteIdAsync(estudianteId);
        }

        public async Task<bool> ActualizarAsistenciaAsync(int inscripcionId, string estadoAsistencia)
        {
            var inscripcion = await _cursoRepository.GetInscripcionByIdAsync(inscripcionId);
            if (inscripcion == null)
                return false;

            inscripcion.EstadoAsistencia = estadoAsistencia;
            inscripcion.FechaConfirmacion = estadoAsistencia == "Asistio" ? DateTime.UtcNow : inscripcion.FechaConfirmacion;

            await _cursoRepository.UpdateInscripcionClaseAsync(inscripcion);
            return true;
        }

        public async Task<List<Curso>> ObtenerCursosPresencialesAsync()
        {
            return await _cursoRepository.GetCursosByTipoAsync(TipoCurso.Presencial);
        }
    }
}