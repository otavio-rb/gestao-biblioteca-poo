using BibliotecaDigital.Web.Exceptions;

namespace BibliotecaDigital.Web.Models;

public sealed class Renovacao
{
    public Renovacao(int emprestimoId, DateTime dataRenovacao, DateTime novaDataPrevista, int sequencia)
    {
        if (novaDataPrevista <= dataRenovacao)
        {
            throw new DominioException("A nova data prevista deve ser posterior a data da renovacao.");
        }

        if (sequencia < 1)
        {
            throw new DominioException("A sequencia da renovacao deve iniciar em 1.");
        }

        EmprestimoId = emprestimoId;
        DataRenovacao = dataRenovacao;
        NovaDataPrevista = novaDataPrevista;
        Sequencia = sequencia;
    }

    public int Id { get; private set; }

    public int EmprestimoId { get; private set; }

    public DateTime DataRenovacao { get; private set; }

    public DateTime NovaDataPrevista { get; private set; }

    public int Sequencia { get; private set; }

    public void DefinirId(int id) => Id = id;

    public static Renovacao Reconstruir(int id, int emprestimoId, DateTime dataRenovacao, DateTime novaDataPrevista, int sequencia)
        => new(emprestimoId, dataRenovacao, novaDataPrevista, sequencia) { Id = id };
}
