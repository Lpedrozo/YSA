using YSA.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using YSA.Core.Interfaces;

public class RecursoActividadService : IRecursoActividadService
{
    private readonly IRecursoActividadRepository _recursoRepo;
    private readonly ICursoRepository _cursoRepo;

    public RecursoActividadService(IRecursoActividadRepository recursoRepo, ICursoRepository cursoRepo)
    {
        _recursoRepo = recursoRepo;
        _cursoRepo = cursoRepo;
    }

    // --- LÓGICA PARA RECURSOS (LECTURA) ---

    // Este método está duplicado por ObtenerRecursosPorCursoAsync, lo mantengo por si se usa en otro lugar.
    public async Task<List<RecursoActividad>> ObtenerRecursosDeEntidadAsync(string tipoEntidad, int entidadId)
    {
        if (string.IsNullOrWhiteSpace(tipoEntidad) || entidadId <= 0)
        {
            return new List<RecursoActividad>();
        }
        return await _recursoRepo.ObtenerRecursosPorEntidadAsync(tipoEntidad, entidadId);
    }

    // Usado en CursoController.VerCurso
    public async Task<List<RecursoActividad>> ObtenerRecursosPorCursoAsync(int cursoId)
    {
        // NOTA: Asumiendo que ObtenerRecursosPorEntidadAsync('Curso', cursoId) trae todas
        // las actividades/recursos directamente ligados al curso.
        return await _recursoRepo.ObtenerRecursosPorEntidadAsync("Curso", cursoId);
    }

    // Usado en CursoController.VerCurso y lógica de negocio
    public async Task<RecursoActividad?> ObtenerRecursoPorIdAsync(int recursoActividadId)
    {
        return await _recursoRepo.ObtenerRecursoPorIdAsync(recursoActividadId);
    }

    // Usado en CursoController.VerCurso y SubirEntregaActividad
    public async Task<EntregaActividad?> ObtenerEntregaPorActividadYEstudianteAsync(int recursoActividadId, int estudianteId)
    {
        return await _recursoRepo.ObtenerEntregaPorRecursoYEstudianteAsync(recursoActividadId, estudianteId);
    }

    // --- LÓGICA PARA ENTREGAS (ESTUDIANTE) ---

    // ¡IMPORTANTE! Se ha eliminado el método 'ProcesarEntregaAsync' y se han mantenido
    // CrearEntregaAsync y ActualizarEntregaExistenteAsync, ya que el controlador
    // ahora maneja la lógica de "crear si no existe" o "actualizar si existe".

    // Usado por CursoController.SubirEntregaActividad (Creación)
    public async Task<bool> CrearEntregaAsync(int recursoActividadId, int estudianteId, string urlArchivo, string? comentario)
    {
        // Validación de existencia y requerimiento de entrega
        var recurso = await _recursoRepo.ObtenerRecursoPorIdAsync(recursoActividadId);
        if (recurso == null || !recurso.RequiereEntrega)
        {
            return false;
        }

        var nuevaEntrega = new EntregaActividad
        {
            RecursoActividadId = recursoActividadId,
            EstudianteId = estudianteId,
            UrlArchivoEntrega = urlArchivo,
            ComentarioEstudiante = comentario,
            FechaEntrega = DateTime.UtcNow,
            Estado = "Pendiente"
        };

        var resultado = await _recursoRepo.CrearEntregaAsync(nuevaEntrega);
        return resultado != null; // Devuelve true si la entidad fue creada exitosamente
    }

    // Usado por CursoController.SubirEntregaActividad (Actualización/Reentrega)
    public async Task<bool> ActualizarEntregaExistenteAsync(int recursoActividadId, int estudianteId, string urlArchivo, string? comentario)
    {
        // 1. Obtener la entrega existente
        var entregaExistente = await _recursoRepo.ObtenerEntregaPorRecursoYEstudianteAsync(recursoActividadId, estudianteId);

        // 2. Validar que exista y que NO esté calificada
        if (entregaExistente == null || entregaExistente.Estado == "Calificado")
        {
            return false;
        }

        // 3. Actualizar propiedades
        entregaExistente.UrlArchivoEntrega = urlArchivo;
        entregaExistente.ComentarioEstudiante = comentario;
        entregaExistente.FechaEntrega = DateTime.UtcNow;
        entregaExistente.Estado = "Pendiente"; // Vuelve a Pendiente al reentregar

        // 4. Llama al Repositorio para guardar los cambios
        return await _recursoRepo.ActualizarEntregaAsync(entregaExistente);
    }

    // --- LÓGICA PARA INSTRUCTORES (CALIFICAR) ---

    public async Task<List<EntregaActividad>> ObtenerTareasPendientesParaInstructorAsync(int instructorId)
    {
        return await _recursoRepo.ObtenerEntregasPendientesPorInstructorAsync(instructorId);
    }

    public async Task<bool> CalificarEntregaAsync(int entregaId, int instructorId, decimal? calificacion, string observacion)
    {
        var entrega = await _recursoRepo.ObtenerEntregaPorIdAsync(entregaId);

        if (entrega == null || entrega.Estado != "Pendiente")
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(observacion) && calificacion == null)
        {
            return false;
        }

        // El repositorio manejará la actualización en la DB
        return await _recursoRepo.ActualizarEntregaConCalificacionAsync(
            entregaId, instructorId, calificacion, observacion);
    }

    // --- LÓGICA PARA RECURSOS (CREACIÓN/ACTUALIZACIÓN/ELIMINACIÓN - INSTRUCTOR) ---

    public async Task<RecursoActividad?> CrearRecursoActividadAsync(RecursoActividad recurso)
    {
        if (string.IsNullOrWhiteSpace(recurso.Titulo) || string.IsNullOrWhiteSpace(recurso.TipoEntidad) || recurso.EntidadId <= 0)
        {
            return null;
        }

        try
        {
            return await _recursoRepo.CrearRecursoActividadAsync(recurso);
        }
        catch (Exception)
        {
            // Loggea la excepción
            return null;
        }
    }

    public async Task<bool> ActualizarRecursoActividadAsync(RecursoActividad recursoActualizado, int recursoId, int instructorId)
    {
        var recursoOriginal = await _recursoRepo.ObtenerRecursoPorIdAsync(recursoId);

        if (recursoOriginal == null)
        {
            return false;
        }

        // Mapear y actualizar solo las propiedades permitidas
        recursoOriginal.Titulo = recursoActualizado.Titulo;
        recursoOriginal.Descripcion = recursoActualizado.Descripcion ?? string.Empty;
        recursoOriginal.TipoRecurso = recursoActualizado.TipoRecurso;
        recursoOriginal.Url = recursoActualizado.Url;
        recursoOriginal.RequiereEntrega = recursoActualizado.RequiereEntrega;

        return await _recursoRepo.ActualizarRecursoActividadAsync(recursoOriginal);
    }

    public async Task<bool> EliminarRecursoActividadAsync(int recursoActividadId, int instructorId)
    {
        var recurso = await _recursoRepo.ObtenerRecursoPorIdAsync(recursoActividadId);

        if (recurso == null)
        {
            return false;
        }

        // Opcional: Validar Permisos del instructor antes de la eliminación.

        return await _recursoRepo.EliminarRecursoActividadAsync(recursoActividadId);
    }

    // NOTA: Se ha eliminado ObtenerEntregaDeEstudianteAsync, ya que estaba duplicado
    // con ObtenerEntregaPorActividadYEstudianteAsync.
    public async Task<List<RecursoActividad>> ObtenerRecursosPorEntidadesAsync(IEnumerable<int> entidadIds)
    {
        // Validación básica
        if (entidadIds == null || !entidadIds.Any())
        {
            return new List<RecursoActividad>();
        }

        // Delega la operación al repositorio.
        return await _recursoRepo.ObtenerRecursosPorEntidadesAsync(entidadIds);
    }
}