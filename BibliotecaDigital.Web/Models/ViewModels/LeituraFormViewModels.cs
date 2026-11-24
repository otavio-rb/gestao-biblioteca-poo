using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BibliotecaDigital.Web.Models.ViewModels;

public class LeituraFormViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecione o leitor.")]
    [Display(Name = "Leitor")]
    public int UsuarioId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione o livro.")]
    [Display(Name = "Livro")]
    public int LivroId { get; set; }

    [Range(1, 20000, ErrorMessage = "Informe o total de paginas da obra.")]
    [Display(Name = "Total de paginas")]
    public int TotalPaginas { get; set; } = 200;

    public IEnumerable<SelectListItem> Leitores { get; set; } = new List<SelectListItem>();

    public IEnumerable<SelectListItem> Livros { get; set; } = new List<SelectListItem>();
}

public class RegistrarPaginaViewModel
{
    public int LeituraId { get; set; }

    [Range(0, 20000, ErrorMessage = "Informe uma pagina valida.")]
    [Display(Name = "Pagina atual")]
    public int Pagina { get; set; }
}

public class LeituraDetalhesViewModel
{
    public required Leitura Leitura { get; init; }

    public string NomeLeitor { get; init; } = string.Empty;

    public string TituloLivro { get; init; } = string.Empty;
}

public class LeituraListaViewModel
{
    public required Leitura Leitura { get; init; }

    public string NomeLeitor { get; init; } = string.Empty;

    public string TituloLivro { get; init; } = string.Empty;
}

public class AvaliacaoFormViewModel
{
    public int LivroId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione o leitor.")]
    public int UsuarioId { get; set; }

    [Range(1, 5, ErrorMessage = "A nota deve ser de 1 a 5.")]
    public int Nota { get; set; } = 5;
}

public class ComentarioFormViewModel
{
    public int LivroId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione o leitor.")]
    public int UsuarioId { get; set; }

    public int? ComentarioPaiId { get; set; }

    [Required(ErrorMessage = "Escreva o comentario.")]
    [StringLength(500, ErrorMessage = "O comentario deve ter no maximo 500 caracteres.")]
    public string Texto { get; set; } = string.Empty;
}
