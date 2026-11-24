using Microsoft.AspNetCore.Mvc.Rendering;

namespace BibliotecaDigital.Web.Models.ViewModels;

public class LeiturasIndexViewModel
{
    public List<Leitura> Leituras { get; set; } = new();

    // Preenchido quando a lista esta filtrada por um leitor.
    public int? UsuarioId { get; set; }

    public HistoricoLeitura? Historico { get; set; }

    public List<Favorito> Favoritos { get; set; } = new();

    public IEnumerable<SelectListItem> Leitores { get; set; } = new List<SelectListItem>();
}
