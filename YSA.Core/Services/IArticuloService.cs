using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using YSA.Core.Entities;

namespace YSA.Core.Interfaces
{
    public interface IArticuloService
    {
        Task<IEnumerable<Articulo>> GetAllArticulosAsync();
        Task<Articulo> GetArticuloByIdAsync(int id);

        // Versión simplificada para catálogo/galería
        Task<Articulo> CreateArticuloAsync(
            Articulo articulo,
            IFormFile? imagenPortadaFile,
            List<IFormFile>? fotosGaleriaFiles);

        Task UpdateArticuloAsync(
            Articulo articulo,
            IFormFile? imagenPortadaFile,
            List<IFormFile>? nuevasFotosGaleriaFiles);

        Task DeleteArticuloAsync(int id);
        Task DeleteFotoGaleriaAsync(int fotoId);
        Task UpdateArticuloEstadoAsync(int id, string nuevoEstado);

        // Método para catálogo público
        Task<List<Articulo>> GetArticulosPublicadosAsync();
    }
}