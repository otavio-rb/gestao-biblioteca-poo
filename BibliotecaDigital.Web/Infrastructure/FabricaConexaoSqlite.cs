using System.Data;
using Microsoft.Data.Sqlite;

namespace BibliotecaDigital.Web.Infrastructure;

public sealed class FabricaConexaoSqlite : IFabricaConexao
{
    private readonly string _stringConexao;

    public FabricaConexaoSqlite(IConfiguration configuracao)
    {
        _stringConexao = configuracao.GetConnectionString("BibliotecaDigital")
            ?? throw new InvalidOperationException("A connection string 'BibliotecaDigital' nao foi configurada.");
    }

    public IDbConnection Criar()
    {
        var conexao = new SqliteConnection(_stringConexao);
        conexao.Open();

        using var comando = conexao.CreateCommand();
        comando.CommandText = "PRAGMA foreign_keys = ON;";
        comando.ExecuteNonQuery();

        return conexao;
    }
}
