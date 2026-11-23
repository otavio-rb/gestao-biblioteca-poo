namespace BibliotecaDigital.Web.Exceptions;

public class RegistroNaoEncontradoException : DominioException
{
    public RegistroNaoEncontradoException(string entidade, int id)
        : base($"{entidade} de identificador {id} nao foi encontrado.")
    {
        Entidade = entidade;
        Id = id;
    }

    public string Entidade { get; }

    public int Id { get; }
}
