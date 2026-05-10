using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YSA.Core.Entities;
using YSA.Core.Interfaces;
using System.Linq;

namespace YSA.Core.Services
{
    public class ArticuloService : IArticuloService
    {
        private readonly IArticuloRepository _articuloRepository;
        private readonly IWebHostEnvironment _hostingEnvironment;

        // Nombre de la carpeta principal de archivos en wwwroot
        private const string RootFolderName = "Articulos";

        public ArticuloService(IArticuloRepository articuloRepository, IWebHostEnvironment hostingEnvironment)
        {
            _articuloRepository = articuloRepository;
            _hostingEnvironment = hostingEnvironment;
        }

        private async Task<string?> GuardarArchivo(IFormFile? file, string subPath)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, RootFolderName, subPath);
            Directory.CreateDirectory(uploadPath);

            var fileName = Path.GetFileNameWithoutExtension(file.FileName)
                         + "_" + Guid.NewGuid().ToString().Substring(0, 8)
                         + Path.GetExtension(file.FileName);

            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{RootFolderName}/{subPath}/{fileName}".Replace('\\', '/');
        }

        private void EliminarArchivo(string? url)
        {
            if (string.IsNullOrEmpty(url)) return;

            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, url.TrimStart('/'));

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Error al intentar eliminar el archivo {filePath}: {ex.Message}");
                }
            }
        }

        // ==================== IMPLEMENTACIÓN DE IArticuloService ====================

        public async Task<IEnumerable<Articulo>> GetAllArticulosAsync()
        {
            return await _articuloRepository.GetAllAsync();
        }

        public async Task<Articulo> GetArticuloByIdAsync(int id)
        {
            return await _articuloRepository.GetByIdAsync(id);
        }

        public async Task<Articulo> CreateArticuloAsync(
            Articulo articulo,
            IFormFile? imagenPortadaFile,
            List<IFormFile>? fotosGaleriaFiles)
        {
            // Inicializar colección de fotos
            articulo.Fotos ??= new List<ArticuloFoto>();
            articulo.FechaPublicacion = DateTime.UtcNow;

            // Guardar imagen de portada (si se proporciona)
            if (imagenPortadaFile != null)
            {
                articulo.UrlImagenPortada = await GuardarArchivo(imagenPortadaFile, "portadas");
            }

            // Guardar el artículo para obtener el ID
            await _articuloRepository.AddAsync(articulo);

            // Guardar fotos de galería
            if (fotosGaleriaFiles != null && fotosGaleriaFiles.Any())
            {
                int orden = 0;
                var galeriaSubPath = $"galeria/{articulo.Id}";

                foreach (var file in fotosGaleriaFiles.Where(f => f != null && f.Length > 0))
                {
                    var url = await GuardarArchivo(file, galeriaSubPath);
                    if (!string.IsNullOrEmpty(url))
                    {
                        articulo.Fotos.Add(new ArticuloFoto
                        {
                            ArticuloId = articulo.Id,
                            UrlFoto = url,
                            Orden = orden++,
                            Descripcion = null
                        });
                    }
                }

                if (articulo.Fotos.Any())
                {
                    await _articuloRepository.UpdateAsync(articulo);
                }
            }

            return articulo;
        }

        public async Task UpdateArticuloAsync(
            Articulo articulo,
            IFormFile? imagenPortadaFile,
            List<IFormFile>? nuevasFotosGaleriaFiles)
        {
            var articuloExistente = await _articuloRepository.GetByIdAsync(articulo.Id);

            if (articuloExistente == null)
            {
                throw new KeyNotFoundException($"Artículo con ID {articulo.Id} no encontrado.");
            }

            articuloExistente.Fotos ??= new List<ArticuloFoto>();

            // 1. Manejo de Imagen de Portada
            if (imagenPortadaFile != null)
            {
                EliminarArchivo(articuloExistente.UrlImagenPortada);
                articuloExistente.UrlImagenPortada = await GuardarArchivo(imagenPortadaFile, "portadas");
            }

            // 2. Actualizar campos básicos (opcionales)
            articuloExistente.Titulo = articulo.Titulo;
            articuloExistente.Resumen = articulo.Resumen;
            articuloExistente.ContenidoTexto = articulo.ContenidoTexto;
            articuloExistente.Categoria = articulo.Categoria;
            articuloExistente.Estado = articulo.Estado;

            // 3. Agregar nuevas fotos de galería
            if (nuevasFotosGaleriaFiles != null && nuevasFotosGaleriaFiles.Any())
            {
                int orden = articuloExistente.Fotos.Count;
                var galeriaSubPath = $"galeria/{articulo.Id}";

                foreach (var file in nuevasFotosGaleriaFiles.Where(f => f != null && f.Length > 0))
                {
                    var url = await GuardarArchivo(file, galeriaSubPath);
                    if (!string.IsNullOrEmpty(url))
                    {
                        articuloExistente.Fotos.Add(new ArticuloFoto
                        {
                            ArticuloId = articulo.Id,
                            UrlFoto = url,
                            Orden = orden++,
                            Descripcion = orden.ToString()
                        });
                    }
                }
            }

            await _articuloRepository.UpdateAsync(articuloExistente);
        }

        public async Task DeleteArticuloAsync(int id)
        {
            var articulo = await _articuloRepository.GetByIdAsync(id);
            if (articulo == null) return;

            // 1. Eliminar archivos del sistema
            EliminarArchivo(articulo.UrlImagenPortada);
            EliminarArchivo(articulo.UrlFotoDestacado);

            // Eliminar todas las fotos de galería
            if (articulo.Fotos != null)
            {
                foreach (var foto in articulo.Fotos)
                {
                    EliminarArchivo(foto.UrlFoto);
                }
            }

            // 2. Eliminar carpeta de galería
            var galeriaPath = Path.Combine(_hostingEnvironment.WebRootPath, RootFolderName, $"galeria/{id}");
            if (Directory.Exists(galeriaPath))
            {
                try
                {
                    Directory.Delete(galeriaPath, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al eliminar carpeta {galeriaPath}: {ex.Message}");
                }
            }

            // 3. Eliminar de la base de datos
            await _articuloRepository.DeleteAsync(id);
        }

        public async Task DeleteFotoGaleriaAsync(int fotoId)
        {
            var foto = await _articuloRepository.GetFotoByIdAsync(fotoId);
            if (foto == null) return;

            EliminarArchivo(foto.UrlFoto);
            await _articuloRepository.DeleteFotoAsync(fotoId);
        }

        public async Task UpdateArticuloEstadoAsync(int id, string nuevoEstado)
        {
            var articulo = await _articuloRepository.GetByIdAsync(id);
            if (articulo == null)
            {
                throw new KeyNotFoundException($"Artículo con ID {id} no encontrado.");
            }

            articulo.Estado = nuevoEstado;
            await _articuloRepository.UpdateAsync(articulo);
        }

        // ==================== NUEVOS MÉTODOS PARA CATÁLOGO ====================

        public async Task<List<Articulo>> GetArticulosPublicadosAsync()
        {
            var allArticulos = await _articuloRepository.GetAllAsync();
            return allArticulos.Where(a => a.Estado == "Publicado")
                               .OrderByDescending(a => a.FechaPublicacion)
                               .ToList();
        }
    }
}