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

        public async Task AtualizarAsync(Usuario usuario)
        {
            var existente = await BuscarPorEmailAsync(usuario.Email);
            if (existente != null && existente.Id != usuario.Id)
                throw new EmailDuplicadoException(usuario.Email);

            using var conexao = AbrirConexao();

            const string sqlUsuario = @"
                UPDATE Usuario SET Nome = @Nome, Email = @Email WHERE Id = @Id;";

            await conexao.ExecuteAsync(sqlUsuario, new { usuario.Nome, usuario.Email, usuario.Id });

            const string sqlEnderecoId = "SELECT EnderecoId FROM Usuario WHERE Id = @Id;";
            var enderecoId = await conexao.ExecuteScalarAsync<int?>(sqlEnderecoId, new { usuario.Id });

            var endereco = usuario.EnderecoResidencial;

            if (enderecoId == null)
            {
                const string sqlInserir = @"
                    INSERT INTO Endereco (Rua, Numero, Cidade, Estado, CEP)
                    VALUES (@Rua, @Numero, @Cidade, @Estado, @CEP);
                    SELECT last_insert_rowid();";

                var novoId = await conexao.ExecuteScalarAsync<int>(sqlInserir, endereco);
                await conexao.ExecuteAsync(
                    "UPDATE Usuario SET EnderecoId = @EnderecoId WHERE Id = @Id;",
                    new { EnderecoId = novoId, usuario.Id });
                return;
            }

            const string sqlEndereco = @"
                UPDATE Endereco SET Rua = @Rua, Numero = @Numero, Cidade = @Cidade,
                                    Estado = @Estado, CEP = @CEP
                WHERE Id = @Id;";

            await conexao.ExecuteAsync(sqlEndereco, new
            {
                endereco.Rua,
                endereco.Numero,
                endereco.Cidade,
                endereco.Estado,
                endereco.CEP,
                Id = enderecoId
            });
        }

        public async Task<bool> PossuiCirculacaoAsync(int id)
        {
            using var conexao = AbrirConexao();

            const string sql = @"
                SELECT (SELECT COUNT(1) FROM Emprestimo WHERE LeitorId = @Id)
                     + (SELECT COUNT(1) FROM Reserva    WHERE LeitorId = @Id);";

            return await conexao.ExecuteScalarAsync<long>(sql, new { Id = id }) > 0;
        }

        public async Task ExcluirAsync(int id)
        {
            using var conexao = AbrirConexao();

            const string sqlEnderecoId = "SELECT EnderecoId FROM Usuario WHERE Id = @Id;";
            var enderecoId = await conexao.ExecuteScalarAsync<int?>(sqlEnderecoId, new { Id = id });

            await conexao.ExecuteAsync("DELETE FROM Usuario WHERE Id = @Id;", new { Id = id });

            if (enderecoId != null)
                await conexao.ExecuteAsync("DELETE FROM Endereco WHERE Id = @Id;", new { Id = enderecoId });
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