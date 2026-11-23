using BibliotecaDigital.Web.Models;

namespace BibliotecaDigital.Web.Repositories.Interfaces;

public interface IEmprestimoRepository : IRepository<Emprestimo>
{
    Emprestimo? ObterComDetalhes(int id);

    IEnumerable<Emprestimo> ObterPorLeitor(int leitorId);

    int ContarAtivosDoLeitor(int leitorId);

    decimal TotalMultasEmAbertoDoLeitor(int leitorId);

    void RegistrarRenovacao(Renovacao renovacao);

    void RegistrarDevolucao(Devolucao devolucao);

    void RegistrarMulta(Multa multa);

    Multa? ObterMultaPorEmprestimo(int emprestimoId);

    void QuitarMulta(Multa multa);
}
