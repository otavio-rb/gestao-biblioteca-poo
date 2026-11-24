using BibliotecaDigital.Web.Models;

namespace BibliotecaDigital.Web.Repositories.Interfaces;

public interface IComentarioRepository : IRepository<Comentario>
{
    IEnumerable<Comentario> ObterPorLivro(int livroId);
}
