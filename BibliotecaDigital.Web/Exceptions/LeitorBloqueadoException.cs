namespace BibliotecaDigital.Web.Exceptions;

public class LeitorBloqueadoException : DominioException
{
    public LeitorBloqueadoException(string nome)
        : base($"O leitor {nome} esta bloqueado e nao pode realizar novos emprestimos.")
    {
    }
}
