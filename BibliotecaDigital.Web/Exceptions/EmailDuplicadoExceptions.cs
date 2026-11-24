namespace BibliotecaDigital.Web.Exceptions
{
    public class EmailDuplicadoException : DominioException
    {
        public EmailDuplicadoException(string email)
            : base($"Já existe um usuário cadastrado com o e-mail '{email}'.") { }
    }
}