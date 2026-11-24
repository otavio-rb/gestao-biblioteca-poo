using BibliotecaDigital.Web.Exceptions;
using BibliotecaDigital.Web.Infrastructure;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Dapper;

namespace BibliotecaDigital.Web.Repositories.Dapper;

public sealed class ExemplarRepository : RepositorioDapperBase, IExemplarRepository
{
    private sealed class LinhaExemplar
    {
        public int Id { get; set; }

        public int LivroId { get; set; }

        public string Formato { get; set; } = string.Empty;

        public string Arquivo { get; set; } = string.Empty;

        public int LicencasTotais { get; set; }

        public int LicencasDisponiveis { get; set; }
    }

    public ExemplarRepository(IFabricaConexao fabricaConexao) : base(fabricaConexao)
    {
    }

    public ExemplarDigital? ObterPorId(int id) => Executar(conexao =>
    {
        var linha = BuscarLinha(conexao, id);
        return linha is null ? null : Mapear(linha);
    }, "buscar o exemplar digital");

    public IEnumerable<ExemplarDigital> ObterPorLivro(int livroId) => Executar(conexao =>
    {
        const string sql = """
            SELECT Id, LivroId, Formato, Arquivo, LicencasTotais, LicencasDisponiveis
            FROM ExemplarDigital WHERE LivroId = @LivroId ORDER BY Id;
            """;
        return conexao.Query<LinhaExemplar>(sql, new { LivroId = livroId }).Select(Mapear).ToList();
    }, "listar os exemplares do livro");

    public int ObterLivroId(int exemplarId) => Executar(conexao =>
    {
        const string sql = "SELECT LivroId FROM ExemplarDigital WHERE Id = @Id;";
        return conexao.ExecuteScalar<int>(sql, new { Id = exemplarId });
    }, "identificar o livro do exemplar");

    public int ObterLicencasTotais(int exemplarId) => Executar(conexao =>
    {
        const string sql = "SELECT LicencasTotais FROM ExemplarDigital WHERE Id = @Id;";
        return conexao.ExecuteScalar<int>(sql, new { Id = exemplarId });
    }, "buscar as licencas do exemplar");

    public int Inserir(int livroId, ExemplarDigital exemplar, int licencasTotais) => Executar(conexao =>
    {
        if (licencasTotais < 1)
        {
            throw new DominioException("O exemplar precisa de ao menos uma licenca.");
        }

        const string sql = """
            INSERT INTO ExemplarDigital (LivroId, Formato, Arquivo, LicencasTotais, LicencasDisponiveis)
            VALUES (@LivroId, @Formato, @Arquivo, @LicencasTotais, @LicencasDisponiveis);
            SELECT last_insert_rowid();
            """;

        var id = conexao.ExecuteScalar<int>(sql, new
        {
            LivroId = livroId,
            exemplar.Formato,
            exemplar.Arquivo,
            LicencasTotais = licencasTotais,
            LicencasDisponiveis = exemplar.VerificarDisponibilidade() ? licencasTotais : 0
        });

        exemplar.DefinirId(id);
        return id;
    }, "cadastrar o exemplar digital");

    public void Atualizar(ExemplarDigital exemplar, int licencasTotais) => Executar(conexao =>
    {
        if (licencasTotais < 1)
        {
            throw new DominioException("O exemplar precisa de ao menos uma licenca.");
        }

        var linha = BuscarLinha(conexao, exemplar.Id)
            ?? throw new RegistroNaoEncontradoException("Exemplar digital", exemplar.Id);

        var emUso = linha.LicencasTotais - linha.LicencasDisponiveis;

        if (licencasTotais < emUso)
        {
            throw new DominioException($"Existem {emUso} licenca(s) emprestada(s); o total nao pode ser menor que isso.");
        }

        var disponiveis = exemplar.VerificarDisponibilidade() ? licencasTotais - emUso : 0;

        if (!exemplar.VerificarDisponibilidade() && emUso > 0)
        {
            throw new DominioException("Existem licencas emprestadas; devolva os emprestimos antes de suspender o exemplar.");
        }

        const string sql = """
            UPDATE ExemplarDigital SET
                Formato = @Formato,
                Arquivo = @Arquivo,
                LicencasTotais = @LicencasTotais,
                LicencasDisponiveis = @LicencasDisponiveis
            WHERE Id = @Id;
            """;

        conexao.Execute(sql, new
        {
            exemplar.Formato,
            exemplar.Arquivo,
            LicencasTotais = licencasTotais,
            LicencasDisponiveis = disponiveis,
            exemplar.Id
        });
    }, "atualizar o exemplar digital");

    public void Remover(int id) => Executar(conexao =>
    {
        if (PossuiEmprestimoAtivoNaConexao(conexao, id))
        {
            throw new DominioException("Este exemplar possui emprestimo ativo e nao pode ser excluido.");
        }

        conexao.Execute("DELETE FROM ExemplarDigital WHERE Id = @Id;", new { Id = id });
    }, "excluir o exemplar digital");

    public bool PossuiEmprestimoAtivo(int exemplarId)
        => Executar(conexao => PossuiEmprestimoAtivoNaConexao(conexao, exemplarId), "verificar os emprestimos do exemplar");

    private static bool PossuiEmprestimoAtivoNaConexao(System.Data.IDbConnection conexao, int exemplarId)
    {
        const string sql = "SELECT COUNT(1) FROM Emprestimo WHERE ExemplarId = @Id AND Status IN (1, 2);";
        return conexao.ExecuteScalar<long>(sql, new { Id = exemplarId }) > 0;
    }

    private static LinhaExemplar? BuscarLinha(System.Data.IDbConnection conexao, int id)
    {
        const string sql = """
            SELECT Id, LivroId, Formato, Arquivo, LicencasTotais, LicencasDisponiveis
            FROM ExemplarDigital WHERE Id = @Id;
            """;
        return conexao.QuerySingleOrDefault<LinhaExemplar>(sql, new { Id = id });
    }

    private static ExemplarDigital Mapear(LinhaExemplar linha)
        => ExemplarDigital.Reconstruir(linha.Id, linha.Formato, linha.Arquivo, linha.LicencasDisponiveis > 0);
}
