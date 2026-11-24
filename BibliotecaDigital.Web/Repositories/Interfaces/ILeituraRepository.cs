using BibliotecaDigital.Web.Models;

namespace BibliotecaDigital.Web.Repositories.Interfaces;

public interface ILeituraRepository : IRepository<Leitura>
{
    IEnumerable<Leitura> ObterPorUsuario(int usuarioId);

    Leitura? ObterPorUsuarioELivro(int usuarioId, int livroId);

    HistoricoLeitura ObterHistorico(int usuarioId);
}
