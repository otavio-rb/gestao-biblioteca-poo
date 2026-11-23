namespace BibliotecaDigital.Web.Exceptions;

public class DominioException : Exception
{
    public DominioException(string mensagem) : base(mensagem)
    {
    }

    public DominioException(string mensagem, Exception interna) : base(mensagem, interna)
    {
    }
}
