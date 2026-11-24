using BibliotecaDigital.Web.Models;

namespace BibliotecaDigital.Web.Repositories.Interfaces;

public interface IExemplarRepository
{
    ExemplarDigital? ObterPorId(int id);

    IEnumerable<ExemplarDigital> ObterPorLivro(int livroId);

    int ObterLivroId(int exemplarId);

    int ObterLicencasTotais(int exemplarId);

    int Inserir(int livroId, ExemplarDigital exemplar, int licencasTotais);

    void Atualizar(ExemplarDigital exemplar, int licencasTotais);

    void Remover(int id);

    bool PossuiEmprestimoAtivo(int exemplarId);
}
