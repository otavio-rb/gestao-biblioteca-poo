using BibliotecaDigital.Web.Exceptions;

namespace BibliotecaDigital.Web.Models;

public sealed class Favorito
{
    public Favorito(int usuarioId, int livroId)
    {
        if (usuarioId <= 0 || livroId <= 0)
        {
            throw new DominioException("O favorito exige um usuario e um livro validos.");
        }

        UsuarioId = usuarioId;
        LivroId = livroId;
        DataAdicao = DateTime.Now;
    }

    public int UsuarioId { get; private set; }

    public int LivroId { get; private set; }

    public DateTime DataAdicao { get; private set; }

    // Dado so para exibicao nas telas (preenchido pelo repositorio).
    public string TituloLivro { get; private set; } = string.Empty;

    public void DescreverContexto(string tituloLivro) => TituloLivro = tituloLivro;

    public bool PertenceA(int usuarioId)
    {
        return UsuarioId == usuarioId;
    }

    // Regra: o favorito e unico pelo par usuario + livro.
    // Dois objetos com o mesmo par sao considerados iguais.
    public override bool Equals(object? obj)
    {
        if (obj is not Favorito outro)
        {
            return false;
        }

        return UsuarioId == outro.UsuarioId && LivroId == outro.LivroId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UsuarioId, LivroId);
    }

    public static Favorito Reconstruir(int usuarioId, int livroId, DateTime dataAdicao)
    {
        return new Favorito(usuarioId, livroId)
        {
            DataAdicao = dataAdicao
        };
    }
}
