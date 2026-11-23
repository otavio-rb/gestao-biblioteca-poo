using BibliotecaDigital.Web.Exceptions;
using BibliotecaDigital.Web.Models.Enums;

namespace BibliotecaDigital.Web.Models;

public sealed class Reserva
{
    public const int PrazoRetiradaDias = 3;

    public Reserva(int leitorId, int exemplarId, int posicaoFila, DateTime? dataReserva = null)
    {
        if (leitorId <= 0 || exemplarId <= 0)
        {
            throw new DominioException("A reserva exige um leitor e um exemplar validos.");
        }

        if (posicaoFila < 1)
        {
            throw new DominioException("A posicao na fila deve iniciar em 1.");
        }

        LeitorId = leitorId;
        ExemplarId = exemplarId;
        PosicaoFila = posicaoFila;
        DataReserva = dataReserva ?? DateTime.Now;
        DataLimiteRetirada = DataReserva.Date.AddDays(PrazoRetiradaDias);
        Status = StatusReserva.Aguardando;
    }

    public int Id { get; private set; }

    public int LeitorId { get; private set; }

    public int ExemplarId { get; private set; }

    public DateTime DataReserva { get; private set; }

    public DateTime DataLimiteRetirada { get; private set; }

    public int PosicaoFila { get; private set; }

    public StatusReserva Status { get; private set; }

    public string TituloLivro { get; private set; } = string.Empty;

    public string NomeLeitor { get; private set; } = string.Empty;

    public bool EstaPendente => Status is StatusReserva.Aguardando or StatusReserva.Disponivel;

    public void Disponibilizar()
    {
        if (Status != StatusReserva.Aguardando)
        {
            throw new DominioException("Apenas reservas aguardando na fila podem ser disponibilizadas.");
        }

        Status = StatusReserva.Disponivel;
        DataLimiteRetirada = DateTime.Now.Date.AddDays(PrazoRetiradaDias);
    }

    public void Cancelar()
    {
        if (!EstaPendente)
        {
            throw new DominioException("Somente reservas pendentes podem ser canceladas.");
        }

        Status = StatusReserva.Cancelada;
    }

    public void Expirar()
    {
        if (!EstaPendente)
        {
            throw new DominioException("Somente reservas pendentes podem expirar.");
        }

        Status = StatusReserva.Expirada;
    }

    public Emprestimo Efetivar(decimal valorReferenciaObra)
    {
        if (!EstaPendente)
        {
            throw new DominioException("Esta reserva nao esta mais pendente.");
        }

        if (DateTime.Now.Date > DataLimiteRetirada.Date)
        {
            Status = StatusReserva.Expirada;
            throw new ReservaExpiradaException(DataLimiteRetirada);
        }

        Status = StatusReserva.Efetivada;
        return new Emprestimo(LeitorId, ExemplarId, valorReferenciaObra, DateTime.Now, Id);
    }

    public void DescreverContexto(string tituloLivro, string nomeLeitor)
    {
        TituloLivro = tituloLivro;
        NomeLeitor = nomeLeitor;
    }

    public void DefinirId(int id) => Id = id;

    public static Reserva Reconstruir(
        int id,
        int leitorId,
        int exemplarId,
        DateTime dataReserva,
        DateTime dataLimiteRetirada,
        int posicaoFila,
        StatusReserva status)
    {
        return new Reserva(leitorId, exemplarId, posicaoFila, dataReserva)
        {
            Id = id,
            DataLimiteRetirada = dataLimiteRetirada,
            Status = status
        };
    }
}
