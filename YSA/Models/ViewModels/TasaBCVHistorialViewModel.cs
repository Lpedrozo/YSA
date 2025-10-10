using System.ComponentModel.DataAnnotations;
using YSA.Core.Entities;
namespace YSA.Web.Models.ViewModels
{
    public class TasaBCVHistorialViewModel
    {
        public IEnumerable<TasaBCV> Tasas { get; set; } = new List<TasaBCV>();
    }
}