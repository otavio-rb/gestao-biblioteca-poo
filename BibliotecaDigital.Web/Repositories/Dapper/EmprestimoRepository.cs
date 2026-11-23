using BibliotecaDigital.Web.Infrastructure;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Models.Enums;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Dapper;

namespace BibliotecaDigital.Web.Repositories.Dapper;

public sealed class EmprestimoRepository : RepositorioDapperBase, IEmprestimoRepository
{
    private const string SelecaoCompleta = """
        SELECT e.Id, e.LeitorId, e.ExemplarId, e.ReservaId, e.ValorReferenciaObra,
               e.DataRetirada, e.DataPrevista, e.DataDevolucao, e.Status,
               l.Titulo AS TituloLivro, u.Nome AS NomeLeitor
        FROM Emprestimo e
        INNER JOIN ExemplarDigital x ON x.Id = e.ExemplarId
        INNER JOIN Livro l           ON l.Id = x.LivroId
        INNER JOIN Usuario u         ON u.Id = e.LeitorId
        """;

    private sealed class LinhaEmprestimo
    {
        public int Id { get; set; }

        public int LeitorId { get; set; }

        public int ExemplarId { get; set; }

        public int? ReservaId { get; set; }

        public decimal ValorReferenciaObra { get; set; }

        public DateTime DataRetirada { get; set; }

        public DateTime DataPrevista { get; set; }

        public DateTime? DataDevolucao { get; set; }

        public int Status { get; set; }

        public string TituloLivro { get; set; } = string.Empty;

        public string NomeLeitor { get; set; } = string.Empty;
    }

    private sealed class LinhaRenovacao
    {
        public int Id { get; set; }

        public int EmprestimoId { get; set; }

        public DateTime DataRenovacao { get; set; }

        public DateTime NovaDataPrevista { get; set; }

        public int Sequencia { get; set; }
    }

    private sealed class LinhaDevolucao
    {
        public int Id { get; set; }

        public int EmprestimoId { get; set; }

        public DateTime DataDevolucao { get; set; }

        public bool HouveAtraso { get; set; }
    }

    private sealed class LinhaMulta
    {
        public int Id { get; set; }

        public int EmprestimoId { get; set; }

        public decimal ValorDiario { get; set; }

        public int DiasAtraso { get; set; }

        public decimal ValorReferenciaObra { get; set; }

        public decimal ValorTotal { get; set; }

        public bool Paga { get; set; }
    }

    public EmprestimoRepository(IFabricaConexao fabricaConexao) : base(fabricaConexao)
    {
    }

    public IEnumerable<Emprestimo> ObterTodos() => Executar(conexao =>
    {
        var sql = $"{SelecaoCompleta} ORDER BY e.DataRetirada DESC;";
        return conexao.Query<LinhaEmprestimo>(sql).Select(Mapear).ToList();
    }, "listar os emprestimos");

    public IEnumerable<Emprestimo> ObterPorLeitor(int leitorId) => Executar(conexao =>
    {
        var sql = $"{SelecaoCompleta} WHERE e.LeitorId = @LeitorId ORDER BY e.DataRetirada DESC;";
        return conexao.Query<LinhaEmprestimo>(sql, new { LeitorId = leitorId }).Select(Mapear).ToList();
    }, "listar os emprestimos do leitor");

    public Emprestimo? ObterPorId(int id) => Executar(conexao =>
    {
        var sql = $"{SelecaoCompleta} WHERE e.Id = @Id;";
        var linha = conexao.QuerySingleOrDefault<LinhaEmprestimo>(sql, new { Id = id });
        return linha is null ? null : Mapear(linha);
    }, "buscar o emprestimo");

    public Emprestimo? ObterComDetalhes(int id) => Executar(conexao =>
    {
        var sql = $"{SelecaoCompleta} WHERE e.Id = @Id;";
        var linha = conexao.QuerySingleOrDefault<LinhaEmprestimo>(sql, new { Id = id });

        if (linha is null)
        {
            return null;
        }

        var emprestimo = Mapear(linha);

        const string sqlRenovacoes = """
            SELECT Id, EmprestimoId, DataRenovacao, NovaDataPrevista, Sequencia
            FROM Renovacao WHERE EmprestimoId = @Id ORDER BY Sequencia;
            """;

        emprestimo.AnexarRenovacoes(conexao.Query<LinhaRenovacao>(sqlRenovacoes, new { Id = id })
            .Select(renovacao => Renovacao.Reconstruir(
                renovacao.Id,
                renovacao.EmprestimoId,
                renovacao.DataRenovacao,
                renovacao.NovaDataPrevista,
                renovacao.Sequencia)));

        const string sqlDevolucao = """
            SELECT Id, EmprestimoId, DataDevolucao, HouveAtraso
            FROM Devolucao WHERE EmprestimoId = @Id;
            """;

        var devolucao = conexao.QuerySingleOrDefault<LinhaDevolucao>(sqlDevolucao, new { Id = id });
        if (devolucao is not null)
        {
            emprestimo.AnexarDevolucao(Devolucao.Reconstruir(
                devolucao.Id,
                devolucao.EmprestimoId,
                devolucao.DataDevolucao,
                devolucao.HouveAtraso));
        }

        emprestimo.AnexarMulta(BuscarMulta(conexao, id));

        return emprestimo;
    }, "carregar os detalhes do emprestimo");

    public int ContarAtivosDoLeitor(int leitorId) => Executar(conexao =>
    {
        const string sql = "SELECT COUNT(1) FROM Emprestimo WHERE LeitorId = @LeitorId AND Status IN (1, 2);";
        return (int)conexao.ExecuteScalar<long>(sql, new { LeitorId = leitorId });
    }, "contar os emprestimos ativos do leitor");

    public decimal TotalMultasEmAbertoDoLeitor(int leitorId) => Executar(conexao =>
    {
        const string sql = """
            SELECT CAST(COALESCE(SUM(m.ValorTotal), 0) AS REAL)
            FROM Multa m
            INNER JOIN Emprestimo e ON e.Id = m.EmprestimoId
            WHERE e.LeitorId = @LeitorId AND m.Paga = 0;
            """;
        return conexao.ExecuteScalar<decimal>(sql, new { LeitorId = leitorId });
    }, "somar as multas em aberto do leitor");

    public int Inserir(Emprestimo entidade) => Executar(conexao =>
    {
        const string sql = """
            INSERT INTO Emprestimo (LeitorId, ExemplarId, ReservaId, ValorReferenciaObra, DataRetirada, DataPrevista, DataDevolucao, Status)
            VALUES (@LeitorId, @ExemplarId, @ReservaId, @ValorReferenciaObra, @DataRetirada, @DataPrevista, @DataDevolucao, @Status);
            SELECT last_insert_rowid();
            """;

        var id = conexao.ExecuteScalar<int>(sql, new
        {
            entidade.LeitorId,
            entidade.ExemplarId,
            entidade.ReservaId,
            entidade.ValorReferenciaObra,
            entidade.DataRetirada,
            entidade.DataPrevista,
            entidade.DataDevolucao,
            Status = (int)entidade.Status
        });

        entidade.DefinirId(id);
        return id;
    }, "registrar o emprestimo");

    public void Atualizar(Emprestimo entidade) => Executar(conexao =>
    {
        const string sql = """
            UPDATE Emprestimo SET
                DataPrevista = @DataPrevista,
                DataDevolucao = @DataDevolucao,
                Status = @Status
            WHERE Id = @Id;
            """;

        conexao.Execute(sql, new
        {
            entidade.DataPrevista,
            entidade.DataDevolucao,
            Status = (int)entidade.Status,
            entidade.Id
        });
    }, "atualizar o emprestimo");

    public void Remover(int id) => Executar(conexao =>
    {
        conexao.Execute("DELETE FROM Emprestimo WHERE Id = @Id;", new { Id = id });
    }, "excluir o emprestimo");

    public void RegistrarRenovacao(Renovacao renovacao) => Executar(conexao =>
    {
        const string sql = """
            INSERT INTO Renovacao (EmprestimoId, DataRenovacao, NovaDataPrevista, Sequencia)
            VALUES (@EmprestimoId, @DataRenovacao, @NovaDataPrevista, @Sequencia);
            SELECT last_insert_rowid();
            """;

        var id = conexao.ExecuteScalar<int>(sql, new
        {
            renovacao.EmprestimoId,
            renovacao.DataRenovacao,
            renovacao.NovaDataPrevista,
            renovacao.Sequencia
        });

        renovacao.DefinirId(id);
    }, "registrar a renovacao");

    public void RegistrarDevolucao(Devolucao devolucao) => Executar(conexao =>
    {
        const string sql = """
            INSERT INTO Devolucao (EmprestimoId, DataDevolucao, HouveAtraso)
            VALUES (@EmprestimoId, @DataDevolucao, @HouveAtraso);
            SELECT last_insert_rowid();
            """;

        var id = conexao.ExecuteScalar<int>(sql, new
        {
            devolucao.EmprestimoId,
            devolucao.DataDevolucao,
            devolucao.HouveAtraso
        });

        devolucao.DefinirId(id);
    }, "registrar a devolucao");

    public void RegistrarMulta(Multa multa) => Executar(conexao =>
    {
        const string sql = """
            INSERT INTO Multa (EmprestimoId, ValorDiario, DiasAtraso, ValorReferenciaObra, ValorTotal, Paga)
            VALUES (@EmprestimoId, @ValorDiario, @DiasAtraso, @ValorReferenciaObra, @ValorTotal, @Paga);
            SELECT last_insert_rowid();
            """;

        var id = conexao.ExecuteScalar<int>(sql, new
        {
            multa.EmprestimoId,
            multa.ValorDiario,
            multa.DiasAtraso,
            multa.ValorReferenciaObra,
            multa.ValorTotal,
            multa.Paga
        });

        multa.DefinirId(id);
    }, "registrar a multa");

    public Multa? ObterMultaPorEmprestimo(int emprestimoId)
        => Executar(conexao => BuscarMulta(conexao, emprestimoId), "buscar a multa do emprestimo");

    public void QuitarMulta(Multa multa) => Executar(conexao =>
    {
        conexao.Execute("UPDATE Multa SET Paga = @Paga WHERE Id = @Id;", new { multa.Paga, multa.Id });
    }, "quitar a multa");

    private static Multa? BuscarMulta(System.Data.IDbConnection conexao, int emprestimoId)
    {
        const string sql = """
            SELECT Id, EmprestimoId, ValorDiario, DiasAtraso, ValorReferenciaObra, ValorTotal, Paga
            FROM Multa WHERE EmprestimoId = @Id;
            """;

        var linha = conexao.QuerySingleOrDefault<LinhaMulta>(sql, new { Id = emprestimoId });

        return linha is null
            ? null
            : Multa.Reconstruir(
                linha.Id,
                linha.EmprestimoId,
                linha.ValorDiario,
                linha.DiasAtraso,
                linha.ValorReferenciaObra,
                linha.ValorTotal,
                linha.Paga);
    }

    private static Emprestimo Mapear(LinhaEmprestimo linha)
    {
        var emprestimo = Emprestimo.Reconstruir(
            linha.Id,
            linha.LeitorId,
            linha.ExemplarId,
            linha.ReservaId,
            linha.ValorReferenciaObra,
            linha.DataRetirada,
            linha.DataPrevista,
            linha.DataDevolucao,
            (StatusEmprestimo)linha.Status);

        emprestimo.DescreverContexto(linha.TituloLivro, linha.NomeLeitor);
        return emprestimo;
    }
}
