using BibliotecaDigital.Web.Exceptions;

namespace BibliotecaDigital.Web.Models;

public sealed class Multa
{
    public const decimal ValorDiarioPadrao = 1.50m;

    public Multa(int emprestimoId, decimal valorDiario, int diasAtraso, decimal valorReferenciaObra)
    {
        if (diasAtraso < 1)
        {
            throw new DominioException("Uma multa so pode ser gerada quando houver ao menos um dia de atraso.");
        }

        if (valorDiario <= 0)
        {
            throw new DominioException("O valor diario da multa deve ser maior que zero.");
        }

        EmprestimoId = emprestimoId;
        ValorDiario = valorDiario;
        DiasAtraso = diasAtraso;
        ValorReferenciaObra = valorReferenciaObra;
        ValorTotal = CalcularValorTotal();
        Paga = false;
    }

    public int Id { get; private set; }

    public int EmprestimoId { get; private set; }

    public decimal ValorDiario { get; private set; }

    public int DiasAtraso { get; private set; }

    public decimal ValorReferenciaObra { get; private set; }

    public decimal ValorTotal { get; private set; }

    public bool Paga { get; private set; }

    public decimal CalcularValorTotal()
    {
        var bruto = ValorDiario * DiasAtraso;
        return bruto > ValorReferenciaObra ? ValorReferenciaObra : bruto;
    }

    public void Quitar()
    {
        if (Paga)
        {
            throw new DominioException("Esta multa ja foi quitada.");
        }

        Paga = true;
    }

    public void DefinirId(int id) => Id = id;

    public static Multa Reconstruir(
        int id,
        int emprestimoId,
        decimal valorDiario,
        int diasAtraso,
        decimal valorReferenciaObra,
        decimal valorTotal,
        bool paga)
    {
        return new Multa(emprestimoId, valorDiario, diasAtraso, valorReferenciaObra)
        {
            Id = id,
            ValorTotal = valorTotal,
            Paga = paga
        };
    }
}
