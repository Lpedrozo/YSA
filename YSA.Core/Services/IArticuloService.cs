using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using YSA.Core.DTOs;
using YSA.Core.Entities;

namespace YSA.Core.Services
{
    public interface IArticuloService
    {
        Task<IEnumerable<Articulo>> GetAllArticulosAsync();
        Task<Articulo> GetArticuloByIdAsync(int id);
        Task<Articulo> CreateArticuloAsync(Articulo articulo, IFormFile fotoDestacadoFile, IFormFile imagenPrincipalFile, List<IFormFile> fotosContenidoFiles);
        Task UpdateArticuloAsync(Articulo articulo, IFormFile fotoDestacadoFile, IFormFile imagenPrincipalFile, List<IFormFile> nuevasFotosContenidoFiles);
        Task DeleteArticuloAsync(int id);
        Task DeleteFotoContenidoAsync(int fotoId);
        Task UpdateArticuloEstadoAsync(int id, string nuevoEstado);

    }
}