using YSA.Core.Entities;
using YSA.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace YSA.Core.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IVentaItemService _ventaItemService;
        private readonly UserManager<Usuario> _userManager;
        private readonly IEstudianteCursoRepository _estudianteCursoRepository;


        public PedidoService(IPedidoRepository pedidoRepository, IVentaItemService ventaItemService, UserManager<Usuario> userManager, IEstudianteCursoRepository estudianteCursoRepository)
        {
            _pedidoRepository = pedidoRepository;
            _ventaItemService = ventaItemService;
            _userManager = userManager;
            _estudianteCursoRepository = estudianteCursoRepository;
        }

        public async Task<Pedido> CrearPedidoAsync(int estudianteId, List<int> ventaItemIds)
        {
            // Ahora usamos UserManager para obtener el usuario directamente
            var estudiante = await _userManager.FindByIdAsync(estudianteId.ToString());
            if (estudiante == null)
            {
                throw new ArgumentException("Estudiante no encontrado.");
            }

            var ventaItems = await _ventaItemService.ObtenerVentaItemsPorIdsAsync(ventaItemIds);
            if (ventaItems == null || !ventaItems.Any())
            {
                throw new ArgumentException("No se encontraron items de venta válidos.");
            }

            var nuevoPedido = new Pedido
            {
                EstudianteId = estudianteId,
                FechaPedido = DateTime.UtcNow,
                Estado = "Pendiente",
                Total = ventaItems.Sum(v => v.Precio),
                PedidoItems = ventaItems.Select(v => new PedidoItem
                {
                    VentaItemId = v.Id,
                    PrecioUnidad = v.Precio,
                    Cantidad = 1
                }).ToList()
            };

            return await _pedidoRepository.AddAsync(nuevoPedido);
        }

        public async Task<Pedido> ObtenerPedidoPorIdAsync(int pedidoId)
        {
            return await _pedidoRepository.GetByIdAsync(pedidoId);
        }

        public async Task<Pedido> ObtenerPedidoConItemsYVentaItemsAsync(int pedidoId)
        {
            return await _pedidoRepository.GetPedidoWithItemsAndVentaItemsAsync(pedidoId);
        }

        public async Task<bool> ActualizarEstadoPedidoAsync(int pedidoId, string nuevoEstado)
        {
            var pedido = await _pedidoRepository.GetByIdAsync(pedidoId);
            if (pedido == null)
            {
                return false;
            }

            pedido.Estado = nuevoEstado;
            return await _pedidoRepository.UpdateAsync(pedido);
        }

        public async Task<Pago> RegistrarPagoAsync(Pago pago)
        {
            return await _pedidoRepository.AddPagoAsync(pago);
        }
        public async Task<Pago> GetPagoWithPedido(int id)
        {
            return await _pedidoRepository.GetPagoWithPedido(id);
        }
        public async Task<IEnumerable<Pedido>> ObtenerPedidosPorEstadoAsync(string estado)
        {
            return await _pedidoRepository.GetPedidosByEstadoAsync(estado);
        }
        public async Task AprobarPedidoYOtorgarAccesoAsync(int pedidoId)
        {
            var pedido = await _pedidoRepository.GetPedidoWithItemsAndVentaItemsAsync(pedidoId);

            if (pedido != null)
            {
                // 1. Cambia el estado del pedido a "Completado"
                pedido.Estado = "Completado";
                await _pedidoRepository.UpdateAsync(pedido);

                // 2. Otorga acceso a los cursos
                foreach (var pedidoItem in pedido.PedidoItems)
                {
                    // Asegúrate de que el VentaItem esté relacionado con un curso
                    if (pedidoItem.VentaItem.CursoId.HasValue)
                    {
                        var estudianteCurso = new EstudianteCurso
                        {
                            EstudianteId = pedido.EstudianteId,
                            CursoId = pedidoItem.VentaItem.CursoId.Value,
                            FechaAccesoOtorgado = DateTime.UtcNow
                        };

                        await _estudianteCursoRepository.AddAsync(estudianteCurso);
                    }
                }
            }
        }
        public async Task<bool> TienePedidoPendientePorCursoAsync(int estudianteId, int cursoId)
        {
            return await _pedidoRepository.ExistePedidoEnEstadoParaCursoAsync(estudianteId, cursoId, "Validando");
        }
        public async Task<IEnumerable<Pedido>> ObtenerPedidosAprobadosPorUsuarioAsync(int estudianteId)
        {
            return await _pedidoRepository.GetPedidosByUsuarioAndEstadoAsync(estudianteId, "Completado");
        }
    }
}