using BibliotecaDigital.Web.Models;

namespace BibliotecaDigital.Web.Repositories.Interfaces;

public interface IAvaliacaoRepository : IRepository<Avaliacao>
{
    Avaliacao? ObterPorUsuarioELivro(int usuarioId, int livroId);

    double? ObterMediaDoLivro(int livroId);
}
