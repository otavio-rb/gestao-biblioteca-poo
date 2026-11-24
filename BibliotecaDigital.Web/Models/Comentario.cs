using BibliotecaDigital.Web.Exceptions;
using BibliotecaDigital.Web.Models.Enums;

namespace BibliotecaDigital.Web.Models;

public sealed class Comentario
{
    public const int TamanhoMaximoTexto = 500;

    private string _texto = string.Empty;

    public Comentario(Leitura leitura, string texto)
    {
        if (leitura == null)
        {
            throw new DominioException("O comentario precisa de uma leitura.");
        }

        // Regra: exige leitura iniciada.
        if (leitura.Status == StatusLeitura.NaoIniciada)
        {
            throw new DominioException("Comece a leitura antes de comentar o livro.");
        }

        ValidarTexto(texto);

        UsuarioId = leitura.UsuarioId;
        LivroId = leitura.LivroId;
        Texto = texto;
        DataCriacao = DateTime.Now;
        Editado = false;
        Excluido = false;
    }

    // Usado apenas pelo Reconstruir (dados vindos do banco).
    private Comentario()
    {
    }

    public int Id { get; private set; }

    public int UsuarioId { get; private set; }

    public int LivroId { get; private set; }

    // Auto-associacao "responde a": null quando o comentario nao e resposta.
    public int? ComentarioPaiId { get; private set; }

    public string Texto
    {
        get => _texto;
        private set => _texto = value.Trim();
    }

    public DateTime DataCriacao { get; private set; }

    public bool Editado { get; private set; }

    public bool Excluido { get; private set; }

    public void Editar(int autorId, string novoTexto)
    {
        GarantirQueEhAutor(autorId);

        if (Excluido)
        {
            throw new DominioException("Um comentario excluido nao pode ser editado.");
        }

        ValidarTexto(novoTexto);
        Texto = novoTexto;
        Editado = true;
    }

    // Exclusao logica: o registro continua, so fica marcado como excluido.
    public void Excluir(int autorId)
    {
        GarantirQueEhAutor(autorId);

        if (Excluido)
        {
            throw new DominioException("Este comentario ja foi excluido.");
        }

        Excluido = true;
    }

    public Comentario Responder(Leitura leitura, string texto)
    {
        if (Excluido)
        {
            throw new DominioException("Nao e possivel responder um comentario excluido.");
        }

        var resposta = new Comentario(leitura, texto);

        if (resposta.LivroId != LivroId)
        {
            throw new DominioException("A resposta precisa ser sobre o mesmo livro.");
        }

        resposta.ComentarioPaiId = Id;
        return resposta;
    }

    // Regra: texto obrigatorio, ate 500 caracteres.
    private void ValidarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new DominioException("O texto do comentario e obrigatorio.");
        }

        if (texto.Trim().Length > TamanhoMaximoTexto)
        {
            throw new DominioException($"O comentario pode ter no maximo {TamanhoMaximoTexto} caracteres.");
        }
    }

    // Regra: so o autor edita ou exclui.
    private void GarantirQueEhAutor(int autorId)
    {
        if (autorId != UsuarioId)
        {
            throw new DominioException("Somente o autor pode alterar este comentario.");
        }
    }

    public void DefinirId(int id) => Id = id;

    public static Comentario Reconstruir(
        int id,
        int usuarioId,
        int livroId,
        int? comentarioPaiId,
        string texto,
        DateTime dataCriacao,
        bool editado,
        bool excluido)
    {
        return new Comentario
        {
            Id = id,
            UsuarioId = usuarioId,
            LivroId = livroId,
            ComentarioPaiId = comentarioPaiId,
            Texto = texto,
            DataCriacao = dataCriacao,
            Editado = editado,
            Excluido = excluido
        };
    }
}
