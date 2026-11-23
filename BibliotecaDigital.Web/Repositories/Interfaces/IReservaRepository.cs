using BibliotecaDigital.Web.Models;

namespace BibliotecaDigital.Web.Repositories.Interfaces;

public interface IReservaRepository : IRepository<Reserva>
{
    IEnumerable<Reserva> ObterPendentesPorExemplar(int exemplarId);

    bool ExisteReservaPendenteParaOutroLeitor(int exemplarId, int leitorId);

    int ProximaPosicaoNaFila(int exemplarId);
}
