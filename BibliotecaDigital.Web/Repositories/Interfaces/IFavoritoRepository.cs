using BibliotecaDigital.Web.Models;

namespace BibliotecaDigital.Web.Repositories.Interfaces;

// Favorito nao tem Id proprio (a chave e o par usuario + livro),
// por isso nao herda de IRepository<T>.
public interface IFavoritoRepository
{
    IEnumerable<Favorito> ObterPorUsuario(int usuarioId);

    bool Existe(int usuarioId, int livroId);

    void Adicionar(Favorito favorito);

    void Remover(int usuarioId, int livroId);
}
