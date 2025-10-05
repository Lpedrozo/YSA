using YSA.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IRecursoActividadRepository
{
    // --- MÉTODOS PARA RECURSOS (CRUD) ---

    /// <summary>
    /// Obtiene todos los recursos/actividades asociados a una entidad (Curso, Módulo o Lección).
    /// </summary>
    Task<List<RecursoActividad>> ObtenerRecursosPorEntidadAsync(string tipoEntidad, int entidadId);

    /// <summary>
    /// Obtiene un recurso/actividad por su ID.
    /// </summary>
    Task<RecursoActividad?> ObtenerRecursoPorIdAsync(int recursoActividadId);

    /// <summary>
    /// Crea un nuevo recurso/actividad.
    /// </summary>
    Task<RecursoActividad> CrearRecursoActividadAsync(RecursoActividad recurso);

    /// <summary>
    /// Actualiza un recurso/actividad existente.
    /// </summary>
    Task<bool> ActualizarRecursoActividadAsync(RecursoActividad recurso);

    /// <summary>
    /// Elimina un recurso/actividad por su ID.
    /// </summary>
    Task<bool> EliminarRecursoActividadAsync(int recursoActividadId);

    // --- MÉTODOS PARA ENTREGAS (CRUD) ---

    /// <summary>
    /// Crea un nuevo registro de entrega de actividad por parte de un estudiante.
    /// </summary>
    Task<EntregaActividad> CrearEntregaAsync(EntregaActividad entrega);

    /// <summary>
    /// Obtiene la entrega de un estudiante para una actividad específica (si existe).
    /// </summary>
    Task<EntregaActividad?> ObtenerEntregaPorRecursoYEstudianteAsync(int recursoActividadId, int estudianteId);

    /// <summary>
    /// Obtiene la entrega por su ID.
    /// </summary>
    Task<EntregaActividad?> ObtenerEntregaPorIdAsync(int entregaId);

    /// <summary>
    /// Actualiza la entidad de entrega (usada para reentrega y para calificación).
    /// </summary>
    Task<bool> ActualizarEntregaAsync(EntregaActividad entrega);

    // --- MÉTODOS PARA CONSULTAS COMPLEJAS ---

    /// <summary>
    /// Obtiene todas las entregas pendientes de revisión para los cursos de un instructor específico.
    /// </summary>
    Task<List<EntregaActividad>> ObtenerEntregasPendientesPorInstructorAsync(int instructorId);

    /// <summary>
    /// Actualiza una entrega con la calificación y observación del instructor.
    /// </summary>
    Task<bool> ActualizarEntregaConCalificacionAsync(int entregaId, int instructorId, decimal? calificacion, string observacion);
    Task<List<RecursoActividad>> ObtenerRecursosPorEntidadesAsync(IEnumerable<int> entidadIds);

}