using BibliotecaDigital.Web.Exceptions;

namespace BibliotecaDigital.Web.Models;

public sealed class Categoria
{
    // O construtor faz o papel do "cadastrar": a categoria ja nasce valida.
    public Categoria(string nome, string descricao)
    {
        Editar(nome, descricao);
    }

    public int Id { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public string Descricao { get; private set; } = string.Empty;

    public void Editar(string nome, string descricao)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DominioException("O nome da categoria e obrigatorio.");
        }

        Nome = nome.Trim();

        // A descricao e opcional.
        Descricao = descricao == null ? string.Empty : descricao.Trim();
    }

    public void DefinirId(int id) => Id = id;

    public static Categoria Reconstruir(int id, string nome, string descricao)
    {
        return new Categoria(nome, descricao) { Id = id };
    }
}
