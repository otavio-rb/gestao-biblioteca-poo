using BibliotecaDigital.Web.Models;

namespace BibliotecaDigital.Web.Repositories.Interfaces;

public interface ILeituraRepository : IRepository<Leitura>
{
    IEnumerable<Leitura> ObterPorUsuario(int usuarioId);

    Leitura? ObterPorUsuarioELivro(int usuarioId, int livroId);

    HistoricoLeitura ObterHistorico(int usuarioId);

    // Listas usadas nos formularios: chave = Id, valor = nome ou titulo.
    Dictionary<int, string> ObterLeitores();

    Dictionary<int, string> ObterLivros();
}
