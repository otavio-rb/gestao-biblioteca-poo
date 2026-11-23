using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BibliotecaDigital.Web.Models.ViewModels;

public class ReservaFormViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecione o leitor.")]
    [Display(Name = "Leitor")]
    public int LeitorId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione o exemplar digital.")]
    [Display(Name = "Exemplar digital")]
    public int ExemplarId { get; set; }

    public IEnumerable<SelectListItem> Leitores { get; set; } = new List<SelectListItem>();

    public IEnumerable<SelectListItem> Exemplares { get; set; } = new List<SelectListItem>();
}
