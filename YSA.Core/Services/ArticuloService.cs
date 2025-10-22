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

            // --- Cambio aquí: usar RootFolderName en lugar de "uploads" ---
            var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, RootFolderName, subPath);
            Directory.CreateDirectory(uploadPath);

            var fileName = Path.GetFileNameWithoutExtension(file.FileName)
                         + "_" + Guid.NewGuid().ToString().Substring(0, 8)
                         + Path.GetExtension(file.FileName);

            var filePath = Path.Combine(uploadPath, fileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores en caso de fallo al escribir el archivo
                Console.WriteLine($"Error al guardar el archivo en {filePath}: {ex.Message}");
                return null;
            }


            // --- Cambio aquí: URL relativa para la BD ---
            // Retorna la URL relativa para guardar en la BD (ej: /Articulos/destacados/nombre.jpg)
            return $"/{RootFolderName}/{subPath}/{fileName}".Replace('\\', '/');
        }

        /// <summary>
        /// Elimina un archivo del sistema de archivos usando su URL relativa.
        /// </summary>
        /// <param name="url">La URL relativa del archivo (ej: /Articulos/destacados/archivo.jpg).</param>
        private void EliminarArchivo(string? url)
        {
            if (string.IsNullOrEmpty(url)) return;

            // La URL ahora comienza con /Articulos/
            // Combina wwwroot con la URL eliminando el '/' inicial
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, url.TrimStart('/'));

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (IOException ex)
                {
                    // Manejar excepción si el archivo está en uso
                    Console.WriteLine($"Error al intentar eliminar el archivo {filePath}: {ex.Message}");
                }
            }
        }

        // --- Implementación IArticuloService ---

        public async Task<IEnumerable<Articulo>> GetAllArticulosAsync()
        {
            return await _articuloRepository.GetAllAsync();
        }

        public async Task<Articulo> GetArticuloByIdAsync(int id)
        {
            // Usamos Articulo? en el retorno para manejar posibles nulos
            return await _articuloRepository.GetByIdAsync(id);
        }

        public async Task<Articulo> CreateArticuloAsync(Articulo articulo, IFormFile? fotoDestacadoFile, IFormFile? imagenPrincipalFile, List<IFormFile>? fotosContenidoFiles)
        {
            // 1. Inicialización de la colección de fotos si es nula
            articulo.Fotos ??= new List<ArticuloFoto>();

            // 2. Guardar y asignar archivos principales
            // SubPath: "destacados" -> wwwroot/Articulos/destacados/
            articulo.UrlFotoDestacado = await GuardarArchivo(fotoDestacadoFile, "destacados");

            // SubPath: "principales" -> wwwroot/Articulos/principales/
            articulo.UrlImagenPrincipal = await GuardarArchivo(imagenPrincipalFile, "principales");

            articulo.FechaPublicacion = DateTime.UtcNow;

            // 3. Guardar el artículo (necesario para obtener el ID)
            await _articuloRepository.AddAsync(articulo);

            // 4. Guardar y asociar fotos de contenido
            if (fotosContenidoFiles != null && fotosContenidoFiles.Count > 0)
            {
                int initialCount = articulo.Fotos.Count;
                // SubPath: "contenido/{articuloId}" -> wwwroot/Articulos/contenido/{articuloId}/
                var contenidoSubPath = $"contenido/{articulo.Id}";

                foreach (var file in fotosContenidoFiles.Where(f => f != null && f.Length > 0))
                {
                    var url = await GuardarArchivo(file, contenidoSubPath);
                    if (!string.IsNullOrEmpty(url))
                    {
                        articulo.Fotos.Add(new ArticuloFoto
                        {
                            ArticuloId = articulo.Id,
                            UrlFoto = url,
                            Orden = ++initialCount // Incrementar y asignar el orden
                        });
                    }
                }
                // Actualizar para guardar las fotos si el AddAsync inicial no lo hizo con las colecciones
                await _articuloRepository.UpdateAsync(articulo);
            }

            return articulo;
        }

        public async Task UpdateArticuloAsync(Articulo articulo, IFormFile? fotoDestacadoFile, IFormFile? imagenPrincipalFile, List<IFormFile>? nuevasFotosContenidoFiles)
        {
            var articuloExistente = await _articuloRepository.GetByIdAsync(articulo.Id);

            if (articuloExistente == null)
            {
                throw new KeyNotFoundException($"Artículo con ID {articulo.Id} no encontrado.");
            }

            // Asegurar que la lista de fotos no es nula antes de añadir
            articuloExistente.Fotos ??= new List<ArticuloFoto>();

            // 1. Manejo de Foto Destacado
            if (fotoDestacadoFile != null)
            {
                EliminarArchivo(articuloExistente.UrlFotoDestacado);
                articulo.UrlFotoDestacado = await GuardarArchivo(fotoDestacadoFile, "destacados");
            }
            else
            {
                articulo.UrlFotoDestacado = articuloExistente.UrlFotoDestacado;
            }

            // 2. Manejo de Imagen Principal
            if (imagenPrincipalFile != null)
            {
                EliminarArchivo(articuloExistente.UrlImagenPrincipal);
                articulo.UrlImagenPrincipal = await GuardarArchivo(imagenPrincipalFile, "principales");
            }
            else
            {
                articulo.UrlImagenPrincipal = articuloExistente.UrlImagenPrincipal;
            }

            // Mapear propiedades sanitizadas (asumimos que el controlador ya las sanitizó)
            articuloExistente.Titulo = articulo.Titulo;
            articuloExistente.Resumen = articulo.Resumen;
            articuloExistente.ContenidoTexto = articulo.ContenidoTexto;
            articuloExistente.Categoria = articulo.Categoria;
            articuloExistente.Estado = articulo.Estado;
            articuloExistente.NombrePersonaDestacada = articulo.NombrePersonaDestacada;
            articuloExistente.BiografiaCortaDestacado = articulo.BiografiaCortaDestacado;

            // Actualizar URLs de archivos principales
            articuloExistente.UrlFotoDestacado = articulo.UrlFotoDestacado;
            articuloExistente.UrlImagenPrincipal = articulo.UrlImagenPrincipal;

            // 3. Guardar y asociar NUEVAS fotos de contenido
            if (nuevasFotosContenidoFiles != null && nuevasFotosContenidoFiles.Count > 0)
            {
                int initialCount = articuloExistente.Fotos.Count;
                var contenidoSubPath = $"contenido/{articulo.Id}";

                foreach (var file in nuevasFotosContenidoFiles.Where(f => f != null && f.Length > 0))
                {
                    var url = await GuardarArchivo(file, contenidoSubPath);
                    if (!string.IsNullOrEmpty(url))
                    {
                        articuloExistente.Fotos.Add(new ArticuloFoto
                        {
                            ArticuloId = articulo.Id,
                            UrlFoto = url,
                            Orden = ++initialCount,
                            Descripcion = "Foto"
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

            // 1. Eliminar archivos del sistema de archivos
            EliminarArchivo(articulo.UrlFotoDestacado);
            EliminarArchivo(articulo.UrlImagenPrincipal);

            // Eliminar todas las fotos de contenido
            if (articulo.Fotos != null)
            {
                foreach (var foto in articulo.Fotos)
                {
                    EliminarArchivo(foto.UrlFoto);
                }
            }

            // 2. Eliminar de la base de datos
            await _articuloRepository.DeleteAsync(id);

            // 3. Opcional: Eliminar la carpeta completa del contenido del artículo si existe.
            var articuloPath = Path.Combine(_hostingEnvironment.WebRootPath, RootFolderName, $"contenido/{id}");
            if (Directory.Exists(articuloPath))
            {
                try
                {
                    // true para eliminar subcarpetas y archivos dentro.
                    Directory.Delete(articuloPath, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al eliminar la carpeta del artículo {articuloPath}: {ex.Message}");
                }
            }
        }

        public async Task DeleteFotoContenidoAsync(int fotoId)
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
    }
}
