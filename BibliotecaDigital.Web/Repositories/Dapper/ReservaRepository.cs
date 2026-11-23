using BibliotecaDigital.Web.Infrastructure;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Models.Enums;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Dapper;

namespace BibliotecaDigital.Web.Repositories.Dapper;

public sealed class ReservaRepository : RepositorioDapperBase, IReservaRepository
{
    private const string SelecaoCompleta = """
        SELECT r.Id, r.LeitorId, r.ExemplarId, r.DataReserva, r.DataLimiteRetirada, r.PosicaoFila, r.Status,
               l.Titulo AS TituloLivro, u.Nome AS NomeLeitor
        FROM Reserva r
        INNER JOIN ExemplarDigital x ON x.Id = r.ExemplarId
        INNER JOIN Livro l           ON l.Id = x.LivroId
        INNER JOIN Usuario u         ON u.Id = r.LeitorId
        """;

    private sealed class LinhaReserva
    {
        public int Id { get; set; }

        public int LeitorId { get; set; }

        public int ExemplarId { get; set; }

        public DateTime DataReserva { get; set; }

        public DateTime DataLimiteRetirada { get; set; }

        public int PosicaoFila { get; set; }

        public int Status { get; set; }

        public string TituloLivro { get; set; } = string.Empty;

        public string NomeLeitor { get; set; } = string.Empty;
    }

    public ReservaRepository(IFabricaConexao fabricaConexao) : base(fabricaConexao)
    {
    }

    public IEnumerable<Reserva> ObterTodos() => Executar(conexao =>
    {
        var sql = $"{SelecaoCompleta} ORDER BY r.DataReserva DESC;";
        return conexao.Query<LinhaReserva>(sql).Select(Mapear).ToList();
    }, "listar as reservas");

    public Reserva? ObterPorId(int id) => Executar(conexao =>
    {
        var sql = $"{SelecaoCompleta} WHERE r.Id = @Id;";
        var linha = conexao.QuerySingleOrDefault<LinhaReserva>(sql, new { Id = id });
        return linha is null ? null : Mapear(linha);
    }, "buscar a reserva");

    public IEnumerable<Reserva> ObterPendentesPorExemplar(int exemplarId) => Executar(conexao =>
    {
        var sql = $"{SelecaoCompleta} WHERE r.ExemplarId = @ExemplarId AND r.Status IN (1, 2) ORDER BY r.PosicaoFila;";
        return conexao.Query<LinhaReserva>(sql, new { ExemplarId = exemplarId }).Select(Mapear).ToList();
    }, "listar as reservas pendentes do exemplar");

    public bool ExisteReservaPendenteParaOutroLeitor(int exemplarId, int leitorId) => Executar(conexao =>
    {
        const string sql = """
            SELECT COUNT(1) FROM Reserva
            WHERE ExemplarId = @ExemplarId AND LeitorId <> @LeitorId AND Status IN (1, 2);
            """;
        return conexao.ExecuteScalar<long>(sql, new { ExemplarId = exemplarId, LeitorId = leitorId }) > 0;
    }, "verificar as reservas pendentes do exemplar");

    public int ProximaPosicaoNaFila(int exemplarId) => Executar(conexao =>
    {
        const string sql = """
            SELECT COALESCE(MAX(PosicaoFila), 0) + 1 FROM Reserva
            WHERE ExemplarId = @ExemplarId AND Status IN (1, 2);
            """;
        return (int)conexao.ExecuteScalar<long>(sql, new { ExemplarId = exemplarId });
    }, "calcular a posicao na fila de reservas");

    public int Inserir(Reserva entidade) => Executar(conexao =>
    {
        const string sql = """
            INSERT INTO Reserva (LeitorId, ExemplarId, DataReserva, DataLimiteRetirada, PosicaoFila, Status)
            VALUES (@LeitorId, @ExemplarId, @DataReserva, @DataLimiteRetirada, @PosicaoFila, @Status);
            SELECT last_insert_rowid();
            """;

        var id = conexao.ExecuteScalar<int>(sql, new
        {
            entidade.LeitorId,
            entidade.ExemplarId,
            entidade.DataReserva,
            entidade.DataLimiteRetirada,
            entidade.PosicaoFila,
            Status = (int)entidade.Status
        });

        entidade.DefinirId(id);
        return id;
    }, "registrar a reserva");

    public void Atualizar(Reserva entidade) => Executar(conexao =>
    {
        const string sql = """
            UPDATE Reserva SET
                DataLimiteRetirada = @DataLimiteRetirada,
                PosicaoFila = @PosicaoFila,
                Status = @Status
            WHERE Id = @Id;
            """;

        conexao.Execute(sql, new
        {
            entidade.DataLimiteRetirada,
            entidade.PosicaoFila,
            Status = (int)entidade.Status,
            entidade.Id
        });
    }, "atualizar a reserva");

    public void Remover(int id) => Executar(conexao =>
    {
        conexao.Execute("DELETE FROM Reserva WHERE Id = @Id;", new { Id = id });
    }, "excluir a reserva");

    private static Reserva Mapear(LinhaReserva linha)
    {
        var reserva = Reserva.Reconstruir(
            linha.Id,
            linha.LeitorId,
            linha.ExemplarId,
            linha.DataReserva,
            linha.DataLimiteRetirada,
            linha.PosicaoFila,
            (StatusReserva)linha.Status);

        reserva.DescreverContexto(linha.TituloLivro, linha.NomeLeitor);
        return reserva;
    }
}
