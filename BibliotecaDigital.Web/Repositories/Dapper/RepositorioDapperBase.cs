using System.Data;
using BibliotecaDigital.Web.Exceptions;
using BibliotecaDigital.Web.Infrastructure;
using Microsoft.Data.Sqlite;

namespace BibliotecaDigital.Web.Repositories.Dapper;

public abstract class RepositorioDapperBase
{
    private readonly IFabricaConexao _fabricaConexao;

    protected RepositorioDapperBase(IFabricaConexao fabricaConexao)
    {
        _fabricaConexao = fabricaConexao;
    }

    protected IDbConnection AbrirConexao() => _fabricaConexao.Criar();

    protected TResultado Executar<TResultado>(Func<IDbConnection, TResultado> operacao, string descricao)
    {
        try
        {
            using var conexao = AbrirConexao();
            return operacao(conexao);
        }
        catch (SqliteException excecao) when (excecao.SqliteErrorCode == 19)
        {
            throw new DominioException($"Nao foi possivel {descricao}: o registro viola uma restricao do banco de dados.", excecao);
        }
        catch (DominioException)
        {
            throw;
        }
        catch (Exception excecao)
        {
            throw new DominioException($"Falha inesperada ao {descricao}.", excecao);
        }
    }

    protected void Executar(Action<IDbConnection> operacao, string descricao)
    {
        Executar<object?>(conexao =>
        {
            operacao(conexao);
            return null;
        }, descricao);
    }
}
