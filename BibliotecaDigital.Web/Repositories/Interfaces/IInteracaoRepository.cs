using BibliotecaDigital.Web.Models;

namespace BibliotecaDigital.Web.Repositories.Interfaces;

public interface IInteracaoRepository
{
    IEnumerable<Avaliacao> ObterAvaliacoesDoLivro(int livroId);

    Avaliacao? ObterAvaliacao(int usuarioId, int livroId);

    void SalvarAvaliacao(Avaliacao avaliacao);

    IEnumerable<Comentario> ObterComentariosDoLivro(int livroId);

    Comentario? ObterComentario(int id);

    int InserirComentario(Comentario comentario);

    void AtualizarComentario(Comentario comentario);

    IEnumerable<Favorito> ObterFavoritosDoUsuario(int usuarioId);

    bool EhFavorito(int usuarioId, int livroId);

    void AlternarFavorito(int usuarioId, int livroId);
}
