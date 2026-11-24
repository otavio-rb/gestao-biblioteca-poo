using System.ComponentModel.DataAnnotations;

namespace BibliotecaDigital.Web.Models.ViewModels;

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
