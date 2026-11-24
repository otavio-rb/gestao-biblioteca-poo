using BibliotecaDigital.Web.Models;

namespace BibliotecaDigital.Web.Repositories.Interfaces;

public interface ILivroRepository : IRepository<Livro>
{
    Livro? ObterComDetalhes(int id);

    IEnumerable<Livro> Pesquisar(string? termo);

    bool IsbnEmUso(string isbn, int idIgnorado = 0);

    int Inserir(Livro livro, decimal valorReferencia);

    void Atualizar(Livro livro, decimal valorReferencia);

    decimal ObterValorReferencia(int livroId);

    void DefinirAutores(int livroId, IEnumerable<int> autorIds);

    void DefinirCategorias(int livroId, IEnumerable<int> categoriaIds);
}
