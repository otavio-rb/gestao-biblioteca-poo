using Dapper;

namespace BibliotecaDigital.Web.Infrastructure;

public sealed class InicializadorBanco
{
    private readonly IFabricaConexao _fabricaConexao;
    private readonly IConfiguration _configuracao;
    private readonly IWebHostEnvironment _ambiente;
    private readonly ILogger<InicializadorBanco> _logger;

    public InicializadorBanco(
        IFabricaConexao fabricaConexao,
        IConfiguration configuracao,
        IWebHostEnvironment ambiente,
        ILogger<InicializadorBanco> logger)
    {
        _fabricaConexao = fabricaConexao;
        _configuracao = configuracao;
        _ambiente = ambiente;
        _logger = logger;
    }

    public void GarantirBancoCriado()
    {
        if (!_configuracao.GetValue("Banco:ExecutarScriptNaInicializacao", true))
        {
            return;
        }

        var caminhoRelativo = _configuracao.GetValue<string>("Banco:CaminhoScript") ?? "Database/script.sql";
        var caminho = Path.Combine(_ambiente.ContentRootPath, caminhoRelativo);

        if (!File.Exists(caminho))
        {
            _logger.LogWarning("Script de banco nao encontrado em {Caminho}.", caminho);
            return;
        }

        using var conexao = _fabricaConexao.Criar();

        var tabelasExistentes = conexao.ExecuteScalar<long>(
            "SELECT COUNT(1) FROM sqlite_master WHERE type = @tipo AND name = @nome;",
            new { tipo = "table", nome = "Usuario" });

        if (tabelasExistentes > 0)
        {
            return;
        }

        var script = File.ReadAllText(caminho);
        conexao.Execute(script);
        _logger.LogInformation("Banco de dados criado e populado a partir de {Caminho}.", caminho);
    }
}
