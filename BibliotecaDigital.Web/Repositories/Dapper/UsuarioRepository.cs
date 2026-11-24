using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using BibliotecaDigital.Web.Exceptions;
using BibliotecaDigital.Web.Infrastructure;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Repositories.Interfaces;

namespace BibliotecaDigital.Web.Repositories.Dapper
{
    public class UsuarioRepository : RepositorioDapperBase, IUsuarioRepository
    {
        public UsuarioRepository(IFabricaConexao fabricaConexao) : base(fabricaConexao)
        {
        }

        public async Task<int> CadastrarAsync(Usuario usuario)
        {
            var existente = await BuscarPorEmailAsync(usuario.Email);
            if (existente != null)
                throw new EmailDuplicadoException(usuario.Email);

            using var conexao = AbrirConexao();

            const string sqlEndereco = @"
                INSERT INTO Endereco (Rua, Numero, Cidade, Estado, CEP)
                VALUES (@Rua, @Numero, @Cidade, @Estado, @CEP);
                SELECT last_insert_rowid();";

            int enderecoId = await conexao.ExecuteScalarAsync<int>(sqlEndereco, usuario.EnderecoResidencial);

            const string sqlUsuario = @"
                INSERT INTO Usuario (Tipo, Nome, Email, EstaBloqueado, LimiteEmprestimos, Senha, Departamento, EnderecoId)
                VALUES (@Tipo, @Nome, @Email, @EstaBloqueado, @LimiteEmprestimos, @Senha, @Departamento, @EnderecoId);
                SELECT last_insert_rowid();";

            var parametros = new
            {
                Tipo = usuario is Administrador ? "ADMINISTRADOR" : "LEITOR",
                usuario.Nome,
                usuario.Email,
                EstaBloqueado = usuario.EstaBloqueado ? 1 : 0,
                LimiteEmprestimos = usuario is Leitor l ? (int?)l.LimiteEmprestimos : null,
                usuario.Senha,
                Departamento = usuario is Administrador a ? a.Departamento : null,
                EnderecoId = enderecoId
            };

            return await conexao.ExecuteScalarAsync<int>(sqlUsuario, parametros);
        }

        public async Task<Usuario?> BuscarPorIdAsync(int id)
        {
            using var conexao = AbrirConexao();

            const string sql = @"
                SELECT u.*, e.Id AS EndId, e.Rua, e.Numero, e.Cidade, e.Estado, e.CEP
                FROM Usuario u
                LEFT JOIN Endereco e ON u.EnderecoId = e.Id
                WHERE u.Id = @Id;";

            var row = await conexao.QueryFirstOrDefaultAsync(sql, new { Id = id });
            return row == null ? null : MapearUsuario(row);
        }

        public async Task<Usuario?> BuscarPorEmailAsync(string email)
        {
            using var conexao = AbrirConexao();

            const string sql = @"
                SELECT u.*, e.Id AS EndId, e.Rua, e.Numero, e.Cidade, e.Estado, e.CEP
                FROM Usuario u
                LEFT JOIN Endereco e ON u.EnderecoId = e.Id
                WHERE LOWER(u.Email) = LOWER(@Email);";

            var row = await conexao.QueryFirstOrDefaultAsync(sql, new { Email = email });
            return row == null ? null : MapearUsuario(row);
        }

        public async Task<IEnumerable<Usuario>> ListarTodosAsync()
        {
            using var conexao = AbrirConexao();

            const string sql = @"
                SELECT u.*, e.Id AS EndId, e.Rua, e.Numero, e.Cidade, e.Estado, e.CEP
                FROM Usuario u
                LEFT JOIN Endereco e ON u.EnderecoId = e.Id;";

            var rows = await conexao.QueryAsync(sql);
            var lista = new List<Usuario>();
            foreach (var row in rows)
                lista.Add(MapearUsuario(row));

            return lista;
        }

        public async Task AtualizarStatusBloqueioAsync(Usuario usuario)
        {
            using var conexao = AbrirConexao();

            const string sql = "UPDATE Usuario SET EstaBloqueado = @EstaBloqueado WHERE Id = @Id;";
            await conexao.ExecuteAsync(sql, new
            {
                EstaBloqueado = usuario.EstaBloqueado ? 1 : 0,
                usuario.Id
            });
        }

        private Usuario MapearUsuario(dynamic row)
        {
            var endereco = row.EndId != null
                ? new Endereco((int)row.EndId, (string)row.Rua, (string)row.Numero, (string)row.Cidade, (string)row.Estado, (string)row.CEP)
                : new Endereco(0, "Nao informado", "-", "Nao informada", "-", "-");

            bool bloqueado = (long)row.EstaBloqueado == 1;
            string senha = (string?)row.Senha ?? "123456";

            if ((string)row.Tipo == "ADMINISTRADOR")
            {
                return new Administrador(
                    (string)row.Nome, (string)row.Email, senha,
                    endereco, (string?)row.Departamento ?? "Geral", (int)row.Id, bloqueado);
            }

            return new Leitor(
                (string)row.Nome, (string)row.Email, senha,
                endereco, (int)row.Id, bloqueado,
                (int)(row.LimiteEmprestimos ?? 3), 0);
        }
    }
}