using BibliotecaDigital.Web.Infrastructure;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Dapper;

namespace BibliotecaDigital.Web.Repositories.Dapper;

public sealed class FavoritoRepository : RepositorioDapperBase, IFavoritoRepository
{
    private sealed class LinhaFavorito
    {
        public int UsuarioId { get; set; }

        public int LivroId { get; set; }

        public DateTime DataAdicao { get; set; }

        public string TituloLivro { get; set; } = string.Empty;
    }

    public FavoritoRepository(IFabricaConexao fabricaConexao) : base(fabricaConexao)
    {
    }

    public IEnumerable<Favorito> ObterPorUsuario(int usuarioId) => Executar(conexao =>
    {
        const string sql = """
            SELECT f.UsuarioId, f.LivroId, f.DataAdicao, l.Titulo AS TituloLivro
            FROM Favorito f
            INNER JOIN Livro l ON l.Id = f.LivroId
            WHERE f.UsuarioId = @UsuarioId
            ORDER BY l.Titulo;
            """;

        return conexao.Query<LinhaFavorito>(sql, new { UsuarioId = usuarioId }).Select(Mapear).ToList();
    }, "listar os favoritos");

    public bool Existe(int usuarioId, int livroId) => Executar(conexao =>
    {
        const string sql = "SELECT COUNT(1) FROM Favorito WHERE UsuarioId = @UsuarioId AND LivroId = @LivroId;";
        return conexao.ExecuteScalar<long>(sql, new { UsuarioId = usuarioId, LivroId = livroId }) > 0;
    }, "verificar o favorito");

    public void Adicionar(Favorito favorito) => Executar(conexao =>
    {
        const string sql = "INSERT INTO Favorito (UsuarioId, LivroId, DataAdicao) VALUES (@UsuarioId, @LivroId, @DataAdicao);";
        conexao.Execute(sql, new { favorito.UsuarioId, favorito.LivroId, favorito.DataAdicao });
    }, "adicionar o favorito");

    public void Remover(int usuarioId, int livroId) => Executar(conexao =>
    {
        const string sql = "DELETE FROM Favorito WHERE UsuarioId = @UsuarioId AND LivroId = @LivroId;";
        conexao.Execute(sql, new { UsuarioId = usuarioId, LivroId = livroId });
    }, "remover o favorito");

    private static Favorito Mapear(LinhaFavorito linha)
    {
        var favorito = Favorito.Reconstruir(linha.UsuarioId, linha.LivroId, linha.DataAdicao);
        favorito.DescreverContexto(linha.TituloLivro);
        return favorito;
    }
}
