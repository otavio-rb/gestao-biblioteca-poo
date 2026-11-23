using BibliotecaDigital.Web.Infrastructure;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Dapper;

namespace BibliotecaDigital.Web.Repositories.Dapper;

public sealed class CirculacaoRepository : RepositorioDapperBase, ICirculacaoRepository
{
    private const string SelecaoLeitor = """
        SELECT u.Id,
               u.Nome,
               u.EstaBloqueado,
               COALESCE(u.LimiteEmprestimos, 3) AS LimiteEmprestimos,
               (SELECT COUNT(1) FROM Emprestimo e WHERE e.LeitorId = u.Id AND e.Status IN (1, 2)) AS EmprestimosAtivos,
               (SELECT CAST(COALESCE(SUM(m.ValorTotal), 0) AS REAL)
                FROM Multa m
                INNER JOIN Emprestimo e ON e.Id = m.EmprestimoId
                WHERE e.LeitorId = u.Id AND m.Paga = 0) AS MultasEmAberto
        FROM Usuario u
        WHERE u.Tipo = 'LEITOR'
        """;

    private const string SelecaoExemplar = """
        SELECT x.Id,
               l.Titulo AS TituloLivro,
               l.ValorReferencia AS ValorReferenciaObra,
               x.LicencasTotais,
               x.LicencasDisponiveis
        FROM ExemplarDigital x
        INNER JOIN Livro l ON l.Id = x.LivroId
        """;

    private sealed class LinhaLeitor
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public bool EstaBloqueado { get; set; }

        public int LimiteEmprestimos { get; set; }

        public int EmprestimosAtivos { get; set; }

        public decimal MultasEmAberto { get; set; }
    }

    private sealed class LinhaExemplar
    {
        public int Id { get; set; }

        public string TituloLivro { get; set; } = string.Empty;

        public decimal ValorReferenciaObra { get; set; }

        public int LicencasTotais { get; set; }

        public int LicencasDisponiveis { get; set; }
    }

    public CirculacaoRepository(IFabricaConexao fabricaConexao) : base(fabricaConexao)
    {
    }

    public IEnumerable<LeitorEmCirculacao> ObterLeitores() => Executar(conexao =>
    {
        var sql = $"{SelecaoLeitor} ORDER BY u.Nome;";
        return conexao.Query<LinhaLeitor>(sql).Select(Mapear).ToList();
    }, "listar os leitores");

    public LeitorEmCirculacao? ObterLeitor(int leitorId) => Executar(conexao =>
    {
        var sql = $"{SelecaoLeitor} AND u.Id = @LeitorId;";
        var linha = conexao.QuerySingleOrDefault<LinhaLeitor>(sql, new { LeitorId = leitorId });
        return linha is null ? null : Mapear(linha);
    }, "buscar o leitor");

    public IEnumerable<ExemplarEmCirculacao> ObterExemplares() => Executar(conexao =>
    {
        var sql = $"{SelecaoExemplar} ORDER BY l.Titulo, x.Id;";
        return conexao.Query<LinhaExemplar>(sql).Select(Mapear).ToList();
    }, "listar os exemplares digitais");

    public ExemplarEmCirculacao? ObterExemplar(int exemplarId) => Executar(conexao =>
    {
        var sql = $"{SelecaoExemplar} WHERE x.Id = @ExemplarId;";
        var linha = conexao.QuerySingleOrDefault<LinhaExemplar>(sql, new { ExemplarId = exemplarId });
        return linha is null ? null : Mapear(linha);
    }, "buscar o exemplar digital");

    public void AtualizarLicencas(ExemplarEmCirculacao exemplar) => Executar(conexao =>
    {
        const string sql = "UPDATE ExemplarDigital SET LicencasDisponiveis = @LicencasDisponiveis WHERE Id = @Id;";
        conexao.Execute(sql, new { exemplar.LicencasDisponiveis, exemplar.Id });
    }, "atualizar as licencas do exemplar");

    private static LeitorEmCirculacao Mapear(LinhaLeitor linha)
        => new(
            linha.Id,
            linha.Nome,
            linha.EstaBloqueado,
            linha.LimiteEmprestimos,
            linha.EmprestimosAtivos,
            linha.MultasEmAberto);

    private static ExemplarEmCirculacao Mapear(LinhaExemplar linha)
        => new(
            linha.Id,
            linha.TituloLivro,
            linha.ValorReferenciaObra,
            linha.LicencasTotais,
            linha.LicencasDisponiveis);
}
