using BibliotecaDigital.Web.Models;

namespace BibliotecaDigital.Web.Repositories.Interfaces;

public interface IAutorRepository : IRepository<Autor>
{
    IEnumerable<Autor> ObterPorLivro(int livroId);

    bool PossuiLivrosVinculados(int autorId);
}
