using BibliotecaDigital.Web.Infrastructure;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Dapper;

namespace BibliotecaDigital.Web.Repositories.Dapper;

public sealed class AutorRepository : RepositorioDapperBase, IAutorRepository
{
    private sealed class LinhaAutor
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Nacionalidade { get; set; } = string.Empty;
    }

    public AutorRepository(IFabricaConexao fabricaConexao) : base(fabricaConexao)
    {
    }

    public IEnumerable<Autor> ObterTodos() => Executar(conexao =>
    {
        const string sql = "SELECT Id, Nome, Nacionalidade FROM Autor ORDER BY Nome;";
        return conexao.Query<LinhaAutor>(sql).Select(Mapear).ToList();
    }, "listar os autores");

    public Autor? ObterPorId(int id) => Executar(conexao =>
    {
        const string sql = "SELECT Id, Nome, Nacionalidade FROM Autor WHERE Id = @Id;";
        var linha = conexao.QuerySingleOrDefault<LinhaAutor>(sql, new { Id = id });
        return linha is null ? null : Mapear(linha);
    }, "buscar o autor");

    public IEnumerable<Autor> ObterPorLivro(int livroId) => Executar(conexao =>
    {
        const string sql = """
            SELECT a.Id, a.Nome, a.Nacionalidade
            FROM Autor a
            INNER JOIN LivroAutor la ON la.AutorId = a.Id
            WHERE la.LivroId = @LivroId
            ORDER BY a.Nome;
            """;
        return conexao.Query<LinhaAutor>(sql, new { LivroId = livroId }).Select(Mapear).ToList();
    }, "listar os autores do livro");

    public int Inserir(Autor entidade) => Executar(conexao =>
    {
        const string sql = """
            INSERT INTO Autor (Nome, Nacionalidade) VALUES (@Nome, @Nacionalidade);
            SELECT last_insert_rowid();
            """;
        var id = conexao.ExecuteScalar<int>(sql, new { entidade.Nome, entidade.Nacionalidade });
        entidade.DefinirId(id);
        return id;
    }, "cadastrar o autor");

    public void Atualizar(Autor entidade) => Executar(conexao =>
    {
        const string sql = "UPDATE Autor SET Nome = @Nome, Nacionalidade = @Nacionalidade WHERE Id = @Id;";
        conexao.Execute(sql, new { entidade.Nome, entidade.Nacionalidade, entidade.Id });
    }, "atualizar o autor");

    public void Remover(int id) => Executar(conexao =>
    {
        conexao.Execute("DELETE FROM Autor WHERE Id = @Id;", new { Id = id });
    }, "excluir o autor");

    public bool PossuiLivrosVinculados(int autorId) => Executar(conexao =>
    {
        const string sql = "SELECT COUNT(1) FROM LivroAutor WHERE AutorId = @AutorId;";
        return conexao.ExecuteScalar<long>(sql, new { AutorId = autorId }) > 0;
    }, "verificar os livros do autor");

    private static Autor Mapear(LinhaAutor linha)
        => Autor.Reconstruir(linha.Id, linha.Nome, linha.Nacionalidade);
}
