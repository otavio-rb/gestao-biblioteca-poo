using BibliotecaDigital.Web.Exceptions;

namespace BibliotecaDigital.Web.Models;

public sealed class Autor
{
    // O construtor faz o papel do "cadastrar": o autor ja nasce valido.
    public Autor(string nome, string nacionalidade)
    {
        Editar(nome, nacionalidade);
    }

    public int Id { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public string Nacionalidade { get; private set; } = string.Empty;

    public void Editar(string nome, string nacionalidade)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length < 3)
        {
            throw new DominioException("O nome do autor deve ter ao menos tres caracteres.");
        }

        if (string.IsNullOrWhiteSpace(nacionalidade))
        {
            throw new DominioException("Informe a nacionalidade do autor.");
        }

        Nome = nome.Trim();
        Nacionalidade = nacionalidade.Trim();
    }

    public void DefinirId(int id) => Id = id;

    public static Autor Reconstruir(int id, string nome, string nacionalidade)
    {
        return new Autor(nome, nacionalidade) { Id = id };
    }
}
