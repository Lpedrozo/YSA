using System.Collections.Generic;
using System.Threading.Tasks;
using YSA.Core.Entities;

namespace YSA.Core.Services
{
    public interface ICursoService
    {
        // Métodos para Categorías
        Task<List<Categoria>> ObtenerTodasLasCategoriasAsync();
        Task CrearCategoriaAsync(Categoria categoria);
        Task<Categoria> ObtenerCategoriaPorIdAsync(int id);
        Task ActualizarCategoriaAsync(Categoria categoria);
        Task EliminarCategoriaAsync(int id);

        // Métodos para Cursos
        Task<List<Curso>> ObtenerTodosLosCursosAsync();
        Task<Curso> ObtenerCursoPorIdAsync(int id);
        Task CrearCursoAsync(Curso curso, int[] categoriasSeleccionadas);
        Task ActualizarCursoAsync(Curso curso, int[] categoriasSeleccionadas);
        Task EliminarCursoAsync(int id);
    }
}