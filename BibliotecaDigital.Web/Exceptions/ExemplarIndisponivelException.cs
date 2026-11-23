namespace BibliotecaDigital.Web.Exceptions;

public class ExemplarIndisponivelException : DominioException
{
    public ExemplarIndisponivelException(string titulo)
        : base($"Nao ha licenca digital livre para \"{titulo}\" no momento.")
    {
    }
}
