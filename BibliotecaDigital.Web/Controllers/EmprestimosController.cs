using BibliotecaDigital.Web.Exceptions;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Models.Enums;
using BibliotecaDigital.Web.Models.ViewModels;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BibliotecaDigital.Web.Controllers;

public class EmprestimosController : ControladorBase
{
    private readonly IEmprestimoRepository _emprestimoRepository;
    private readonly IReservaRepository _reservaRepository;
    private readonly ICirculacaoRepository _circulacaoRepository;

    public EmprestimosController(
        IEmprestimoRepository emprestimoRepository,
        IReservaRepository reservaRepository,
        ICirculacaoRepository circulacaoRepository)
    {
        _emprestimoRepository = emprestimoRepository;
        _reservaRepository = reservaRepository;
        _circulacaoRepository = circulacaoRepository;
    }

    public IActionResult Index() => View(_emprestimoRepository.ObterTodos().ToList());

    public IActionResult Detalhes(int id)
    {
        var emprestimo = _emprestimoRepository.ObterComDetalhes(id);

        if (emprestimo is null)
        {
            Alerta("Emprestimo nao encontrado.");
            return RedirectToAction(nameof(Index));
        }

        ViewBag.ReservaPendente = _reservaRepository
            .ExisteReservaPendenteParaOutroLeitor(emprestimo.ExemplarId, emprestimo.LeitorId);

        return View(emprestimo);
    }

    public IActionResult Registrar() => View(PrepararFormulario(new EmprestimoFormViewModel()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Registrar(EmprestimoFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(PrepararFormulario(modelo));
        }

        return ExecutarRegraNaView(() =>
        {
            var leitor = _circulacaoRepository.ObterLeitor(modelo.LeitorId)
                ?? throw new RegistroNaoEncontradoException("Leitor", modelo.LeitorId);

            var exemplar = _circulacaoRepository.ObterExemplar(modelo.ExemplarId)
                ?? throw new RegistroNaoEncontradoException("Exemplar digital", modelo.ExemplarId);

            leitor.GarantirQuePodeEmprestar();

            if (_reservaRepository.ExisteReservaPendenteParaOutroLeitor(exemplar.Id, leitor.Id))
            {
                throw new DominioException("Existe fila de reserva para este exemplar; efetive a reserva pela tela de reservas.");
            }

            exemplar.ConsumirLicenca();

            var emprestimo = new Emprestimo(leitor.Id, exemplar.Id, exemplar.ValorReferenciaObra);
            _emprestimoRepository.Inserir(emprestimo);
            _circulacaoRepository.AtualizarLicencas(exemplar);

            Sucesso($"Emprestimo registrado. Devolucao prevista para {emprestimo.DataPrevista:dd/MM/yyyy}.");
            return RedirectToAction(nameof(Detalhes), new { id = emprestimo.Id });
        }, () => View(PrepararFormulario(modelo)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Renovar(int id) => ExecutarRegra(() =>
    {
        var emprestimo = _emprestimoRepository.ObterComDetalhes(id)
            ?? throw new RegistroNaoEncontradoException("Emprestimo", id);

        var existeReserva = _reservaRepository
            .ExisteReservaPendenteParaOutroLeitor(emprestimo.ExemplarId, emprestimo.LeitorId);

        var renovacao = emprestimo.Renovar(existeReserva);

        _emprestimoRepository.RegistrarRenovacao(renovacao);
        _emprestimoRepository.Atualizar(emprestimo);

        Sucesso($"Emprestimo renovado. Nova data prevista: {emprestimo.DataPrevista:dd/MM/yyyy}.");
        return RedirectToAction(nameof(Detalhes), new { id });
    }, mensagem =>
    {
        Alerta(mensagem);
        return RedirectToAction(nameof(Detalhes), new { id });
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Devolver(int id) => ExecutarRegra(() =>
    {
        var emprestimo = _emprestimoRepository.ObterComDetalhes(id)
            ?? throw new RegistroNaoEncontradoException("Emprestimo", id);

        var exemplar = _circulacaoRepository.ObterExemplar(emprestimo.ExemplarId)
            ?? throw new RegistroNaoEncontradoException("Exemplar digital", emprestimo.ExemplarId);

        var devolucao = emprestimo.Devolver(Multa.ValorDiarioPadrao);

        _emprestimoRepository.Atualizar(emprestimo);
        _emprestimoRepository.RegistrarDevolucao(devolucao);

        exemplar.LiberarLicenca();
        _circulacaoRepository.AtualizarLicencas(exemplar);

        if (emprestimo.Multa is not null)
        {
            _emprestimoRepository.RegistrarMulta(emprestimo.Multa);
            Alerta($"Devolucao com {emprestimo.Multa.DiasAtraso} dia(s) de atraso. Multa gerada: {emprestimo.Multa.ValorTotal:C}.");
        }
        else
        {
            Sucesso("Devolucao registrada no prazo e licenca liberada.");
        }

        LiberarProximaReserva(emprestimo.ExemplarId);

        return RedirectToAction(nameof(Detalhes), new { id });
    }, mensagem =>
    {
        Alerta(mensagem);
        return RedirectToAction(nameof(Detalhes), new { id });
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult QuitarMulta(int id) => ExecutarRegra(() =>
    {
        var multa = _emprestimoRepository.ObterMultaPorEmprestimo(id)
            ?? throw new DominioException("Este emprestimo nao possui multa registrada.");

        multa.Quitar();
        _emprestimoRepository.QuitarMulta(multa);

        Sucesso($"Multa de {multa.ValorTotal:C} quitada.");
        return RedirectToAction(nameof(Detalhes), new { id });
    }, mensagem =>
    {
        Alerta(mensagem);
        return RedirectToAction(nameof(Detalhes), new { id });
    });

    private void LiberarProximaReserva(int exemplarId)
    {
        var proxima = _reservaRepository.ObterPendentesPorExemplar(exemplarId)
            .FirstOrDefault(reserva => reserva.Status == StatusReserva.Aguardando);

        if (proxima is null)
        {
            return;
        }

        proxima.Disponibilizar();
        _reservaRepository.Atualizar(proxima);
    }

    private EmprestimoFormViewModel PrepararFormulario(EmprestimoFormViewModel modelo)
    {
        modelo.Leitores = _circulacaoRepository.ObterLeitores()
            .Select(leitor => new SelectListItem(leitor.Resumo(), leitor.Id.ToString()))
            .ToList();

        modelo.Exemplares = _circulacaoRepository.ObterExemplares()
            .Select(exemplar => new SelectListItem(exemplar.Resumo(), exemplar.Id.ToString()))
            .ToList();

        return modelo;
    }
}
