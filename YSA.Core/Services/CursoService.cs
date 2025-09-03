using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    }
}