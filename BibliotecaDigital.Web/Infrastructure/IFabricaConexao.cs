using System.Data;

namespace BibliotecaDigital.Web.Infrastructure;

public interface IFabricaConexao
{
    IDbConnection Criar();
}
