using System.Collections.Generic;
using BibliotecaDigital.Web.Exceptions;

namespace BibliotecaDigital.Web.Models
{
    public class Leitor : Usuario
    {
        public int LimiteEmprestimos { get; private set; }
        public int LivrosAtualmenteEmprestados { get; private set; }

        public Leitor(string nome, string email, string senha, Endereco endereco,
            int id = 0, bool estaBloqueado = false, int limiteEmprestimos = 3, int livrosEmprestados = 0)
            : base(nome, email, senha, endereco, id, estaBloqueado)
        {
            LimiteEmprestimos = limiteEmprestimos;
            LivrosAtualmenteEmprestados = livrosEmprestados;
        }

        public override List<string> ObterPermissoes() =>
            new List<string> { "Pesquisar Acervo", "Realizar Empréstimo", "Renovar Empréstimo" };

        public override string ExecutarOperacaoEspecifica()
        {
            if (EstaBloqueado)
                throw new LeitorBloqueadoException("Leitor bloqueado não pode realizar empréstimos.");

            return "Acesso liberado ao catálogo de empréstimos.";
        }
    }
}