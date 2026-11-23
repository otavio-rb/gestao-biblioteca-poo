using BibliotecaDigital.Web.Models;

namespace BibliotecaDigital.Web.Repositories.Interfaces;

public interface ICirculacaoRepository
{
    IEnumerable<LeitorEmCirculacao> ObterLeitores();

    LeitorEmCirculacao? ObterLeitor(int leitorId);

    IEnumerable<ExemplarEmCirculacao> ObterExemplares();

    ExemplarEmCirculacao? ObterExemplar(int exemplarId);

    void AtualizarLicencas(ExemplarEmCirculacao exemplar);
}
