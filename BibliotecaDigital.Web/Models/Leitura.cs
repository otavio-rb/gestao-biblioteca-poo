using BibliotecaDigital.Web.Exceptions;
using BibliotecaDigital.Web.Models.Enums;

namespace BibliotecaDigital.Web.Models;

public sealed class Leitura
{
    public Leitura(int usuarioId, int livroId, int totalPaginas)
    {
        if (usuarioId <= 0 || livroId <= 0)
        {
            throw new DominioException("A leitura exige um usuario e um livro validos.");
        }

        UsuarioId = usuarioId;
        LivroId = livroId;
        Status = StatusLeitura.NaoIniciada;

        // Composicao: a propria Leitura cria o seu progresso.
        Progresso = new ProgressoLeitura(totalPaginas);
    }

    public int Id { get; private set; }

    public int UsuarioId { get; private set; }

    public int LivroId { get; private set; }

    public StatusLeitura Status { get; private set; }

    public ProgressoLeitura Progresso { get; private set; }

    public DateTime? DataInicio { get; private set; }

    public DateTime? DataConclusao { get; private set; }

    // Dados so para exibicao nas telas (preenchidos pelo repositorio).
    public string TituloLivro { get; private set; } = string.Empty;

    public string NomeLeitor { get; private set; } = string.Empty;

    public void Iniciar()
    {
        if (Status != StatusLeitura.NaoIniciada)
        {
            throw new DominioException("Esta leitura ja foi iniciada.");
        }

        Status = StatusLeitura.EmAndamento;
        DataInicio = DateTime.Now;
    }

    public void Pausar()
    {
        if (Status != StatusLeitura.EmAndamento)
        {
            throw new DominioException("So e possivel pausar uma leitura em andamento.");
        }

        Status = StatusLeitura.Pausada;
    }

    public void Retomar()
    {
        if (Status != StatusLeitura.Pausada)
        {
            throw new DominioException("So e possivel retomar uma leitura pausada.");
        }

        Status = StatusLeitura.EmAndamento;
    }

    public void RegistrarPagina(int pagina)
    {
        // Regra: so avanca com status EM_ANDAMENTO.
        if (Status != StatusLeitura.EmAndamento)
        {
            throw new DominioException("So e possivel registrar paginas com a leitura em andamento.");
        }

        Progresso.AvancarPara(pagina);

        // Regra: ao chegar em 100% a leitura e concluida.
        if (Progresso.EstaCompleto())
        {
            Concluir();
        }
    }

    public void Concluir()
    {
        if (Status == StatusLeitura.NaoIniciada)
        {
            throw new DominioException("Nao e possivel concluir uma leitura que nem comecou.");
        }

        if (Status == StatusLeitura.Concluida)
        {
            throw new DominioException("Esta leitura ja foi concluida.");
        }

        Progresso.AvancarPara(Progresso.TotalPaginas);
        Status = StatusLeitura.Concluida;
        DataConclusao = DateTime.Now;
    }

    public bool EstaConcluida()
    {
        return Status == StatusLeitura.Concluida;
    }

    public double GetPercentual()
    {
        return Progresso.GetPercentual();
    }

    public void DescreverContexto(string tituloLivro, string nomeLeitor)
    {
        TituloLivro = tituloLivro;
        NomeLeitor = nomeLeitor;
    }

    public void DefinirId(int id) => Id = id;

    public static Leitura Reconstruir(
        int id,
        int usuarioId,
        int livroId,
        StatusLeitura status,
        ProgressoLeitura progresso,
        DateTime? dataInicio,
        DateTime? dataConclusao)
    {
        return new Leitura(usuarioId, livroId, progresso.TotalPaginas)
        {
            Id = id,
            Status = status,
            Progresso = progresso,
            DataInicio = dataInicio,
            DataConclusao = dataConclusao
        };
    }
}
