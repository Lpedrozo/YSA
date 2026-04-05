using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using YSA.Core.DTOs;
using YSA.Core.Entities;
using YSA.Core.Interfaces;

namespace YSA.Core.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly UserManager<Usuario> _userManager;
        private readonly IPedidoService _pedidoService;
        private readonly IVentaItemService _ventaItemService;
        private readonly ICursoService _cursoService;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            UserManager<Usuario> userManager,
            IPedidoService pedidoService,
            IVentaItemService ventaItemService,
            ICursoService cursoService)
        {
            _usuarioRepository = usuarioRepository;
            _userManager = userManager;
            _pedidoService = pedidoService;
            _ventaItemService = ventaItemService;
            _cursoService = cursoService;
        }

        public async Task<int> GetTotalEstudiantesAsync()
        {
            return await _usuarioRepository.GetTotalEstudiantesAsync();
        }

        public async Task<Usuario> GetUsuarioByIdAsync(string id)
        {
            return await _usuarioRepository.GetByIdAsync(id);
        }

        public async Task<IdentityResult> UpdateUsuarioAsync(Usuario usuario)
        {
            return await _userManager.UpdateAsync(usuario);
        }

        public async Task<IdentityResult> ChangePasswordAsync(Usuario usuario, string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(usuario, currentPassword, newPassword);
        }

        public async Task<EstudianteDetalleDto> ObtenerEstudianteDetalleAsync(int id)
        {
            var usuario = await _userManager.FindByIdAsync(id.ToString());
            if (usuario == null)
                return null;

            // Obtener cursos comprados
            var pedidosAprobados = await _pedidoService.ObtenerPedidosAprobadosPorUsuarioAsync(usuario.Id);
            var cursosComprados = new List<string>();
            var clasesInscritas = new List<string>();

            foreach (var pedido in pedidosAprobados)
            {
                var ventaItems = await _ventaItemService.ObtenerItemsPorPedidoIdAsync(pedido.Id);
                foreach (var item in ventaItems)
                {
                    if (item.Tipo == "Curso")
                    {
                        var curso = await _cursoService.ObtenerCursoPorIdAsync((int)item.CursoId);
                        if (curso != null)
                        {
                            cursosComprados.Add(curso.Titulo);

                            // Si es curso presencial, obtener clases inscritas
                            if (curso.TipoCurso == Core.Enums.TipoCurso.Presencial)
                            {
                                var inscripciones = await _cursoService.ObtenerInscripcionesPorEstudianteIdAsync(usuario.Id);
                                foreach (var inscripcion in inscripciones)
                                {
                                    if (inscripcion.ClasePresencial != null)
                                    {
                                        clasesInscritas.Add($"{inscripcion.ClasePresencial.Titulo} - {inscripcion.ClasePresencial.FechaHoraInicio:dd/MM/yyyy}");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Obtener pedidos con pagos
            var pedidos = await _pedidoService.ObtenerPedidosPorUsuarioAsync(usuario.Id);
            var pedidosResumen = pedidos.Select(p => new PedidoResumenDto
            {
                Id = p.Id,
                FechaPedido = p.FechaPedido,
                Total = p.Total,
                Estado = p.Estado,
                MetodoPago = p.Pago?.MetodoPago,
                ReferenciaPago = p.Pago?.ReferenciaPago,
                FechaPago = p.Pago?.FechaPago
            }).ToList();

            return new EstudianteDetalleDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Cedula = usuario.Cedula,
                WhatsApp = usuario.WhatsApp,
                FechaNacimiento = usuario.FechaNacimiento,
                EsMenorEdad = usuario.EsMenorEdad,
                NombreRepresentante = usuario.NombreRepresentante,
                CedulaRepresentante = usuario.CedulaRepresentante,
                ExperienciaTatuaje = usuario.ExperienciaTatuaje,
                AtendidoPor = usuario.AtendidoPor,
                FechaCreacion = usuario.FechaCreacion,
                UrlImagen = usuario.UrlImagen,
                CursosComprados = cursosComprados.Distinct().ToList(),
                ClasesInscritas = clasesInscritas.Distinct().ToList(),
                Pedidos = pedidosResumen
            };
        }
    }
}