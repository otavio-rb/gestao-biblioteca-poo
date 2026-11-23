using BibliotecaDigital.Web.Exceptions;

namespace BibliotecaDigital.Web.Models;

public sealed class LeitorEmCirculacao
{
    public LeitorEmCirculacao(
        int id,
        string nome,
        bool estaBloqueado,
        int limiteEmprestimos,
        int emprestimosAtivos,
        decimal multasEmAberto)
    {
        if (id <= 0)
        {
            throw new DominioException("O leitor informado e invalido.");
        }

        Id = id;
        Nome = string.IsNullOrWhiteSpace(nome) ? throw new DominioException("O leitor precisa ter nome.") : nome.Trim();
        EstaBloqueado = estaBloqueado;
        LimiteEmprestimos = limiteEmprestimos < 1 ? 1 : limiteEmprestimos;
        EmprestimosAtivos = emprestimosAtivos < 0 ? 0 : emprestimosAtivos;
        MultasEmAberto = multasEmAberto < 0 ? 0 : multasEmAberto;
    }

    public int Id { get; }

    public string Nome { get; }

    public bool EstaBloqueado { get; }

    public int LimiteEmprestimos { get; }

    public int EmprestimosAtivos { get; }

    public decimal MultasEmAberto { get; }

    public int VagasDisponiveis => Math.Max(0, LimiteEmprestimos - EmprestimosAtivos);

    public bool PodeEmprestar()
        => !EstaBloqueado && MultasEmAberto == 0 && EmprestimosAtivos < LimiteEmprestimos;

    public void GarantirQuePodeEmprestar()
    {
        if (EstaBloqueado)
        {
            throw new LeitorBloqueadoException(Nome);
        }

        if (MultasEmAberto > 0)
        {
            throw new MultaEmAbertoException(MultasEmAberto);
        }

        if (EmprestimosAtivos >= LimiteEmprestimos)
        {
            throw new LimiteEmprestimosException(Nome, LimiteEmprestimos);
        }
    }

    public void GarantirQuePodeReservar()
    {
        if (EstaBloqueado)
        {
            throw new LeitorBloqueadoException(Nome);
        }
    }

    public string Resumo() => $"{Nome} ({EmprestimosAtivos}/{LimiteEmprestimos} emprestimos)";
}
