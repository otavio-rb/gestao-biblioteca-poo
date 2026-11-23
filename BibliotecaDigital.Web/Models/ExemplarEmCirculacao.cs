using BibliotecaDigital.Web.Exceptions;

namespace BibliotecaDigital.Web.Models;

public sealed class ExemplarEmCirculacao
{
    public ExemplarEmCirculacao(
        int id,
        string tituloLivro,
        decimal valorReferenciaObra,
        int licencasTotais,
        int licencasDisponiveis)
    {
        if (id <= 0)
        {
            throw new DominioException("O exemplar digital informado e invalido.");
        }

        if (valorReferenciaObra <= 0)
        {
            throw new DominioException("A obra precisa ter valor de referencia maior que zero.");
        }

        if (licencasTotais < 1)
        {
            throw new DominioException("O exemplar precisa de ao menos uma licenca.");
        }

        if (licencasDisponiveis < 0 || licencasDisponiveis > licencasTotais)
        {
            throw new DominioException("A quantidade de licencas disponiveis e inconsistente.");
        }

        Id = id;
        TituloLivro = tituloLivro;
        ValorReferenciaObra = valorReferenciaObra;
        LicencasTotais = licencasTotais;
        LicencasDisponiveis = licencasDisponiveis;
    }

    public int Id { get; }

    public string TituloLivro { get; }

    public decimal ValorReferenciaObra { get; }

    public int LicencasTotais { get; }

    public int LicencasDisponiveis { get; private set; }

    public int LicencasEmUso => LicencasTotais - LicencasDisponiveis;

    public bool VerificarDisponibilidade() => LicencasDisponiveis > 0;

    public void ConsumirLicenca()
    {
        if (!VerificarDisponibilidade())
        {
            throw new ExemplarIndisponivelException(TituloLivro);
        }

        LicencasDisponiveis--;
    }

    public void LiberarLicenca()
    {
        if (LicencasDisponiveis >= LicencasTotais)
        {
            throw new DominioException("Nao existe licenca emprestada para liberar neste exemplar.");
        }

        LicencasDisponiveis++;
    }

    public string Resumo() => $"{TituloLivro} ({LicencasDisponiveis} de {LicencasTotais} licenca(s) livre(s))";
}
