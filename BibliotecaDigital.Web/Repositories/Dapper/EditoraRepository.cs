using BibliotecaDigital.Web.Infrastructure;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Dapper;

namespace BibliotecaDigital.Web.Repositories.Dapper;

public sealed class EditoraRepository : RepositorioDapperBase, IEditoraRepository
{
    private sealed class LinhaEditora
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Endereco { get; set; } = string.Empty;
    }

    public EditoraRepository(IFabricaConexao fabricaConexao) : base(fabricaConexao)
    {
    }

    public IEnumerable<Editora> ObterTodos() => Executar(conexao =>
    {
        const string sql = "SELECT Id, Nome, Endereco FROM Editora ORDER BY Nome;";
        return conexao.Query<LinhaEditora>(sql).Select(Mapear).ToList();
    }, "listar as editoras");

    public Editora? ObterPorId(int id) => Executar(conexao =>
    {
        const string sql = "SELECT Id, Nome, Endereco FROM Editora WHERE Id = @Id;";
        var linha = conexao.QuerySingleOrDefault<LinhaEditora>(sql, new { Id = id });
        return linha is null ? null : Mapear(linha);
    }, "buscar a editora");

    public int Inserir(Editora entidade) => Executar(conexao =>
    {
        const string sql = """
            INSERT INTO Editora (Nome, Endereco) VALUES (@Nome, @Endereco);
            SELECT last_insert_rowid();
            """;
        var id = conexao.ExecuteScalar<int>(sql, new { entidade.Nome, entidade.Endereco });
        entidade.DefinirId(id);
        return id;
    }, "cadastrar a editora");

    public void Atualizar(Editora entidade) => Executar(conexao =>
    {
        const string sql = "UPDATE Editora SET Nome = @Nome, Endereco = @Endereco WHERE Id = @Id;";
        conexao.Execute(sql, new { entidade.Nome, entidade.Endereco, entidade.Id });
    }, "atualizar a editora");

    public void Remover(int id) => Executar(conexao =>
    {
        conexao.Execute("DELETE FROM Editora WHERE Id = @Id;", new { Id = id });
    }, "excluir a editora");

    public bool PossuiLivrosVinculados(int editoraId) => Executar(conexao =>
    {
        const string sql = "SELECT COUNT(1) FROM Livro WHERE EditoraId = @EditoraId;";
        return conexao.ExecuteScalar<long>(sql, new { EditoraId = editoraId }) > 0;
    }, "verificar os livros da editora");

    private static Editora Mapear(LinhaEditora linha)
        => Editora.Reconstruir(linha.Id, linha.Nome, linha.Endereco);
}
