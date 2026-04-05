using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using YSA.Core.Enums;

namespace YSA.Data.Repositories
{
    public class CursoRepository : ICursoRepository
    {
        private readonly ApplicationDbContext _context;

        public CursoRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<int> GetTotalCursosAsync()
        {
            return await _context.Cursos.CountAsync();
        }
        public async Task<List<Curso>> GetAllDigitalsAsync()
        {
            return await _context.Cursos
                                 .Where(c => c.TipoCurso == Core.Enums.TipoCurso.Digital)
                                 .Include(c => c.CursoCategorias)
                                     .ThenInclude(cc => cc.Categoria)
                                 .Include(c => c.CursoInstructores) 
                                     .ThenInclude(ci => ci.Artista)   
                                         .ThenInclude(a => a.Usuario)
                                 .ToListAsync();
        }
        public async Task<List<Curso>> GetAll()
        {
            return await _context.Cursos
                                 .Include(c => c.CursoCategorias)
                                     .ThenInclude(cc => cc.Categoria)
                                 .Include(c => c.CursoInstructores)
                                     .ThenInclude(ci => ci.Artista)
                                         .ThenInclude(a => a.Usuario)
                                 .ToListAsync();
        }
        public async Task<List<Curso>> GetAllPresencialsAsync()
        {
            return await _context.Cursos
                                 .Where(c => c.TipoCurso == Core.Enums.TipoCurso.Presencial)
                                 .Include(c => c.CursoCategorias)
                                     .ThenInclude(cc => cc.Categoria)
                                 .Include(c => c.CursoInstructores)
                                     .ThenInclude(ci => ci.Artista)
                                         .ThenInclude(a => a.Usuario)
                                 .ToListAsync();
        }

        public async Task<Curso> GetByIdAsync(int id)
        {
            return await _context.Cursos
                                 .Include(c => c.CursoCategorias)
                                    .ThenInclude(cc => cc.Categoria)
                                 .Include(c => c.Modulos)
                                    .ThenInclude(m => m.Lecciones)
                                 .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Curso curso)
        {
            try
            {
                await _context.Cursos.AddAsync(curso);
                await _context.SaveChangesAsync();
            } catch (Exception ex)
            {

            }
            
        }

        public async Task UpdateAsync(Curso curso)
        {
            _context.Cursos.Update(curso);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso != null)
            {
                _context.Cursos.Remove(curso);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateWithCategoriesAsync(Curso curso, int[] categoriasSeleccionadas)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Obtener la entidad del curso y sus relaciones actuales
                    var cursoExistente = await _context.Cursos
                        .Include(c => c.CursoCategorias)
                        .FirstOrDefaultAsync(c => c.Id == curso.Id);

                    if (cursoExistente == null)
                    {
                        throw new InvalidOperationException("Curso no encontrado.");
                    }

                    // 2. Actualizar las propiedades básicas del curso
                    _context.Entry(cursoExistente).CurrentValues.SetValues(curso);

                    // 3. Obtener los IDs de las categorías existentes y las seleccionadas
                    var categoriasExistentesIds = cursoExistente.CursoCategorias.Select(cc => cc.CategoriaId).ToList();
                    var categoriasSeleccionadasSet = new HashSet<int>(categoriasSeleccionadas);

                    // 4. Identificar las categorías a eliminar
                    var categoriasAEliminar = cursoExistente.CursoCategorias
                        .Where(cc => !categoriasSeleccionadasSet.Contains(cc.CategoriaId))
                        .ToList();

                    // 5. Identificar las categorías a agregar
                    var categoriasAAgregarIds = categoriasSeleccionadasSet
                        .Where(catId => !categoriasExistentesIds.Contains(catId))
                        .ToList();

                    // 6. Ejecutar los cambios en el contexto
                    _context.CursoCategorias.RemoveRange(categoriasAEliminar);
                    _context.CursoCategorias.AddRange(categoriasAAgregarIds.Select(catId => new CursoCategoria { CursoId = curso.Id, CategoriaId = catId }));

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        public async Task ActualizarCursoAsync(Curso curso)
        {
            _context.Cursos.Update(curso);
            await _context.SaveChangesAsync();
        }
        public async Task CrearResenaAsync(Resena resena)
        {
            _context.Resenas.Add(resena);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Resena>> GetResenasPorCursoAsync(int cursoId)
        {
            return await _context.Resenas
                .Include(r => r.Estudiante) // Asumiendo que tienes una relación con el usuario
                .Where(r => r.CursoId == cursoId)
                .ToListAsync();
        }
        public async Task CrearPreguntaAsync(PreguntaRespuesta pregunta)
        {
            _context.PreguntasRespuestas.Add(pregunta);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PreguntaRespuesta>> GetPreguntasPorCursoAsync(int cursoId)
        {
            return await _context.PreguntasRespuestas
                .Include(p => p.Estudiante)
                .Include(p => p.Instructor)
                .Where(p => p.CursoId == cursoId)
                .OrderByDescending(p => p.FechaPregunta)
                .ToListAsync();
        }
        public async Task<List<Anuncio>> GetAnunciosPorCursoAsync(int cursoId)
        {
            return await _context.Anuncios
                .Where(a => a.CursoId == cursoId)
                .OrderByDescending(a => a.FechaPublicacion)
                .ToListAsync();
        }

        public async Task<Anuncio> GetAnuncioByIdAsync(int id)
        {
            return await _context.Anuncios.FindAsync(id);
        }

        public async Task CrearAnuncioAsync(Anuncio anuncio)
        {
            _context.Anuncios.Add(anuncio);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAnuncioAsync(Anuncio anuncio)
        {
            _context.Entry(anuncio).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAnuncioAsync(int id)
        {
            var anuncio = await _context.Anuncios.FindAsync(id);
            if (anuncio != null)
            {
                _context.Anuncios.Remove(anuncio);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<Curso>> GetCursosByEstudianteIdAsync(int estudianteId)
        {
            var cursos = await _context.EstudianteCursos
                                       .Where(ec => ec.EstudianteId == estudianteId)
                                       .Include(ec => ec.Curso)

                                       .ThenInclude(c => c.CursoInstructores)  // 1. Incluye la colección de enlaces
                                       .ThenInclude(ci => ci.Artista)           // 2. Incluye el Artista/Instructor desde el enlace

                                       .OrderByDescending(ec => ec.FechaAccesoOtorgado)
                                       .Take(5)
                                       .Select(ec => ec.Curso)
                                       .ToListAsync();
            return cursos;
        }
        public async Task<bool> ResponderPreguntaAsync(int preguntaId, string respuesta, int instructorId)
        {
            var artistaId = await _context.Artistas.Where(a => a.UsuarioId == instructorId).Select(a => a.Id).FirstOrDefaultAsync();

            var pregunta = await _context.PreguntasRespuestas
                .FirstOrDefaultAsync(pr => pr.Id == preguntaId && pr.Respuesta == "Gracias por preguntar. Ahora, espera la respuesta de tus instructores, ellos no tardan en responder.");

            if (pregunta == null)
            {
                return false; // La pregunta no existe o ya fue respondida
            }

            // 1. Actualiza los campos de respuesta
            pregunta.Respuesta = respuesta;
            pregunta.InstructorId = artistaId;
            pregunta.FechaRespuesta = DateTime.UtcNow; // O DateTime.Now, según tu estándar

            // 2. Marca la entidad como modificada y guarda los cambios
            _context.PreguntasRespuestas.Update(pregunta);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<PreguntaRespuesta>> ObtenerPreguntasPendientesPorInstructorAsync(int instructorId)
        {
            var artistaId = await _context.Artistas
                .Where(a => a.UsuarioId == instructorId)
                .Select(a => a.Id)
                .FirstOrDefaultAsync();

            if (artistaId == 0)
            {
                return new List<PreguntaRespuesta>();
            }

            var cursosIds = await _context.CursoInstructores
                .Where(ci => ci.ArtistaId == artistaId) // Filtra por el ArtistaId
                .Select(ci => ci.CursoId)               // Selecciona solo los IDs de los cursos
                .ToListAsync();

            if (!cursosIds.Any())
            {
                return new List<PreguntaRespuesta>();
            }

            return await _context.PreguntasRespuestas
                .Where(pr => cursosIds.Contains(pr.CursoId) && pr.Respuesta == "Gracias por preguntar. Ahora, espera la respuesta de tus instructores, ellos no tardan en responder.")
                .Include(pr => pr.Curso)        // Incluye el curso para contexto
                .Include(pr => pr.Estudiante)   // Incluye el estudiante que preguntó
                .OrderBy(pr => pr.FechaPregunta)
                .ToListAsync();
        }

        public async Task CrearAsociacionInstructor(int cursoId, int artistaId)
        {
            var existeAsociacion = await _context.CursoInstructores
                .AnyAsync(ci => ci.CursoId == cursoId && ci.ArtistaId == artistaId);

            if (existeAsociacion)
            {
                throw new InvalidOperationException("Este artista ya está asociado a este curso.");
            }

            var nuevaAsociacion = new CursoInstructor
            {
                CursoId = cursoId,
                ArtistaId = artistaId
            };

            await _context.CursoInstructores.AddAsync(nuevaAsociacion);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Artista>> ObtenerArtistasAsociadosACursoAsync(int cursoId)
        {

            return await _context.CursoInstructores
                .Where(ca => ca.CursoId == cursoId)
                .Select(ca => ca.Artista)
                .ToListAsync();

        }

        public async Task DesasociarArtistaACursoAsync(int cursoId, int instructorId)
        {
            var asociacion = await _context.CursoInstructores
                .FirstOrDefaultAsync(ca => ca.CursoId == cursoId && ca.ArtistaId == instructorId);

            if (asociacion != null)
            {
                _context.CursoInstructores.Remove(asociacion);
                await _context.SaveChangesAsync();
            }
        }
        public async Task AddCursoCategoriasAsync(List<CursoCategoria> cursoCategorias)
        {
            await _context.CursoCategorias.AddRangeAsync(cursoCategorias);
            await _context.SaveChangesAsync();
        }

        public async Task AddClasePresencialAsync(ClasePresencial clase)
        {
            await _context.ClasesPresenciales.AddAsync(clase);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ClasePresencial>> GetClasesByCursoIdAsync(int cursoId)
        {
            return await _context.ClasesPresenciales
                .Where(c => c.CursoId == cursoId)
                .OrderBy(c => c.FechaHoraInicio)
                .Include(c => c.Inscripciones)
                    .ThenInclude(i => i.Estudiante)  // ← Agrega esta línea para incluir los datos del estudiante
                .ToListAsync();
        }

        public async Task<ClasePresencial> GetClaseByIdAsync(int id)
        {
            return await _context.ClasesPresenciales
                .Include(c => c.Inscripciones)
                .ThenInclude(i => i.Estudiante)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateClasePresencialAsync(ClasePresencial clase)
        {
            _context.ClasesPresenciales.Update(clase);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteClasePresencialAsync(int id)
        {
            var clase = await GetClaseByIdAsync(id);
            if (clase != null)
            {
                _context.ClasesPresenciales.Remove(clase);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddInscripcionClaseAsync(InscripcionClase inscripcion)
        {
            await _context.InscripcionesClases.AddAsync(inscripcion);
            await _context.SaveChangesAsync();
        }

        public async Task<InscripcionClase> GetInscripcionByClaseAndEstudianteAsync(int clasePresencialId, int estudianteId)
        {
            return await _context.InscripcionesClases
                .FirstOrDefaultAsync(i => i.ClasePresencialId == clasePresencialId && i.EstudianteId == estudianteId);
        }

        public async Task<List<InscripcionClase>> GetInscripcionesByClaseIdAsync(int clasePresencialId)
        {
            return await _context.InscripcionesClases
                .Where(i => i.ClasePresencialId == clasePresencialId)
                .Include(i => i.Estudiante)
                .OrderBy(i => i.FechaInscripcion)
                .ToListAsync();
        }

        public async Task<List<InscripcionClase>> GetInscripcionesByEstudianteIdAsync(int estudianteId)
        {
            return await _context.InscripcionesClases
                .Where(i => i.EstudianteId == estudianteId)
                .Include(i => i.ClasePresencial)
                .ThenInclude(c => c.Curso)
                .OrderByDescending(i => i.FechaInscripcion)
                .ToListAsync();
        }

        public async Task<InscripcionClase> GetInscripcionByIdAsync(int id)
        {
            return await _context.InscripcionesClases
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task UpdateInscripcionClaseAsync(InscripcionClase inscripcion)
        {
            _context.InscripcionesClases.Update(inscripcion);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteInscripcionClaseAsync(int id)
        {
            var inscripcion = await GetInscripcionByIdAsync(id);
            if (inscripcion != null)
            {
                _context.InscripcionesClases.Remove(inscripcion);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Curso>> GetCursosByTipoAsync(TipoCurso tipo)
        {
            return await _context.Cursos
                .Where(c => c.TipoCurso == tipo)
                .Include(c => c.CursoCategorias)
                    .ThenInclude(cc => cc.Categoria)
                .Include(c => c.CursoInstructores)
                    .ThenInclude(ci => ci.Artista)
                        .ThenInclude(a => a.Usuario)
                .Include(c => c.ClasesPresenciales)
                .ToListAsync();
        }

        public void UpdateClasePresencial(ClasePresencial clase)
        {
            _context.ClasesPresenciales.Update(clase);
        }

        public void UpdateInscripcionClase(InscripcionClase inscripcion)
        {
            _context.InscripcionesClases.Update(inscripcion);
        }

        public async Task AddCursoCategoriaAsync(CursoCategoria cursoCategoria)
        {
            await _context.CursoCategorias.AddAsync(cursoCategoria);
        }
    }
}