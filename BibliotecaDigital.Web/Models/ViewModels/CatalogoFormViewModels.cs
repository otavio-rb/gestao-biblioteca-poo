using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BibliotecaDigital.Web.Models.ViewModels;

public class AutorFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do autor.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 120 caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a nacionalidade.")]
    [Display(Name = "Nacionalidade")]
    public string Nacionalidade { get; set; } = string.Empty;

    public static AutorFormViewModel DoAutor(Autor autor)
        => new() { Id = autor.Id, Nome = autor.Nome, Nacionalidade = autor.Nacionalidade };
}

public class EditoraFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome da editora.")]
    [StringLength(120, ErrorMessage = "O nome deve ter no maximo 120 caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o endereco da editora.")]
    [StringLength(200, ErrorMessage = "O endereco deve ter no maximo 200 caracteres.")]
    [Display(Name = "Endereco")]
    public string Endereco { get; set; } = string.Empty;

    public static EditoraFormViewModel DaEditora(Editora editora)
        => new() { Id = editora.Id, Nome = editora.Nome, Endereco = editora.Endereco };
}

public class CategoriaFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome da categoria.")]
    [StringLength(80, ErrorMessage = "O nome deve ter no maximo 80 caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "A descricao deve ter no maximo 250 caracteres.")]
    [Display(Name = "Descricao")]
    public string Descricao { get; set; } = string.Empty;

    public static CategoriaFormViewModel DaCategoria(Categoria categoria)
        => new() { Id = categoria.Id, Nome = categoria.Nome, Descricao = categoria.Descricao };
}

public class LivroFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o titulo da obra.")]
    [StringLength(180, ErrorMessage = "O titulo deve ter no maximo 180 caracteres.")]
    [Display(Name = "Titulo")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o ISBN.")]
    [Display(Name = "ISBN")]
    public string Isbn { get; set; } = string.Empty;

    [Range(1, 2100, ErrorMessage = "Informe um ano de publicacao valido.")]
    [Display(Name = "Ano de publicacao")]
    public int AnoPublicacao { get; set; } = DateTime.Now.Year;

    [StringLength(600, ErrorMessage = "A descricao deve ter no maximo 600 caracteres.")]
    [Display(Name = "Descricao")]
    public string Descricao { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Selecione a editora.")]
    [Display(Name = "Editora")]
    public int EditoraId { get; set; }

    [Range(0.01, 10000, ErrorMessage = "Informe o valor de referencia da obra.")]
    [Display(Name = "Valor de referencia (R$)")]
    public decimal ValorReferencia { get; set; } = 50m;

    [MinLength(1, ErrorMessage = "Selecione ao menos um autor.")]
    [Display(Name = "Autores")]
    public List<int> AutorIds { get; set; } = new();

    [Display(Name = "Categorias")]
    public List<int> CategoriaIds { get; set; } = new();

    public IEnumerable<SelectListItem> Editoras { get; set; } = new List<SelectListItem>();

    public IEnumerable<SelectListItem> AutoresDisponiveis { get; set; } = new List<SelectListItem>();

    public IEnumerable<SelectListItem> CategoriasDisponiveis { get; set; } = new List<SelectListItem>();

    public bool EhEdicao => Id > 0;

    public static LivroFormViewModel DoLivro(Livro livro, decimal valorReferencia) => new()
    {
        Id = livro.Id,
        Titulo = livro.Titulo,
        Isbn = livro.Isbn,
        AnoPublicacao = livro.AnoPublicacao,
        Descricao = livro.Descricao,
        EditoraId = livro.Editora.Id,
        ValorReferencia = valorReferencia,
        AutorIds = livro.Autores.Select(autor => autor.Id).ToList(),
        CategoriaIds = livro.Categorias.Select(categoria => categoria.Id).ToList()
    };
}

public class ExemplarFormViewModel
{
    public int Id { get; set; }

    public int LivroId { get; set; }

    public string TituloLivro { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o formato do exemplar.")]
    [Display(Name = "Formato")]
    public string Formato { get; set; } = "EPUB";

    [Required(ErrorMessage = "Informe o nome do arquivo.")]
    [Display(Name = "Arquivo")]
    public string Arquivo { get; set; } = string.Empty;

    [Range(1, 500, ErrorMessage = "O total de licencas deve ficar entre 1 e 500.")]
    [Display(Name = "Licencas totais")]
    public int LicencasTotais { get; set; } = 1;

    [Display(Name = "Disponivel para emprestimo")]
    public bool Disponivel { get; set; } = true;

    public bool EhEdicao => Id > 0;

    public static ExemplarFormViewModel DoExemplar(ExemplarDigital exemplar, int livroId, string tituloLivro, int licencasTotais) => new()
    {
        Id = exemplar.Id,
        LivroId = livroId,
        TituloLivro = tituloLivro,
        Formato = exemplar.Formato,
        Arquivo = exemplar.Arquivo,
        LicencasTotais = licencasTotais,
        Disponivel = exemplar.VerificarDisponibilidade()
    };
}

public class LivroDetalhesViewModel
{
    public required Livro Livro { get; init; }

    public decimal ValorReferencia { get; init; }

    public IReadOnlyList<Avaliacao> Avaliacoes { get; init; } = new List<Avaliacao>();

    public IReadOnlyList<Comentario> Comentarios { get; init; } = new List<Comentario>();

    public IReadOnlyDictionary<int, string> NomesDeUsuarios { get; init; } = new Dictionary<int, string>();

    public IEnumerable<SelectListItem> Leitores { get; init; } = new List<SelectListItem>();

    public double NotaMedia => Avaliacoes.Count == 0 ? 0 : Math.Round(Avaliacoes.Average(avaliacao => avaliacao.Nota), 1);

    public IReadOnlyList<Comentario> Raizes
        => Comentarios.Where(comentario => comentario.ComentarioPaiId is null).ToList();

    public IReadOnlyList<Comentario> RespostasDe(int comentarioId)
        => Comentarios.Where(comentario => comentario.ComentarioPaiId == comentarioId).ToList();

    public string NomeDe(int usuarioId)
        => NomesDeUsuarios.TryGetValue(usuarioId, out var nome) ? nome : $"Usuario {usuarioId}";
}
