using System.ComponentModel.DataAnnotations;
namespace YSA.Web.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalCursos { get; set; }
        public int TotalEstudiantes { get; set; }
        public int PedidosPendientes { get; set; }
    }
}