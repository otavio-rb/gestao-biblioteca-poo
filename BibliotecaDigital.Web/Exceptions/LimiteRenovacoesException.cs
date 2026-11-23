namespace BibliotecaDigital.Web.Exceptions;

public class LimiteRenovacoesException : DominioException
{
    public LimiteRenovacoesException(int limite)
        : base($"O emprestimo ja atingiu o limite de {limite} renovacoes.")
    {
    }
}
