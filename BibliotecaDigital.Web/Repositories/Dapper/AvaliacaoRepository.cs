using BibliotecaDigital.Web.Infrastructure;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Dapper;

namespace BibliotecaDigital.Web.Repositories.Dapper;

public sealed class AvaliacaoRepository : RepositorioDapperBase, IAvaliacaoRepository
{
    private const string Selecao = "SELECT Id, UsuarioId, LivroId, Nota, DataAvaliacao FROM Avaliacao";

    private sealed class LinhaAvaliacao
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public int LivroId { get; set; }

        public int Nota { get; set; }

        public DateTime DataAvaliacao { get; set; }
    }

    public AvaliacaoRepository(IFabricaConexao fabricaConexao) : base(fabricaConexao)
    {
    }

    public IEnumerable<Avaliacao> ObterTodos() => Executar(conexao =>
    {
        var sql = $"{Selecao} ORDER BY DataAvaliacao DESC;";
        return conexao.Query<LinhaAvaliacao>(sql).Select(Mapear).ToList();
    }, "listar as avaliacoes");

    public Avaliacao? ObterPorId(int id) => Executar(conexao =>
    {
        var sql = $"{Selecao} WHERE Id = @Id;";
        var linha = conexao.QuerySingleOrDefault<LinhaAvaliacao>(sql, new { Id = id });
        return linha is null ? null : Mapear(linha);
    }, "buscar a avaliacao");

    public Avaliacao? ObterPorUsuarioELivro(int usuarioId, int livroId) => Executar(conexao =>
    {
        var sql = $"{Selecao} WHERE UsuarioId = @UsuarioId AND LivroId = @LivroId;";
        var linha = conexao.QuerySingleOrDefault<LinhaAvaliacao>(sql, new { UsuarioId = usuarioId, LivroId = livroId });
        return linha is null ? null : Mapear(linha);
    }, "buscar a avaliacao do leitor");

    public double? ObterMediaDoLivro(int livroId) => Executar(conexao =>
    {
        const string sql = "SELECT AVG(CAST(Nota AS REAL)) FROM Avaliacao WHERE LivroId = @LivroId;";
        return conexao.ExecuteScalar<double?>(sql, new { LivroId = livroId });
    }, "calcular a media do livro");

    public int Inserir(Avaliacao entidade) => Executar(conexao =>
    {
        const string sql = """
            INSERT INTO Avaliacao (UsuarioId, LivroId, Nota, DataAvaliacao)
            VALUES (@UsuarioId, @LivroId, @Nota, @DataAvaliacao);
            SELECT last_insert_rowid();
            """;

        var id = conexao.ExecuteScalar<int>(sql, new
        {
            entidade.UsuarioId,
            entidade.LivroId,
            entidade.Nota,
            entidade.DataAvaliacao
        });

        entidade.DefinirId(id);
        return id;
    }, "registrar a avaliacao");

    public void Atualizar(Avaliacao entidade) => Executar(conexao =>
    {
        const string sql = "UPDATE Avaliacao SET Nota = @Nota, DataAvaliacao = @DataAvaliacao WHERE Id = @Id;";
        conexao.Execute(sql, new { entidade.Nota, entidade.DataAvaliacao, entidade.Id });
    }, "atualizar a avaliacao");

    public void Remover(int id) => Executar(conexao =>
    {
        conexao.Execute("DELETE FROM Avaliacao WHERE Id = @Id;", new { Id = id });
    }, "excluir a avaliacao");

    private static Avaliacao Mapear(LinhaAvaliacao linha)
        => Avaliacao.Reconstruir(linha.Id, linha.UsuarioId, linha.LivroId, linha.Nota, linha.DataAvaliacao);
}
