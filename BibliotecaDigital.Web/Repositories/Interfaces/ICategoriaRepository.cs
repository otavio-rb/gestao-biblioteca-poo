using BibliotecaDigital.Web.Models;

namespace BibliotecaDigital.Web.Repositories.Interfaces;

public interface ICategoriaRepository : IRepository<Categoria>
{
    IEnumerable<Categoria> ObterPorLivro(int livroId);
}
