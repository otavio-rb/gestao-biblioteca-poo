using System.Collections.Generic;
using BibliotecaDigital.Web.Exceptions;

namespace BibliotecaDigital.Web.Models
{
    public class Administrador : Usuario
    {
        public string Departamento { get; private set; }

        public Administrador(string nome, string email, string senha, Endereco endereco,
            string departamento, int id = 0, bool estaBloqueado = false)
            : base(nome, email, senha, endereco, id, estaBloqueado)
        {
            Departamento = departamento;
        }

        public override void Bloquear()
        {
            if (Departamento == "TI_Root")
                throw new DominioException("Administradores do nível Root não podem ser bloqueados.");

            base.Bloquear();
        }

        public override List<string> ObterPermissoes() =>
            new List<string> { "Gerenciar Acervo", "Gerenciar Usuários", "Emitir Relatórios", "Bloquear Leitores" };

        public override string ExecutarOperacaoEspecifica() =>
            "Acesso liberado ao painel administrativo.";
    }
}