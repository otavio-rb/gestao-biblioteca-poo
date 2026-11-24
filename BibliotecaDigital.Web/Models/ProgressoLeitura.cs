using BibliotecaDigital.Web.Exceptions;

namespace BibliotecaDigital.Web.Models;

// Faz parte da Leitura (composicao): nao existe progresso sem leitura.
public sealed class ProgressoLeitura
{
    public ProgressoLeitura(int totalPaginas)
    {
        if (totalPaginas <= 0)
        {
            throw new DominioException("O livro precisa ter pelo menos uma pagina.");
        }

        TotalPaginas = totalPaginas;
        PaginaAtual = 0;
        UltimaAtualizacao = DateTime.Now;
    }

    public int PaginaAtual { get; private set; }

    public int TotalPaginas { get; private set; }

    public DateTime UltimaAtualizacao { get; private set; }

    // Regra: 0 <= paginaAtual <= totalPaginas
    public void AvancarPara(int pagina)
    {
        if (pagina < 0 || pagina > TotalPaginas)
        {
            throw new DominioException($"A pagina deve estar entre 0 e {TotalPaginas}.");
        }

        PaginaAtual = pagina;
        UltimaAtualizacao = DateTime.Now;
    }

    // percentual = paginaAtual / totalPaginas x 100
    public double GetPercentual()
    {
        return (double)PaginaAtual / TotalPaginas * 100;
    }

    public bool EstaCompleto()
    {
        return PaginaAtual == TotalPaginas;
    }

    public static ProgressoLeitura Reconstruir(int paginaAtual, int totalPaginas, DateTime ultimaAtualizacao)
    {
        return new ProgressoLeitura(totalPaginas)
        {
            PaginaAtual = paginaAtual,
            UltimaAtualizacao = ultimaAtualizacao
        };
    }
}
