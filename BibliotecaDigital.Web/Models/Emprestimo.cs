using BibliotecaDigital.Web.Exceptions;
using BibliotecaDigital.Web.Models.Enums;

namespace BibliotecaDigital.Web.Models;

public sealed class Emprestimo
{
    public const int PrazoPadraoDias = 14;
    public const int MaximoRenovacoes = 2;

    private readonly List<Renovacao> _renovacoes = new();

    public Emprestimo(int leitorId, int exemplarId, decimal valorReferenciaObra, DateTime? dataRetirada = null, int? reservaId = null)
    {
        if (leitorId <= 0 || exemplarId <= 0)
        {
            throw new DominioException("O emprestimo exige um leitor e um exemplar validos.");
        }

        if (valorReferenciaObra <= 0)
        {
            throw new DominioException("O valor de referencia da obra deve ser maior que zero.");
        }

        LeitorId = leitorId;
        ExemplarId = exemplarId;
        ValorReferenciaObra = valorReferenciaObra;
        DataRetirada = dataRetirada ?? DateTime.Now;
        DataPrevista = DataRetirada.Date.AddDays(PrazoPadraoDias);
        ReservaId = reservaId;
        Status = StatusEmprestimo.Ativo;
    }

    public int Id { get; private set; }

    public int LeitorId { get; private set; }

    public int ExemplarId { get; private set; }

    public int? ReservaId { get; private set; }

    public decimal ValorReferenciaObra { get; private set; }

    public DateTime DataRetirada { get; private set; }

    public DateTime DataPrevista { get; private set; }

    public DateTime? DataDevolucao { get; private set; }

    public StatusEmprestimo Status { get; private set; }

    public IReadOnlyList<Renovacao> Renovacoes => _renovacoes;

    public Devolucao? Devolucao { get; private set; }

    public Multa? Multa { get; private set; }

    public bool EstaEncerrado => Status is StatusEmprestimo.Devolvido or StatusEmprestimo.DevolvidoComAtraso;

    public string TituloLivro { get; private set; } = string.Empty;

    public string NomeLeitor { get; private set; } = string.Empty;

    public bool EstaEmAtraso(DateTime? referencia = null)
    {
        var data = (referencia ?? DateTime.Now).Date;
        var limite = EstaEncerrado ? DataDevolucao!.Value.Date : data;
        return limite > DataPrevista.Date;
    }

    public int CalcularDiasAtraso(DateTime? referencia = null)
    {
        var data = EstaEncerrado ? DataDevolucao!.Value.Date : (referencia ?? DateTime.Now).Date;
        var dias = (data - DataPrevista.Date).Days;
        return dias > 0 ? dias : 0;
    }

    public bool PodeRenovar(bool existeReservaPendente)
        => !EstaEncerrado
           && _renovacoes.Count < MaximoRenovacoes
           && !EstaEmAtraso()
           && !existeReservaPendente;

    public Renovacao Renovar(bool existeReservaPendente)
    {
        if (EstaEncerrado)
        {
            throw new DominioException("Um emprestimo ja devolvido nao pode ser renovado.");
        }

        if (_renovacoes.Count >= MaximoRenovacoes)
        {
            throw new LimiteRenovacoesException(MaximoRenovacoes);
        }

        if (EstaEmAtraso())
        {
            throw new DominioException("Emprestimos em atraso nao podem ser renovados.");
        }

        if (existeReservaPendente)
        {
            throw new DominioException("Existe reserva pendente para este exemplar; a renovacao nao e permitida.");
        }

        var novaDataPrevista = DataPrevista.AddDays(PrazoPadraoDias);
        var renovacao = new Renovacao(Id, DateTime.Now, novaDataPrevista, _renovacoes.Count + 1);
        _renovacoes.Add(renovacao);
        DataPrevista = novaDataPrevista;
        Status = StatusEmprestimo.Renovado;
        return renovacao;
    }

    public Devolucao Devolver(decimal valorDiarioMulta, DateTime? dataDevolucao = null)
    {
        if (EstaEncerrado)
        {
            throw new DominioException("Este emprestimo ja foi devolvido.");
        }

        DataDevolucao = dataDevolucao ?? DateTime.Now;
        var diasAtraso = CalcularDiasAtraso();
        var houveAtraso = diasAtraso > 0;

        Status = houveAtraso ? StatusEmprestimo.DevolvidoComAtraso : StatusEmprestimo.Devolvido;
        Devolucao = new Devolucao(Id, DataDevolucao.Value, houveAtraso);

        if (houveAtraso)
        {
            Multa = new Multa(Id, valorDiarioMulta, diasAtraso, ValorReferenciaObra);
        }

        return Devolucao;
    }

    public void AnexarRenovacoes(IEnumerable<Renovacao> renovacoes)
    {
        _renovacoes.Clear();
        _renovacoes.AddRange(renovacoes.OrderBy(renovacao => renovacao.Sequencia));
    }

    public void AnexarDevolucao(Devolucao? devolucao) => Devolucao = devolucao;

    public void AnexarMulta(Multa? multa) => Multa = multa;

    public void DescreverContexto(string tituloLivro, string nomeLeitor)
    {
        TituloLivro = tituloLivro;
        NomeLeitor = nomeLeitor;
    }

    public void DefinirId(int id) => Id = id;

    public static Emprestimo Reconstruir(
        int id,
        int leitorId,
        int exemplarId,
        int? reservaId,
        decimal valorReferenciaObra,
        DateTime dataRetirada,
        DateTime dataPrevista,
        DateTime? dataDevolucao,
        StatusEmprestimo status)
    {
        var emprestimo = new Emprestimo(leitorId, exemplarId, valorReferenciaObra, dataRetirada, reservaId)
        {
            Id = id,
            DataPrevista = dataPrevista,
            DataDevolucao = dataDevolucao,
            Status = status
        };

        return emprestimo;
    }
}
