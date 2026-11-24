using BibliotecaDigital.Web.Exceptions;

namespace BibliotecaDigital.Web.Models;

// Faz parte do Livro (composicao): quem cria o exemplar e o metodo Livro.AdicionarExemplar.
public sealed class ExemplarDigital
{
    public ExemplarDigital(string formato, string arquivo)
    {
        if (string.IsNullOrWhiteSpace(formato))
        {
            throw new DominioException("Informe o formato do exemplar (ex.: PDF, EPUB).");
        }

        if (string.IsNullOrWhiteSpace(arquivo))
        {
            throw new DominioException("Informe o arquivo do exemplar.");
        }

        Formato = formato.Trim().ToUpper();
        Arquivo = arquivo.Trim();
        Disponivel = true;
    }

    public int Id { get; private set; }

    public string Formato { get; private set; }

    public string Arquivo { get; private set; }

    public bool Disponivel { get; private set; }

    public bool VerificarDisponibilidade()
    {
        return Disponivel;
    }

    public void AlterarDisponibilidade(bool disponivel)
    {
        Disponivel = disponivel;
    }

    public void DefinirId(int id) => Id = id;

    public static ExemplarDigital Reconstruir(int id, string formato, string arquivo, bool disponivel)
    {
        return new ExemplarDigital(formato, arquivo)
        {
            Id = id,
            Disponivel = disponivel
        };
    }
}
