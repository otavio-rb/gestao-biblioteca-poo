namespace BibliotecaDigital.Web.Exceptions;

public class MultaEmAbertoException : DominioException
{
    public MultaEmAbertoException(decimal valorEmAberto)
        : base($"Existe multa em aberto no valor de {valorEmAberto:C}. Quite a pendencia antes de um novo emprestimo.")
    {
    }
}
