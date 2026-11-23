using BibliotecaDigital.Web.Exceptions;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Models.ViewModels;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BibliotecaDigital.Web.Controllers;

public class ReservasController : ControladorBase
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IEmprestimoRepository _emprestimoRepository;
    private readonly ICirculacaoRepository _circulacaoRepository;

    public ReservasController(
        IReservaRepository reservaRepository,
        IEmprestimoRepository emprestimoRepository,
        ICirculacaoRepository circulacaoRepository)
    {
        _reservaRepository = reservaRepository;
        _emprestimoRepository = emprestimoRepository;
        _circulacaoRepository = circulacaoRepository;
    }

    public IActionResult Index() => View(_reservaRepository.ObterTodos().ToList());

    public IActionResult Criar() => View(PrepararFormulario(new ReservaFormViewModel()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Criar(ReservaFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(PrepararFormulario(modelo));
        }

        return ExecutarRegraNaView(() =>
        {
            var leitor = _circulacaoRepository.ObterLeitor(modelo.LeitorId)
                ?? throw new RegistroNaoEncontradoException("Leitor", modelo.LeitorId);

            leitor.GarantirQuePodeReservar();

            var exemplar = _circulacaoRepository.ObterExemplar(modelo.ExemplarId)
                ?? throw new RegistroNaoEncontradoException("Exemplar digital", modelo.ExemplarId);

            var posicao = _reservaRepository.ProximaPosicaoNaFila(exemplar.Id);
            var reserva = new Reserva(leitor.Id, exemplar.Id, posicao);

            if (exemplar.VerificarDisponibilidade())
            {
                reserva.Disponibilizar();
            }

            _reservaRepository.Inserir(reserva);

            Sucesso($"Reserva registrada na posicao {posicao} da fila. Retirada ate {reserva.DataLimiteRetirada:dd/MM/yyyy}.");
            return RedirectToAction(nameof(Index));
        }, () => View(PrepararFormulario(modelo)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancelar(int id) => ExecutarRegraComRedirecionamento(() =>
    {
        var reserva = _reservaRepository.ObterPorId(id)
            ?? throw new RegistroNaoEncontradoException("Reserva", id);

        reserva.Cancelar();
        _reservaRepository.Atualizar(reserva);

        Sucesso("Reserva cancelada.");
        return RedirectToAction(nameof(Index));
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Expirar(int id) => ExecutarRegraComRedirecionamento(() =>
    {
        var reserva = _reservaRepository.ObterPorId(id)
            ?? throw new RegistroNaoEncontradoException("Reserva", id);

        reserva.Expirar();
        _reservaRepository.Atualizar(reserva);

        Alerta("Reserva marcada como expirada.");
        return RedirectToAction(nameof(Index));
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Efetivar(int id) => ExecutarRegraComRedirecionamento(() =>
    {
        var reserva = _reservaRepository.ObterPorId(id)
            ?? throw new RegistroNaoEncontradoException("Reserva", id);

        var leitor = _circulacaoRepository.ObterLeitor(reserva.LeitorId)
            ?? throw new RegistroNaoEncontradoException("Leitor", reserva.LeitorId);

        var exemplar = _circulacaoRepository.ObterExemplar(reserva.ExemplarId)
            ?? throw new RegistroNaoEncontradoException("Exemplar digital", reserva.ExemplarId);

        leitor.GarantirQuePodeEmprestar();

        try
        {
            var emprestimo = reserva.Efetivar(exemplar.ValorReferenciaObra);

            exemplar.ConsumirLicenca();

            _emprestimoRepository.Inserir(emprestimo);
            _circulacaoRepository.AtualizarLicencas(exemplar);
            _reservaRepository.Atualizar(reserva);

            Sucesso($"Reserva efetivada. Devolucao prevista para {emprestimo.DataPrevista:dd/MM/yyyy}.");
            return RedirectToAction("Detalhes", "Emprestimos", new { id = emprestimo.Id });
        }
        catch (ReservaExpiradaException)
        {
            _reservaRepository.Atualizar(reserva);
            throw;
        }
    });

    private ReservaFormViewModel PrepararFormulario(ReservaFormViewModel modelo)
    {
        modelo.Leitores = _circulacaoRepository.ObterLeitores()
            .Select(leitor => new SelectListItem(leitor.Nome, leitor.Id.ToString()))
            .ToList();

        modelo.Exemplares = _circulacaoRepository.ObterExemplares()
            .Select(exemplar => new SelectListItem(exemplar.Resumo(), exemplar.Id.ToString()))
            .ToList();

        return modelo;
    }
}
