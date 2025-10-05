using YSA.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
// using Microsoft.AspNetCore.Http; // Removido, IFormFile no debe estar en la capa Core/Servicios

public interface IRecursoActividadService
{
    // --- LÓGICA PARA RECURSOS (LECTURA/ADMIN) ---

    /// <summary>
    /// Obtiene todos los recursos y actividades para una entidad específica (Curso, Módulo o Lección).
    /// </summary>
    Task<List<RecursoActividad>> ObtenerRecursosDeEntidadAsync(string tipoEntidad, int entidadId);

    /// <summary>
    /// Obtiene todos los recursos/actividades ligados a un curso, incluyendo los de sus módulos/lecciones (debe implementarse la lógica de agregación).
    /// </summary>
    Task<List<RecursoActividad>> ObtenerRecursosPorCursoAsync(int cursoId);

    /// <summary>
    /// Obtiene un recurso/actividad por su ID.
    /// </summary>
    Task<RecursoActividad?> ObtenerRecursoPorIdAsync(int recursoActividadId);

    // --- LÓGICA PARA ENTREGAS (ESTUDIANTE) ---

    /// <summary>
    /// Obtiene la entrega de un estudiante para una actividad específica (si existe).
    /// </summary>
    Task<EntregaActividad?> ObtenerEntregaPorActividadYEstudianteAsync(int recursoActividadId, int estudianteId);

    /// <summary>
    /// Crea un nuevo registro de entrega de actividad.
    /// </summary>
    Task<bool> CrearEntregaAsync(int recursoActividadId, int estudianteId, string urlArchivo, string? comentario);

    /// <summary>
    /// Actualiza una entrega existente (reentrega) siempre y cuando no esté calificada.
    /// </summary>
    Task<bool> ActualizarEntregaExistenteAsync(int recursoActividadId, int estudianteId, string urlArchivo, string? comentario);

    // --- LÓGICA PARA INSTRUCTORES (CALIFICAR) ---

    /// <summary>
    /// Obtiene las actividades pendientes de revisión que corresponden a los cursos del instructor.
    /// </summary>
    Task<List<EntregaActividad>> ObtenerTareasPendientesParaInstructorAsync(int instructorId);

    /// <summary>
    /// Califica una entrega, valida la pertenencia y aplica la retroalimentación.
    /// </summary>
    Task<bool> CalificarEntregaAsync(int entregaId, int instructorId, decimal? calificacion, string observacion);

    // --- LÓGICA PARA RECURSOS (CRUD ADMIN) ---

    Task<RecursoActividad?> CrearRecursoActividadAsync(RecursoActividad recurso);
    Task<bool> ActualizarRecursoActividadAsync(RecursoActividad recurso, int recursoId, int instructorId);
    Task<bool> EliminarRecursoActividadAsync(int recursoActividadId, int instructorId);
    Task<List<RecursoActividad>> ObtenerRecursosPorEntidadesAsync(IEnumerable<int> entidadIds);

}