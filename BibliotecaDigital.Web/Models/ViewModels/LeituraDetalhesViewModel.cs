namespace BibliotecaDigital.Web.Models.ViewModels;

public class LeituraDetalhesViewModel
{
    public Leitura Leitura { get; set; } = null!;

    // Avaliacao do leitor desta leitura (null se ainda nao avaliou).
    public Avaliacao? Avaliacao { get; set; }

    public double? MediaLivro { get; set; }

    // Todos os comentarios sobre o livro, de qualquer leitor.
    public List<Comentario> Comentarios { get; set; } = new();

    public bool EhFavorito { get; set; }

    public List<Comentario> ComentariosPrincipais()
    {
        var resultado = new List<Comentario>();

        foreach (var comentario in Comentarios)
        {
            if (comentario.ComentarioPaiId == null)
            {
                resultado.Add(comentario);
            }
        }

        return resultado;
    }

    public List<Comentario> RespostasDe(int comentarioId)
    {
        var resultado = new List<Comentario>();

        foreach (var comentario in Comentarios)
        {
            if (comentario.ComentarioPaiId == comentarioId)
            {
                resultado.Add(comentario);
            }
        }

        return resultado;
    }
}
