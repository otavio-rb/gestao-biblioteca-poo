using BibliotecaDigital.Web.Infrastructure;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Dapper;

namespace BibliotecaDigital.Web.Repositories.Dapper;

public sealed class ComentarioRepository : RepositorioDapperBase, IComentarioRepository
{
    private const string Selecao = """
        SELECT c.Id, c.UsuarioId, c.LivroId, c.ComentarioPaiId, c.Texto,
               c.DataCriacao, c.Editado, c.Excluido, u.Nome AS NomeAutor
        FROM Comentario c
        INNER JOIN Usuario u ON u.Id = c.UsuarioId
        """;

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

        public string NomeAutor { get; set; } = string.Empty;
    }

    public ComentarioRepository(IFabricaConexao fabricaConexao) : base(fabricaConexao)
    {
    }

    public IEnumerable<Comentario> ObterTodos() => Executar(conexao =>
    {
        var sql = $"{Selecao} ORDER BY c.DataCriacao;";
        return conexao.Query<LinhaComentario>(sql).Select(Mapear).ToList();
    }, "listar os comentarios");

    public IEnumerable<Comentario> ObterPorLivro(int livroId) => Executar(conexao =>
    {
        var sql = $"{Selecao} WHERE c.LivroId = @LivroId ORDER BY c.DataCriacao;";
        return conexao.Query<LinhaComentario>(sql, new { LivroId = livroId }).Select(Mapear).ToList();
    }, "listar os comentarios do livro");

    public Comentario? ObterPorId(int id) => Executar(conexao =>
    {
        var sql = $"{Selecao} WHERE c.Id = @Id;";
        var linha = conexao.QuerySingleOrDefault<LinhaComentario>(sql, new { Id = id });
        return linha is null ? null : Mapear(linha);
    }, "buscar o comentario");

    public int Inserir(Comentario entidade) => Executar(conexao =>
    {
        const string sql = """
            INSERT INTO Comentario (UsuarioId, LivroId, ComentarioPaiId, Texto, DataCriacao, Editado, Excluido)
            VALUES (@UsuarioId, @LivroId, @ComentarioPaiId, @Texto, @DataCriacao, @Editado, @Excluido);
            SELECT last_insert_rowid();
            """;

        var id = conexao.ExecuteScalar<int>(sql, new
        {
            entidade.UsuarioId,
            entidade.LivroId,
            entidade.ComentarioPaiId,
            entidade.Texto,
            entidade.DataCriacao,
            entidade.Editado,
            entidade.Excluido
        });

        entidade.DefinirId(id);
        return id;
    }, "registrar o comentario");

    public void Atualizar(Comentario entidade) => Executar(conexao =>
    {
        const string sql = "UPDATE Comentario SET Texto = @Texto, Editado = @Editado, Excluido = @Excluido WHERE Id = @Id;";
        conexao.Execute(sql, new { entidade.Texto, entidade.Editado, entidade.Excluido, entidade.Id });
    }, "atualizar o comentario");

    // A tela usa exclusao logica (Comentario.Excluir); este metodo existe para cumprir o contrato IRepository.
    public void Remover(int id) => Executar(conexao =>
    {
        conexao.Execute("DELETE FROM Comentario WHERE Id = @Id;", new { Id = id });
    }, "excluir o comentario");

    private static Comentario Mapear(LinhaComentario linha)
    {
        var comentario = Comentario.Reconstruir(
            linha.Id,
            linha.UsuarioId,
            linha.LivroId,
            linha.ComentarioPaiId,
            linha.Texto,
            linha.DataCriacao,
            linha.Editado,
            linha.Excluido);

        comentario.DescreverContexto(linha.NomeAutor);
        return comentario;
    }
}
