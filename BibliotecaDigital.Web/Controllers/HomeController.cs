using BibliotecaDigital.Web.Models.Enums;
using BibliotecaDigital.Web.Models.ViewModels;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaDigital.Web.Controllers;

public class HomeController : ControladorBase
{
    private readonly IEmprestimoRepository _emprestimoRepository;
    private readonly IReservaRepository _reservaRepository;
    private readonly ICirculacaoRepository _circulacaoRepository;

    public HomeController(
        IEmprestimoRepository emprestimoRepository,
        IReservaRepository reservaRepository,
        ICirculacaoRepository circulacaoRepository)
    {
        _emprestimoRepository = emprestimoRepository;
        _reservaRepository = reservaRepository;
        _circulacaoRepository = circulacaoRepository;
    }

    public IActionResult Index()
    {
        var emprestimos = _emprestimoRepository.ObterTodos().ToList();
        var reservas = _reservaRepository.ObterTodos().ToList();
        var exemplares = _circulacaoRepository.ObterExemplares().ToList();
        var leitores = _circulacaoRepository.ObterLeitores().ToList();

        var ativos = emprestimos
            .Where(emprestimo => emprestimo.Status is StatusEmprestimo.Ativo or StatusEmprestimo.Renovado)
            .ToList();

        var painel = new PainelViewModel
        {
            EmprestimosAtivos = ativos.Count,
            EmprestimosEmAtraso = ativos.Count(emprestimo => emprestimo.EstaEmAtraso()),
            EmprestimosEncerrados = emprestimos.Count(emprestimo => emprestimo.EstaEncerrado),
            ReservasPendentes = reservas.Count(reserva => reserva.EstaPendente),
            LicencasDisponiveis = exemplares.Sum(exemplar => exemplar.LicencasDisponiveis),
            MultasEmAberto = leitores.Sum(leitor => leitor.MultasEmAberto),
            UltimosEmprestimos = emprestimos.Take(5).ToList(),
            FilaDeReservas = reservas.Where(reserva => reserva.EstaPendente).Take(5).ToList()
        };

        return View(painel);
    }

    [Route("Home/Erro")]
    public IActionResult Erro() => View();
}
