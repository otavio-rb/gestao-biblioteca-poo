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

    [Range(1, 10000, ErrorMessage = "Informe o total de paginas (de 1 a 10000).")]
    [Display(Name = "Total de paginas")]
    public int TotalPaginas { get; set; }

    public IEnumerable<SelectListItem> Leitores { get; set; } = new List<SelectListItem>();

    public IEnumerable<SelectListItem> Livros { get; set; } = new List<SelectListItem>();
}
