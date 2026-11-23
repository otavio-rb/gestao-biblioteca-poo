using System.Collections.Generic;
using BibliotecaDigital.Web.Exceptions;

namespace BibliotecaDigital.Web.Models
{
    public abstract class Usuario
    {
public int Id { get; protected set; }
public string Nome { get; private set; } = string.Empty;
public string Email { get; private set; } = string.Empty;
public string Senha { get; private set; } = string.Empty;
public bool EstaBloqueado { get; protected set; }
public Endereco EnderecoResidencial { get; private set; } = null!;

        protected Usuario(string nome, string email, string senha, Endereco endereco, int id = 0, bool estaBloqueado = false)
        {
            Id = id;
            EstaBloqueado = estaBloqueado;
            AtualizarDados(nome, email, endereco);
            DefinirSenha(senha);
        }

        public void AtualizarDados(string nome, string email, Endereco endereco)
        {
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email))
                throw new DominioException("Nome e Email são obrigatórios.");

            Nome = nome;
            Email = email;
            EnderecoResidencial = endereco ?? throw new DominioException("Endereço é obrigatório.");
        }

        private void DefinirSenha(string senha)
        {
            if (string.IsNullOrWhiteSpace(senha) || senha.Length < 6)
                throw new DominioException("A senha deve ter no mínimo 6 caracteres.");
            Senha = senha;
        }

        public virtual void Bloquear()
        {
            if (EstaBloqueado)
                throw new DominioException("O usuário já está bloqueado.");

            EstaBloqueado = true;
        }

        public virtual void Desbloquear()
        {
            EstaBloqueado = false;
        }

        public abstract List<string> ObterPermissoes();
        public abstract string ExecutarOperacaoEspecifica();
    }
}