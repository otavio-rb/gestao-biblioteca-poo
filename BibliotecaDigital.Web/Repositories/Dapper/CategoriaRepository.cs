using BibliotecaDigital.Web.Infrastructure;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Dapper;

namespace BibliotecaDigital.Web.Repositories.Dapper;

public sealed class CategoriaRepository : RepositorioDapperBase, ICategoriaRepository
{
    private sealed class LinhaCategoria
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;
    }

    public CategoriaRepository(IFabricaConexao fabricaConexao) : base(fabricaConexao)
    {
    }

    public IEnumerable<Categoria> ObterTodos() => Executar(conexao =>
    {
        const string sql = "SELECT Id, Nome, Descricao FROM Categoria ORDER BY Nome;";
        return conexao.Query<LinhaCategoria>(sql).Select(Mapear).ToList();
    }, "listar as categorias");

    public Categoria? ObterPorId(int id) => Executar(conexao =>
    {
        const string sql = "SELECT Id, Nome, Descricao FROM Categoria WHERE Id = @Id;";
        var linha = conexao.QuerySingleOrDefault<LinhaCategoria>(sql, new { Id = id });
        return linha is null ? null : Mapear(linha);
    }, "buscar a categoria");

    public IEnumerable<Categoria> ObterPorLivro(int livroId) => Executar(conexao =>
    {
        const string sql = """
            SELECT c.Id, c.Nome, c.Descricao
            FROM Categoria c
            INNER JOIN LivroCategoria lc ON lc.CategoriaId = c.Id
            WHERE lc.LivroId = @LivroId
            ORDER BY c.Nome;
            """;
        return conexao.Query<LinhaCategoria>(sql, new { LivroId = livroId }).Select(Mapear).ToList();
    }, "listar as categorias do livro");

    public int Inserir(Categoria entidade) => Executar(conexao =>
    {
        const string sql = """
            INSERT INTO Categoria (Nome, Descricao) VALUES (@Nome, @Descricao);
            SELECT last_insert_rowid();
            """;
        var id = conexao.ExecuteScalar<int>(sql, new { entidade.Nome, entidade.Descricao });
        entidade.DefinirId(id);
        return id;
    }, "cadastrar a categoria");

    public void Atualizar(Categoria entidade) => Executar(conexao =>
    {
        const string sql = "UPDATE Categoria SET Nome = @Nome, Descricao = @Descricao WHERE Id = @Id;";
        conexao.Execute(sql, new { entidade.Nome, entidade.Descricao, entidade.Id });
    }, "atualizar a categoria");

    public void Remover(int id) => Executar(conexao =>
    {
        conexao.Execute("DELETE FROM Categoria WHERE Id = @Id;", new { Id = id });
    }, "excluir a categoria");

    private static Categoria Mapear(LinhaCategoria linha)
        => Categoria.Reconstruir(linha.Id, linha.Nome, linha.Descricao);
}
