using BibliotecaDigital.Web.Exceptions;
using BibliotecaDigital.Web.Models.Enums;

namespace BibliotecaDigital.Web.Models;

// Agregacao: o historico reune as leituras de um usuario,
// mas cada Leitura existe por conta propria.
public sealed class HistoricoLeitura
{
    private readonly List<Leitura> _leituras = new();

    public HistoricoLeitura(int usuarioId)
    {
        if (usuarioId <= 0)
        {
            throw new DominioException("O historico exige um usuario valido.");
        }

        UsuarioId = usuarioId;
    }

    public int Id { get; private set; }

    public int UsuarioId { get; private set; }

    public void Registrar(Leitura leitura)
    {
        if (leitura == null)
        {
            throw new DominioException("Informe a leitura que sera registrada.");
        }

        if (leitura.UsuarioId != UsuarioId)
        {
            throw new DominioException("A leitura pertence a outro usuario.");
        }

        if (BuscarPorLivro(leitura.LivroId) != null)
        {
            throw new DominioException("Este livro ja esta no historico do usuario.");
        }

        _leituras.Add(leitura);
    }

    // Retorna null quando o livro nao esta no historico (equivale ao Optional do diagrama).
    public Leitura? BuscarPorLivro(int livroId)
    {
        foreach (var leitura in _leituras)
        {
            if (leitura.LivroId == livroId)
            {
                return leitura;
            }
        }

        return null;
    }

    public List<Leitura> ListarEmAndamento()
    {
        var resultado = new List<Leitura>();

        foreach (var leitura in _leituras)
        {
            if (leitura.Status == StatusLeitura.EmAndamento)
            {
                resultado.Add(leitura);
            }
        }

        return resultado;
    }

    public List<Leitura> ListarConcluidas()
    {
        var resultado = new List<Leitura>();

        foreach (var leitura in _leituras)
        {
            if (leitura.EstaConcluida())
            {
                resultado.Add(leitura);
            }
        }

        return resultado;
    }

    public int TotalLivrosLidos()
    {
        return ListarConcluidas().Count;
    }

    // Devolve uma lista somente leitura para ninguem alterar o historico por fora.
    public IReadOnlyList<Leitura> GetLeituras()
    {
        return _leituras;
    }

    public void DefinirId(int id) => Id = id;
}
