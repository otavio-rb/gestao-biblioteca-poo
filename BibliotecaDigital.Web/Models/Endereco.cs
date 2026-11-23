using BibliotecaDigital.Web.Exceptions;

namespace BibliotecaDigital.Web.Models
{
    public class Endereco
    {
public int Id { get; private set; }
public string Rua { get; private set; } = string.Empty;
public string Numero { get; private set; } = string.Empty;
public string Cidade { get; private set; } = string.Empty;
public string Estado { get; private set; } = string.Empty;
public string CEP { get; private set; } = string.Empty;

        public Endereco(int id, string rua, string numero, string cidade, string estado, string cep)
            : this(rua, numero, cidade, estado, cep)
        {
            Id = id;
        }

        public Endereco(string rua, string numero, string cidade, string estado, string cep)
        {
            AtualizarEndereco(rua, numero, cidade, estado, cep);
        }

        public void AtualizarEndereco(string rua, string numero, string cidade, string estado, string cep)
        {
            if (string.IsNullOrWhiteSpace(rua) || string.IsNullOrWhiteSpace(cidade))
                throw new DominioException("Rua e Cidade são obrigatórios.");

            Rua = rua;
            Numero = numero;
            Cidade = cidade;
            Estado = estado;
            CEP = cep;
        }

        public override string ToString() => $"{Rua}, {Numero} - {Cidade}/{Estado} ({CEP})";
    }
}