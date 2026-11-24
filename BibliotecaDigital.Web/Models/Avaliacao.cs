using BibliotecaDigital.Web.Exceptions;

namespace BibliotecaDigital.Web.Models;

public sealed class Avaliacao
{
    public const int NotaMinima = 1;
    public const int NotaMaxima = 5;

    public Avaliacao(Leitura leitura, int nota)
    {
        if (leitura == null)
        {
            throw new DominioException("A avaliacao precisa de uma leitura.");
        }

        // Regra: so avalia depois de terminar o livro.
        if (!leitura.EstaConcluida())
        {
            throw new DominioException("So e possivel avaliar um livro depois de concluir a leitura.");
        }

        ValidarNota(nota);

        UsuarioId = leitura.UsuarioId;
        LivroId = leitura.LivroId;
        Nota = nota;
        DataAvaliacao = DateTime.Now;
    }

    // Usado apenas pelo Reconstruir (dados vindos do banco).
    private Avaliacao()
    {
    }

    public int Id { get; private set; }

    public int UsuarioId { get; private set; }

    public int LivroId { get; private set; }

    public int Nota { get; private set; }

    public DateTime DataAvaliacao { get; private set; }

    public void AlterarNota(int novaNota)
    {
        ValidarNota(novaNota);
        Nota = novaNota;
        DataAvaliacao = DateTime.Now;
    }

    public int GetNota()
    {
        return Nota;
    }

    // Regra: nota inteira de 1 a 5.
    private void ValidarNota(int nota)
    {
        if (nota < NotaMinima || nota > NotaMaxima)
        {
            throw new DominioException($"A nota deve ser de {NotaMinima} a {NotaMaxima}.");
        }
    }

    public void DefinirId(int id) => Id = id;

    public static Avaliacao Reconstruir(int id, int usuarioId, int livroId, int nota, DateTime dataAvaliacao)
    {
        return new Avaliacao
        {
            Id = id,
            UsuarioId = usuarioId,
            LivroId = livroId,
            Nota = nota,
            DataAvaliacao = dataAvaliacao
        };
    }
}
