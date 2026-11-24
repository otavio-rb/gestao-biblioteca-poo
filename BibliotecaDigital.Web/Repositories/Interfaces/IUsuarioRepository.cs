using System.Collections.Generic;
using System.Threading.Tasks;
using BibliotecaDigital.Web.Models;

namespace BibliotecaDigital.Web.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<int> CadastrarAsync(Usuario usuario);
        Task<Usuario?> BuscarPorIdAsync(int id);
        Task<Usuario?> BuscarPorEmailAsync(string email);
        Task<IEnumerable<Usuario>> ListarTodosAsync();
        Task AtualizarStatusBloqueioAsync(Usuario usuario);
    }
}