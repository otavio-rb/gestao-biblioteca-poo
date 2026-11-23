namespace BibliotecaDigital.Web.Models.ViewModels;

public class PainelViewModel
{
    public int EmprestimosAtivos { get; init; }

    public int EmprestimosEmAtraso { get; init; }

    public int EmprestimosEncerrados { get; init; }

    public int ReservasPendentes { get; init; }

    public int LicencasDisponiveis { get; init; }

    public decimal MultasEmAberto { get; init; }

    public IReadOnlyList<Emprestimo> UltimosEmprestimos { get; init; } = new List<Emprestimo>();

    public IReadOnlyList<Reserva> FilaDeReservas { get; init; } = new List<Reserva>();
}
