using BibliotecaDigital.Web.Exceptions;

namespace BibliotecaDigital.Web.Models;

public sealed class Editora
{
    // O construtor faz o papel do "cadastrar": a editora ja nasce valida.
    public Editora(string nome, string endereco)
    {
        Editar(nome, endereco);
    }

    public int Id { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public string Endereco { get; private set; } = string.Empty;

    public void Editar(string nome, string endereco)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DominioException("O nome da editora e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(endereco))
        {
            throw new DominioException("O endereco da editora e obrigatorio.");
        }

        Nome = nome.Trim();
        Endereco = endereco.Trim();
    }

    public void DefinirId(int id) => Id = id;

    public static Editora Reconstruir(int id, string nome, string endereco)
    {
        return new Editora(nome, endereco) { Id = id };
    }
}
