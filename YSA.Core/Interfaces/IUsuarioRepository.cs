using YSA.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Core.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario> GetByIdAsync(string id);
        Task UpdateAsync(Usuario usuario);

    }
}