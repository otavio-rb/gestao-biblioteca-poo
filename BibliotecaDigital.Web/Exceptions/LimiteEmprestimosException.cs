namespace BibliotecaDigital.Web.Exceptions;

public class LimiteEmprestimosException : DominioException
{
    public LimiteEmprestimosException(string nome, int limite)
        : base($"{nome} ja possui {limite} emprestimo(s) ativo(s), que e o limite do seu perfil.")
    {
    }
}
