using YSA.Core.Entities;
using YSA.Core.Interfaces;
using YSA.Data.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace YSA.Data.Repositories
{
    public class CursoRepository : ICursoRepository
    {
        private readonly ApplicationDbContext _context;

        public CursoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Curso>> GetAllWithDetailsAsync()
        {
            return await _context.Cursos
                                 .Include(c => c.CursoCategorias)
                                     .ThenInclude(cc => cc.Categoria)

                                 // *** CAMBIOS PARA LA RELACIÓN MUCHOS A MUCHOS ***
                                 .Include(c => c.CursoInstructores) // 1. Incluye la colección de enlaces
                                     .ThenInclude(ci => ci.Artista)    // 2. Incluye el Artista (Instructor) desde el enlace
                                         .ThenInclude(a => a.Usuario)  // 3. Incluye la entidad Usuario del Artista

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
            await _context.Cursos.AddAsync(curso);
            await _context.SaveChangesAsync();
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
    }
}