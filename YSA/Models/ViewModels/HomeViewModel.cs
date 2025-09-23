using System.ComponentModel.DataAnnotations;
using YSA.Core.Entities;

namespace YSA.Web.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Evento> EventosAcademia { get; set; }
        public IEnumerable<Artista> Artistas { get; set; }
    }
}
