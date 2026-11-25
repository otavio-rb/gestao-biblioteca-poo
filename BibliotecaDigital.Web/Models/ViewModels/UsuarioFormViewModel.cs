namespace BibliotecaDigital.Web.Models.ViewModels
{
    public class UsuarioFormViewModel
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public bool EhAdministrador { get; set; }
        public string? Departamento { get; set; }

        public string Rua { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string CEP { get; set; } = string.Empty;

        public static UsuarioFormViewModel DoUsuario(Usuario usuario)
        {
            var endereco = usuario.EnderecoResidencial;

            return new UsuarioFormViewModel
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                EhAdministrador = usuario is Administrador,
                Departamento = (usuario as Administrador)?.Departamento,
                Rua = endereco.Rua,
                Numero = endereco.Numero,
                Cidade = endereco.Cidade,
                Estado = endereco.Estado,
                CEP = endereco.CEP
            };
        }
    }
}
