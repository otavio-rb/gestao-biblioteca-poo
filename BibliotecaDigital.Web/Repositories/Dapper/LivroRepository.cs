using BibliotecaDigital.Web.Exceptions;
using BibliotecaDigital.Web.Infrastructure;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Dapper;

namespace BibliotecaDigital.Web.Repositories.Dapper;

public sealed class LivroRepository : RepositorioDapperBase, ILivroRepository
{
    private const string ColunasPadrao = "Id, Titulo, Isbn, AnoPublicacao, Descricao, EditoraId, ValorReferencia";

    private sealed class LinhaLivro
    {
        public int Id { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Isbn { get; set; } = string.Empty;

        public int AnoPublicacao { get; set; }

        public string Descricao { get; set; } = string.Empty;

        public int EditoraId { get; set; }

        public decimal ValorReferencia { get; set; }
    }

    private sealed class LinhaEditora
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Endereco { get; set; } = string.Empty;
    }

    private sealed class LinhaAutorDoLivro
    {
        public int LivroId { get; set; }

        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Nacionalidade { get; set; } = string.Empty;
    }

    private sealed class LinhaCategoriaDoLivro
    {
        public int LivroId { get; set; }

        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;
    }

    private sealed class LinhaExemplarDoLivro
    {
        public int Id { get; set; }

        public int LivroId { get; set; }

        public string Formato { get; set; } = string.Empty;

        public string Arquivo { get; set; } = string.Empty;

        public int LicencasDisponiveis { get; set; }
    }

    public LivroRepository(IFabricaConexao fabricaConexao) : base(fabricaConexao)
    {
    }

    public IEnumerable<Livro> ObterTodos() => Pesquisar(null);

    public IEnumerable<Livro> Pesquisar(string? termo) => Executar(conexao =>
    {
        var sql = $"""
            SELECT {ColunasPadrao}
            FROM Livro
            WHERE (@Termo IS NULL OR Titulo LIKE @Filtro OR Isbn LIKE @Filtro)
            ORDER BY Titulo;
            """;

        var termoLimpo = string.IsNullOrWhiteSpace(termo) ? null : termo.Trim();
        var linhas = conexao.Query<LinhaLivro>(sql, new { Termo = termoLimpo, Filtro = $"%{termoLimpo}%" }).ToList();

        return linhas.Count == 0 ? new List<Livro>() : Montar(conexao, linhas);
    }, "listar os livros");

    public Livro? ObterPorId(int id) => ObterComDetalhes(id);

    public Livro? ObterComDetalhes(int id) => Executar(conexao =>
    {
        var sql = $"SELECT {ColunasPadrao} FROM Livro WHERE Id = @Id;";
        var linha = conexao.QuerySingleOrDefault<LinhaLivro>(sql, new { Id = id });

        return linha is null ? null : Montar(conexao, new List<LinhaLivro> { linha }).FirstOrDefault();
    }, "carregar os detalhes do livro");

    public bool IsbnEmUso(string isbn, int idIgnorado = 0) => Executar(conexao =>
    {
        const string sql = "SELECT COUNT(1) FROM Livro WHERE Isbn = @Isbn AND Id <> @Id;";
        return conexao.ExecuteScalar<long>(sql, new { Isbn = isbn, Id = idIgnorado }) > 0;
    }, "verificar o ISBN informado");

    public decimal ObterValorReferencia(int livroId) => Executar(conexao =>
    {
        const string sql = "SELECT ValorReferencia FROM Livro WHERE Id = @Id;";
        return conexao.ExecuteScalar<decimal>(sql, new { Id = livroId });
    }, "buscar o valor de referencia do livro");

    public int Inserir(Livro entidade) => Inserir(entidade, 1m);

    public int Inserir(Livro entidade, decimal valorReferencia) => Executar(conexao =>
    {
        const string sql = """
            INSERT INTO Livro (Titulo, Isbn, AnoPublicacao, Descricao, EditoraId, ValorReferencia)
            VALUES (@Titulo, @Isbn, @AnoPublicacao, @Descricao, @EditoraId, @ValorReferencia);
            SELECT last_insert_rowid();
            """;

        var id = conexao.ExecuteScalar<int>(sql, new
        {
            entidade.Titulo,
            entidade.Isbn,
            entidade.AnoPublicacao,
            entidade.Descricao,
            EditoraId = entidade.Editora.Id,
            ValorReferencia = valorReferencia
        });

        entidade.DefinirId(id);
        return id;
    }, "cadastrar o livro");

    public void Atualizar(Livro entidade) => Atualizar(entidade, ObterValorReferencia(entidade.Id));

    public void Atualizar(Livro entidade, decimal valorReferencia) => Executar(conexao =>
    {
        const string sql = """
            UPDATE Livro SET
                Titulo = @Titulo,
                Isbn = @Isbn,
                AnoPublicacao = @AnoPublicacao,
                Descricao = @Descricao,
                EditoraId = @EditoraId,
                ValorReferencia = @ValorReferencia
            WHERE Id = @Id;
            """;

        conexao.Execute(sql, new
        {
            entidade.Titulo,
            entidade.Isbn,
            entidade.AnoPublicacao,
            entidade.Descricao,
            EditoraId = entidade.Editora.Id,
            ValorReferencia = valorReferencia,
            entidade.Id
        });
    }, "atualizar o livro");

    public void Remover(int id) => Executar(conexao =>
    {
        const string sqlVinculos = """
            SELECT COUNT(1)
            FROM Emprestimo e
            INNER JOIN ExemplarDigital x ON x.Id = e.ExemplarId
            WHERE x.LivroId = @Id AND e.Status IN (1, 2);
            """;

        if (conexao.ExecuteScalar<long>(sqlVinculos, new { Id = id }) > 0)
        {
            throw new DominioException("Existem emprestimos ativos para esta obra; ela nao pode ser excluida.");
        }

        conexao.Execute("DELETE FROM Livro WHERE Id = @Id;", new { Id = id });
    }, "excluir o livro");

    public void DefinirAutores(int livroId, IEnumerable<int> autorIds) => Executar(conexao =>
    {
        var ids = autorIds.Distinct().ToList();

        if (ids.Count == 0)
        {
            throw new DominioException("O livro deve ter pelo menos um autor.");
        }

        using var transacao = conexao.BeginTransaction();

        conexao.Execute("DELETE FROM LivroAutor WHERE LivroId = @LivroId;", new { LivroId = livroId }, transacao);

        const string sql = "INSERT INTO LivroAutor (LivroId, AutorId) VALUES (@LivroId, @AutorId);";
        conexao.Execute(sql, ids.Select(autorId => new { LivroId = livroId, AutorId = autorId }).ToList(), transacao);

        transacao.Commit();
    }, "vincular os autores do livro");

    public void DefinirCategorias(int livroId, IEnumerable<int> categoriaIds) => Executar(conexao =>
    {
        using var transacao = conexao.BeginTransaction();

        conexao.Execute("DELETE FROM LivroCategoria WHERE LivroId = @LivroId;", new { LivroId = livroId }, transacao);

        var ids = categoriaIds.Distinct().ToList();

        if (ids.Count > 0)
        {
            const string sql = "INSERT INTO LivroCategoria (LivroId, CategoriaId) VALUES (@LivroId, @CategoriaId);";
            conexao.Execute(sql, ids.Select(categoriaId => new { LivroId = livroId, CategoriaId = categoriaId }).ToList(), transacao);
        }

        transacao.Commit();
    }, "vincular as categorias do livro");

    private static List<Livro> Montar(System.Data.IDbConnection conexao, List<LinhaLivro> linhas)
    {
        var ids = linhas.Select(linha => linha.Id).ToList();
        var editoraIds = linhas.Select(linha => linha.EditoraId).Distinct().ToList();

        const string sqlEditoras = "SELECT Id, Nome, Endereco FROM Editora WHERE Id IN @Ids;";
        var editoras = conexao.Query<LinhaEditora>(sqlEditoras, new { Ids = editoraIds })
            .ToDictionary(linha => linha.Id, linha => Editora.Reconstruir(linha.Id, linha.Nome, linha.Endereco));

        const string sqlAutores = """
            SELECT la.LivroId, a.Id, a.Nome, a.Nacionalidade
            FROM Autor a
            INNER JOIN LivroAutor la ON la.AutorId = a.Id
            WHERE la.LivroId IN @Ids
            ORDER BY a.Nome;
            """;

        var autores = conexao.Query<LinhaAutorDoLivro>(sqlAutores, new { Ids = ids })
            .GroupBy(linha => linha.LivroId)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.Select(linha => Autor.Reconstruir(linha.Id, linha.Nome, linha.Nacionalidade)).ToList());

        const string sqlCategorias = """
            SELECT lc.LivroId, c.Id, c.Nome, c.Descricao
            FROM Categoria c
            INNER JOIN LivroCategoria lc ON lc.CategoriaId = c.Id
            WHERE lc.LivroId IN @Ids
            ORDER BY c.Nome;
            """;

        var categorias = conexao.Query<LinhaCategoriaDoLivro>(sqlCategorias, new { Ids = ids })
            .GroupBy(linha => linha.LivroId)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.Select(linha => Categoria.Reconstruir(linha.Id, linha.Nome, linha.Descricao)).ToList());

        const string sqlExemplares = """
            SELECT Id, LivroId, Formato, Arquivo, LicencasDisponiveis
            FROM ExemplarDigital
            WHERE LivroId IN @Ids
            ORDER BY Id;
            """;

        var exemplares = conexao.Query<LinhaExemplarDoLivro>(sqlExemplares, new { Ids = ids })
            .GroupBy(linha => linha.LivroId)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo
                    .Select(linha => ExemplarDigital.Reconstruir(linha.Id, linha.Formato, linha.Arquivo, linha.LicencasDisponiveis > 0))
                    .ToList());

        var resultado = new List<Livro>();

        foreach (var linha in linhas)
        {
            if (!editoras.TryGetValue(linha.EditoraId, out var editora))
            {
                continue;
            }

            var autoresDoLivro = autores.TryGetValue(linha.Id, out var lista) ? lista : new List<Autor>();

            if (autoresDoLivro.Count == 0)
            {
                continue;
            }

            resultado.Add(Livro.Reconstruir(
                linha.Id,
                linha.Titulo,
                linha.Isbn,
                linha.AnoPublicacao,
                linha.Descricao,
                editora,
                autoresDoLivro,
                categorias.TryGetValue(linha.Id, out var cats) ? cats : new List<Categoria>(),
                exemplares.TryGetValue(linha.Id, out var exs) ? exs : new List<ExemplarDigital>()));
        }

        return resultado;
    }
}
