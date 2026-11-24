using BibliotecaDigital.Web.Infrastructure;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Dapper;

namespace BibliotecaDigital.Web.Repositories.Dapper;

public sealed class InteracaoRepository : RepositorioDapperBase, IInteracaoRepository
{
    private sealed class LinhaAvaliacao
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public int LivroId { get; set; }

        public int Nota { get; set; }

        public DateTime DataAvaliacao { get; set; }
    }

    private sealed class LinhaComentario
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public int LivroId { get; set; }

        public int? ComentarioPaiId { get; set; }

        public string Texto { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; }

        public bool Editado { get; set; }

        public bool Excluido { get; set; }
    }

    private sealed class LinhaFavorito
    {
        public int UsuarioId { get; set; }

        public int LivroId { get; set; }

        public DateTime DataAdicao { get; set; }
    }

    public InteracaoRepository(IFabricaConexao fabricaConexao) : base(fabricaConexao)
    {
    }

    public IEnumerable<Avaliacao> ObterAvaliacoesDoLivro(int livroId) => Executar(conexao =>
    {
        const string sql = """
            SELECT Id, UsuarioId, LivroId, Nota, DataAvaliacao
            FROM Avaliacao WHERE LivroId = @LivroId ORDER BY DataAvaliacao DESC;
            """;
        return conexao.Query<LinhaAvaliacao>(sql, new { LivroId = livroId }).Select(Mapear).ToList();
    }, "listar as avaliacoes do livro");

    public Avaliacao? ObterAvaliacao(int usuarioId, int livroId) => Executar(conexao =>
    {
        const string sql = """
            SELECT Id, UsuarioId, LivroId, Nota, DataAvaliacao
            FROM Avaliacao WHERE UsuarioId = @UsuarioId AND LivroId = @LivroId;
            """;
        var linha = conexao.QuerySingleOrDefault<LinhaAvaliacao>(sql, new { UsuarioId = usuarioId, LivroId = livroId });
        return linha is null ? null : Mapear(linha);
    }, "buscar a avaliacao do usuario");

    public void SalvarAvaliacao(Avaliacao avaliacao) => Executar(conexao =>
    {
        const string sql = """
            INSERT INTO Avaliacao (UsuarioId, LivroId, Nota, DataAvaliacao)
            VALUES (@UsuarioId, @LivroId, @Nota, @DataAvaliacao)
            ON CONFLICT (UsuarioId, LivroId)
            DO UPDATE SET Nota = excluded.Nota, DataAvaliacao = excluded.DataAvaliacao;

            SELECT Id FROM Avaliacao WHERE UsuarioId = @UsuarioId AND LivroId = @LivroId;
            """;

        var id = conexao.ExecuteScalar<int>(sql, new
        {
            avaliacao.UsuarioId,
            avaliacao.LivroId,
            avaliacao.Nota,
            avaliacao.DataAvaliacao
        });

        avaliacao.DefinirId(id);
    }, "salvar a avaliacao");

    public IEnumerable<Comentario> ObterComentariosDoLivro(int livroId) => Executar(conexao =>
    {
        const string sql = """
            SELECT Id, UsuarioId, LivroId, ComentarioPaiId, Texto, DataCriacao, Editado, Excluido
            FROM Comentario WHERE LivroId = @LivroId ORDER BY DataCriacao;
            """;
        return conexao.Query<LinhaComentario>(sql, new { LivroId = livroId }).Select(Mapear).ToList();
    }, "listar os comentarios do livro");

    public Comentario? ObterComentario(int id) => Executar(conexao =>
    {
        const string sql = """
            SELECT Id, UsuarioId, LivroId, ComentarioPaiId, Texto, DataCriacao, Editado, Excluido
            FROM Comentario WHERE Id = @Id;
            """;
        var linha = conexao.QuerySingleOrDefault<LinhaComentario>(sql, new { Id = id });
        return linha is null ? null : Mapear(linha);
    }, "buscar o comentario");

    public int InserirComentario(Comentario comentario) => Executar(conexao =>
    {
        const string sql = """
            INSERT INTO Comentario (UsuarioId, LivroId, ComentarioPaiId, Texto, DataCriacao, Editado, Excluido)
            VALUES (@UsuarioId, @LivroId, @ComentarioPaiId, @Texto, @DataCriacao, @Editado, @Excluido);
            SELECT last_insert_rowid();
            """;

        var id = conexao.ExecuteScalar<int>(sql, new
        {
            comentario.UsuarioId,
            comentario.LivroId,
            comentario.ComentarioPaiId,
            comentario.Texto,
            comentario.DataCriacao,
            comentario.Editado,
            comentario.Excluido
        });

        comentario.DefinirId(id);
        return id;
    }, "publicar o comentario");

    public void AtualizarComentario(Comentario comentario) => Executar(conexao =>
    {
        const string sql = "UPDATE Comentario SET Texto = @Texto, Editado = @Editado, Excluido = @Excluido WHERE Id = @Id;";
        conexao.Execute(sql, new { comentario.Texto, comentario.Editado, comentario.Excluido, comentario.Id });
    }, "atualizar o comentario");

    public IEnumerable<Favorito> ObterFavoritosDoUsuario(int usuarioId) => Executar(conexao =>
    {
        const string sql = """
            SELECT UsuarioId, LivroId, DataAdicao
            FROM Favorito WHERE UsuarioId = @UsuarioId ORDER BY DataAdicao DESC;
            """;
        return conexao.Query<LinhaFavorito>(sql, new { UsuarioId = usuarioId })
            .Select(linha => Favorito.Reconstruir(linha.UsuarioId, linha.LivroId, linha.DataAdicao))
            .ToList();
    }, "listar os favoritos do usuario");

    public bool EhFavorito(int usuarioId, int livroId) => Executar(conexao =>
    {
        const string sql = "SELECT COUNT(1) FROM Favorito WHERE UsuarioId = @UsuarioId AND LivroId = @LivroId;";
        return conexao.ExecuteScalar<long>(sql, new { UsuarioId = usuarioId, LivroId = livroId }) > 0;
    }, "verificar o favorito");

    public void AlternarFavorito(int usuarioId, int livroId) => Executar(conexao =>
    {
        var parametros = new { UsuarioId = usuarioId, LivroId = livroId };

        const string sqlExiste = "SELECT COUNT(1) FROM Favorito WHERE UsuarioId = @UsuarioId AND LivroId = @LivroId;";

        if (conexao.ExecuteScalar<long>(sqlExiste, parametros) > 0)
        {
            conexao.Execute("DELETE FROM Favorito WHERE UsuarioId = @UsuarioId AND LivroId = @LivroId;", parametros);
            return;
        }

        var favorito = new Favorito(usuarioId, livroId);

        const string sqlInserir = "INSERT INTO Favorito (UsuarioId, LivroId, DataAdicao) VALUES (@UsuarioId, @LivroId, @DataAdicao);";
        conexao.Execute(sqlInserir, new { favorito.UsuarioId, favorito.LivroId, favorito.DataAdicao });
    }, "alternar o favorito");

    private static Avaliacao Mapear(LinhaAvaliacao linha)
        => Avaliacao.Reconstruir(linha.Id, linha.UsuarioId, linha.LivroId, linha.Nota, linha.DataAvaliacao);

    private static Comentario Mapear(LinhaComentario linha)
        => Comentario.Reconstruir(
            linha.Id,
            linha.UsuarioId,
            linha.LivroId,
            linha.ComentarioPaiId,
            linha.Texto,
            linha.DataCriacao,
            linha.Editado,
            linha.Excluido);
}
