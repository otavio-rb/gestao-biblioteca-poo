using BibliotecaDigital.Web.Models;

namespace BibliotecaDigital.Web.Repositories.Interfaces;

public interface IEditoraRepository : IRepository<Editora>
{
    bool PossuiLivrosVinculados(int editoraId);
}
