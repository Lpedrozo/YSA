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
                                 .ToListAsync();
        }

        public async Task<Curso> GetByIdAsync(int id)
        {
            return await _context.Cursos
                                 .Include(c => c.CursoCategorias)
                                 .ThenInclude(cc => cc.Categoria)
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
    }
}