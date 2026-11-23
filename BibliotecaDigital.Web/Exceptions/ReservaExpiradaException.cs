namespace BibliotecaDigital.Web.Exceptions;

public class ReservaExpiradaException : DominioException
{
    public ReservaExpiradaException(DateTime dataLimite)
        : base($"A reserva expirou em {dataLimite:dd/MM/yyyy} e nao pode mais ser efetivada.")
    {
    }
}
