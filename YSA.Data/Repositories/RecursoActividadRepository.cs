using YSA.Data.Data;
using YSA.Core.Entities;
using YSA.Core.Interfaces; // Asegúrate de incluir esta interfaz
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

public class RecursoActividadRepository : IRecursoActividadRepository
{
    private readonly ApplicationDbContext _context;

    public RecursoActividadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // --- MÉTODOS PARA RECURSOS (CRUD) ---

    public async Task<List<RecursoActividad>> ObtenerRecursosPorEntidadAsync(string tipoEntidad, int entidadId)
    {
        return await _context.RecursosActividades
            .Where(r => r.TipoEntidad == tipoEntidad && r.EntidadId == entidadId)
            .ToListAsync();
    }

    public async Task<RecursoActividad?> ObtenerRecursoPorIdAsync(int recursoActividadId)
    {
        return await _context.RecursosActividades.FindAsync(recursoActividadId);
    }

    public async Task<RecursoActividad> CrearRecursoActividadAsync(RecursoActividad recurso)
    {
        _context.RecursosActividades.Add(recurso);
        await _context.SaveChangesAsync();
        return recurso;
    }

    public async Task<bool> ActualizarRecursoActividadAsync(RecursoActividad recurso)
    {
        _context.RecursosActividades.Update(recurso);
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.RecursosActividades.AnyAsync(r => r.Id == recurso.Id))
            {
                return false;
            }
            throw;
        }
    }

    public async Task<bool> EliminarRecursoActividadAsync(int recursoActividadId)
    {
        var recurso = await _context.RecursosActividades.FindAsync(recursoActividadId);
        if (recurso == null)
        {
            return false;
        }

        // Opción 2: Solo eliminar el recurso, confiando en ON DELETE CASCADE
        _context.RecursosActividades.Remove(recurso);
        await _context.SaveChangesAsync();

        return true;
    }

    // --- MÉTODOS PARA ENTREGAS (ESTUDIANTE) ---

    public async Task<EntregaActividad> CrearEntregaAsync(EntregaActividad entrega)
    {
        _context.EntregasActividades.Add(entrega);
        await _context.SaveChangesAsync();
        return entrega;
    }

    // **NUEVO / CRÍTICO PARA EL FLUJO DE REENTREGA**
    public async Task<bool> ActualizarEntregaAsync(EntregaActividad entrega)
    {
        _context.EntregasActividades.Update(entrega);
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.EntregasActividades.AnyAsync(e => e.Id == entrega.Id))
            {
                return false;
            }
            throw;
        }
    }

    public async Task<EntregaActividad?> ObtenerEntregaPorRecursoYEstudianteAsync(int recursoActividadId, int estudianteId)
    {
        return await _context.EntregasActividades
            .Include(ea => ea.RecursoActividad)
            .FirstOrDefaultAsync(ea => ea.RecursoActividadId == recursoActividadId && ea.EstudianteId == estudianteId);
    }

    // --- MÉTODOS PARA ENTREGAS (INSTRUCTOR) ---

    public async Task<EntregaActividad?> ObtenerEntregaPorIdAsync(int entregaId)
    {
        return await _context.EntregasActividades
            .Include(ea => ea.RecursoActividad)
            .FirstOrDefaultAsync(ea => ea.Id == entregaId);
    }

    public async Task<List<EntregaActividad>> ObtenerEntregasPendientesPorInstructorAsync(int usuarioInstructorId)
    {
        // 1. Obtener el ArtistaId (InstructorId en la tabla Artistas)
        var artistaId = await _context.Artistas
            .Where(a => a.UsuarioId == usuarioInstructorId)
            .Select(a => a.Id)
            .FirstOrDefaultAsync();

        if (artistaId == 0)
        {
            return new List<EntregaActividad>();
        }

        // 2. Obtener IDs de Cursos, Módulos y Lecciones del Instructor

        // A. Obtener todos los Cursos del instructor
        // *** CAMBIO CLAVE: Aplicar Includes/ThenIncludes ANTES del Select ***
        var cursos = await _context.CursoInstructores
            .Where(ci => ci.ArtistaId == artistaId)
            // 1. Incluir el objeto Curso
            .Include(ci => ci.Curso)
                // 2. Incluir la colección Modulos DENTRO del Curso (ci.Curso.Modulos)
                .ThenInclude(c => c.Modulos)
                    // 3. Incluir la colección Lecciones DENTRO de Modulos (ci.Curso.Modulos.Lecciones)
                    .ThenInclude(m => m.Lecciones)
            // 4. Proyectar la entidad Curso para iterar (que ya viene cargada con Módulos y Lecciones)
            .Select(ci => ci.Curso)
            .ToListAsync();

        // B. Extraer todos los IDs de las entidades a las que el instructor tiene acceso
        var entidadIds = new HashSet<int>();

        foreach (var curso in cursos)
        {
            // ... (La lógica de iteración es correcta y se mantiene)
            // IDs de Cursos
            entidadIds.Add(curso.Id);

            // Notar que 'curso.Modulos' ahora está cargado debido al Include/ThenInclude anterior
            foreach (var modulo in curso.Modulos)
            {
                // IDs de Módulos
                entidadIds.Add(modulo.Id);

                foreach (var leccion in modulo.Lecciones)
                {
                    // IDs de Lecciones
                    entidadIds.Add(leccion.Id);
                }
            }
        }

        // Si no hay ninguna entidad (curso, módulo, lección), no hay actividades que buscar.
        if (!entidadIds.Any())
        {
            return new List<EntregaActividad>();
        }

        // 3. Obtener todas las Actividades asociadas a cualquiera de estas entidades
        var actividadesIds = await _context.RecursosActividades
            .Where(ra => entidadIds.Contains(ra.EntidadId))
            .Select(ra => ra.Id)
            .ToListAsync();

        // Si no hay actividades, retornar lista vacía
        if (!actividadesIds.Any())
        {
            return new List<EntregaActividad>();
        }

        // 4. Obtener Entregas Pendientes, incluyendo las navegaciones necesarias para la vista
        return await _context.EntregasActividades
            .Include(ea => ea.Estudiante)
            .Include(ea => ea.RecursoActividad)
            .Where(ea => actividadesIds.Contains(ea.RecursoActividadId) && ea.Estado == "Pendiente")
            .OrderByDescending(ea => ea.FechaEntrega)
            .ToListAsync();
    }

    public async Task<bool> ActualizarEntregaConCalificacionAsync(int entregaId, int instructorId, decimal? calificacion, string observacion)
    {
        var artistaId = await _context.Artistas.Where(a => a.UsuarioId == instructorId).Select(a => a.Id).FirstOrDefaultAsync();

        var entrega = await _context.EntregasActividades.FindAsync(entregaId);

        if (entrega == null || entrega.Estado != "Pendiente")
        {
            return false;
        }

        entrega.Calificacion = calificacion;
        entrega.ObservacionInstructor = observacion;
        entrega.InstructorId = artistaId;
        entrega.FechaCalificacion = DateTime.UtcNow;
        entrega.Estado = "Calificado";

        _context.EntregasActividades.Update(entrega);
        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<List<RecursoActividad>> ObtenerRecursosPorEntidadesAsync(IEnumerable<int> entidadIds)
    {
        // 1. Convertir a una lista para evitar múltiples enumeraciones si no es estrictamente necesario,
        // aunque Entity Framework puede optimizar el Contains().
        var idsList = entidadIds.ToList();

        // 2. Consulta de Entity Framework Core para buscar recursos cuya EntidadId 
        // esté contenida en la lista proporcionada.
        return await _context.RecursosActividades
            // Filtra donde el EntidadId está en la colección de IDs.
            .Where(ra => idsList.Contains(ra.EntidadId))

            // Puedes añadir aquí .Include() si necesitas cargar datos relacionados del RecursoActividad.
            // Por ejemplo:
            // .Include(ra => ra.EntidadAsociada) 

            .ToListAsync();
    }
}