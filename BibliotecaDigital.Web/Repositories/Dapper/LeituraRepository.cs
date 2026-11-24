using BibliotecaDigital.Web.Infrastructure;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Models.Enums;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Dapper;

namespace BibliotecaDigital.Web.Repositories.Dapper;

public sealed class LeituraRepository : RepositorioDapperBase, ILeituraRepository
{
    private const string SelecaoCompleta = """
        SELECT le.Id, le.UsuarioId, le.LivroId, le.Status, le.PaginaAtual, le.TotalPaginas,
               le.UltimaAtualizacao, le.DataInicio, le.DataConclusao,
               l.Titulo AS TituloLivro, u.Nome AS NomeLeitor
        FROM Leitura le
        INNER JOIN Livro l   ON l.Id = le.LivroId
        INNER JOIN Usuario u ON u.Id = le.UsuarioId
        """;

    private sealed class LinhaLeitura
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public int LivroId { get; set; }

        public int Status { get; set; }

        public int PaginaAtual { get; set; }

        public int TotalPaginas { get; set; }

        public DateTime UltimaAtualizacao { get; set; }

        public DateTime? DataInicio { get; set; }

        public DateTime? DataConclusao { get; set; }

        public string TituloLivro { get; set; } = string.Empty;

        public string NomeLeitor { get; set; } = string.Empty;
    }

    private sealed class LinhaOpcao
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;
    }

    public LeituraRepository(IFabricaConexao fabricaConexao) : base(fabricaConexao)
    {
    }

    public IEnumerable<Leitura> ObterTodos() => Executar(conexao =>
    {
        var sql = $"{SelecaoCompleta} ORDER BY le.UltimaAtualizacao DESC;";
        return conexao.Query<LinhaLeitura>(sql).Select(Mapear).ToList();
    }, "listar as leituras");

    public IEnumerable<Leitura> ObterPorUsuario(int usuarioId) => Executar(conexao =>
    {
        var sql = $"{SelecaoCompleta} WHERE le.UsuarioId = @UsuarioId ORDER BY le.UltimaAtualizacao DESC;";
        return conexao.Query<LinhaLeitura>(sql, new { UsuarioId = usuarioId }).Select(Mapear).ToList();
    }, "listar as leituras do leitor");

    public Leitura? ObterPorId(int id) => Executar(conexao =>
    {
        var sql = $"{SelecaoCompleta} WHERE le.Id = @Id;";
        var linha = conexao.QuerySingleOrDefault<LinhaLeitura>(sql, new { Id = id });
        return linha is null ? null : Mapear(linha);
    }, "buscar a leitura");

    public Leitura? ObterPorUsuarioELivro(int usuarioId, int livroId) => Executar(conexao =>
    {
        var sql = $"{SelecaoCompleta} WHERE le.UsuarioId = @UsuarioId AND le.LivroId = @LivroId;";
        var linha = conexao.QuerySingleOrDefault<LinhaLeitura>(sql, new { UsuarioId = usuarioId, LivroId = livroId });
        return linha is null ? null : Mapear(linha);
    }, "buscar a leitura do livro");

    // Monta o HistoricoLeitura do usuario com todas as leituras dele.
    public HistoricoLeitura ObterHistorico(int usuarioId)
    {
        var historico = new HistoricoLeitura(usuarioId);

        foreach (var leitura in ObterPorUsuario(usuarioId))
        {
            historico.Registrar(leitura);
        }

        return historico;
    }

    public Dictionary<int, string> ObterLeitores() => Executar(conexao =>
    {
        const string sql = "SELECT Id, Nome FROM Usuario WHERE Tipo = 'LEITOR' ORDER BY Nome;";
        return conexao.Query<LinhaOpcao>(sql).ToDictionary(linha => linha.Id, linha => linha.Nome);
    }, "listar os leitores");

    public Dictionary<int, string> ObterLivros() => Executar(conexao =>
    {
        const string sql = "SELECT Id, Titulo AS Nome FROM Livro ORDER BY Titulo;";
        return conexao.Query<LinhaOpcao>(sql).ToDictionary(linha => linha.Id, linha => linha.Nome);
    }, "listar os livros");

    public int Inserir(Leitura entidade) => Executar(conexao =>
    {
        const string sql = """
            INSERT INTO Leitura (UsuarioId, LivroId, Status, PaginaAtual, TotalPaginas, UltimaAtualizacao, DataInicio, DataConclusao)
            VALUES (@UsuarioId, @LivroId, @Status, @PaginaAtual, @TotalPaginas, @UltimaAtualizacao, @DataInicio, @DataConclusao);
            SELECT last_insert_rowid();
            """;

        var id = conexao.ExecuteScalar<int>(sql, new
        {
            entidade.UsuarioId,
            entidade.LivroId,
            Status = (int)entidade.Status,
            entidade.Progresso.PaginaAtual,
            entidade.Progresso.TotalPaginas,
            entidade.Progresso.UltimaAtualizacao,
            entidade.DataInicio,
            entidade.DataConclusao
        });

        entidade.DefinirId(id);
        return id;
    }, "registrar a leitura");

    public void Atualizar(Leitura entidade) => Executar(conexao =>
    {
        const string sql = """
            UPDATE Leitura SET
                Status = @Status,
                PaginaAtual = @PaginaAtual,
                UltimaAtualizacao = @UltimaAtualizacao,
                DataInicio = @DataInicio,
                DataConclusao = @DataConclusao
            WHERE Id = @Id;
            """;

        conexao.Execute(sql, new
        {
            Status = (int)entidade.Status,
            entidade.Progresso.PaginaAtual,
            entidade.Progresso.UltimaAtualizacao,
            entidade.DataInicio,
            entidade.DataConclusao,
            entidade.Id
        });
    }, "atualizar a leitura");

    public void Remover(int id) => Executar(conexao =>
    {
        conexao.Execute("DELETE FROM Leitura WHERE Id = @Id;", new { Id = id });
    }, "excluir a leitura");

    private static Leitura Mapear(LinhaLeitura linha)
    {
        var progresso = ProgressoLeitura.Reconstruir(linha.PaginaAtual, linha.TotalPaginas, linha.UltimaAtualizacao);

        var leitura = Leitura.Reconstruir(
            linha.Id,
            linha.UsuarioId,
            linha.LivroId,
            (StatusLeitura)linha.Status,
            progresso,
            linha.DataInicio,
            linha.DataConclusao);

        leitura.DescreverContexto(linha.TituloLivro, linha.NomeLeitor);
        return leitura;
    }
}
