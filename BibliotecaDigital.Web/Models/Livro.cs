using BibliotecaDigital.Web.Exceptions;

namespace BibliotecaDigital.Web.Models;

public sealed class Livro
{
    // Associacao "escrito por" (1..* autores)
    private readonly List<Autor> _autores = new();

    // Agregacao "classificado em" (0..* categorias): a categoria existe sem o livro.
    private readonly List<Categoria> _categorias = new();

    // Composicao "possui" (1..* exemplares): o exemplar so existe dentro do livro.
    private readonly List<ExemplarDigital> _exemplares = new();

    // O construtor faz o papel do "cadastrar".
    // Regra 2: precisa de pelo menos um autor. Regra 3: precisa de uma editora.
    public Livro(string titulo, string isbn, int anoPublicacao, string descricao, Editora editora, Autor autor)
    {
        Editar(titulo, isbn, anoPublicacao, descricao, editora);
        AdicionarAutor(autor);
    }

    public int Id { get; private set; }

    public string Titulo { get; private set; } = string.Empty;

    public string Isbn { get; private set; } = string.Empty;

    public int AnoPublicacao { get; private set; }

    public string Descricao { get; private set; } = string.Empty;

    // "publicado por": todo livro tem exatamente uma editora.
    public Editora Editora { get; private set; } = null!;

    public IReadOnlyList<Autor> Autores => _autores;

    public IReadOnlyList<Categoria> Categorias => _categorias;

    public IReadOnlyList<ExemplarDigital> Exemplares => _exemplares;

    public void Editar(string titulo, string isbn, int anoPublicacao, string descricao, Editora editora)
    {
        // Regra 1: todo livro deve possuir titulo e ISBN.
        if (string.IsNullOrWhiteSpace(titulo))
        {
            throw new DominioException("O titulo do livro e obrigatorio.");
        }

        if (anoPublicacao <= 0 || anoPublicacao > DateTime.Now.Year)
        {
            throw new DominioException("Informe um ano de publicacao valido.");
        }

        // Regra 3: todo livro deve possuir uma editora.
        if (editora == null)
        {
            throw new DominioException("Todo livro deve possuir uma editora.");
        }

        Titulo = titulo.Trim();
        Isbn = ValidarIsbn(isbn);
        AnoPublicacao = anoPublicacao;
        Descricao = descricao == null ? string.Empty : descricao.Trim();
        Editora = editora;
    }

    public void AdicionarAutor(Autor autor)
    {
        if (autor == null)
        {
            throw new DominioException("Informe o autor do livro.");
        }

        if (_autores.Contains(autor))
        {
            throw new DominioException("Este autor ja esta associado ao livro.");
        }

        _autores.Add(autor);
    }

    public void RemoverAutor(Autor autor)
    {
        // Regra 2: o livro nao pode ficar sem autor.
        if (_autores.Count == 1)
        {
            throw new DominioException("O livro deve ter pelo menos um autor.");
        }

        _autores.Remove(autor);
    }

    // Regra 4: um livro pode pertencer a uma ou mais categorias.
    public void Classificar(Categoria categoria)
    {
        if (categoria == null)
        {
            throw new DominioException("Informe a categoria.");
        }

        if (_categorias.Contains(categoria))
        {
            throw new DominioException("O livro ja esta nesta categoria.");
        }

        _categorias.Add(categoria);
    }

    // Composicao: o proprio livro cria o exemplar.
    public ExemplarDigital AdicionarExemplar(string formato, string arquivo)
    {
        var exemplar = new ExemplarDigital(formato, arquivo);
        _exemplares.Add(exemplar);
        return exemplar;
    }

    public void RemoverExemplar(ExemplarDigital exemplar)
    {
        // Regra 5: o livro deve ter pelo menos um exemplar digital.
        if (_exemplares.Count == 1)
        {
            throw new DominioException("O livro deve ter pelo menos um exemplar digital.");
        }

        _exemplares.Remove(exemplar);
    }

    // Regra 5: usado antes de salvar, para garantir que o livro ja tem exemplar.
    public bool PossuiExemplar()
    {
        return _exemplares.Count > 0;
    }

    // Aceita ISBN com ou sem tracos, desde que tenha 10 ou 13 digitos.
    // Regra 6 (ISBN unico) e verificada no repositorio, que consulta o banco.
    private static string ValidarIsbn(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new DominioException("O ISBN do livro e obrigatorio.");
        }

        var somenteNumeros = isbn.Replace("-", "").Replace(" ", "");

        if (somenteNumeros.Length != 10 && somenteNumeros.Length != 13)
        {
            throw new DominioException("O ISBN deve ter 10 ou 13 digitos.");
        }

        return somenteNumeros;
    }

    public void DefinirId(int id) => Id = id;

    public static Livro Reconstruir(
        int id,
        string titulo,
        string isbn,
        int anoPublicacao,
        string descricao,
        Editora editora,
        List<Autor> autores,
        List<Categoria> categorias,
        List<ExemplarDigital> exemplares)
    {
        var livro = new Livro(titulo, isbn, anoPublicacao, descricao, editora, autores[0]) { Id = id };

        livro._autores.Clear();
        livro._autores.AddRange(autores);
        livro._categorias.AddRange(categorias);
        livro._exemplares.AddRange(exemplares);

        return livro;
    }
}
