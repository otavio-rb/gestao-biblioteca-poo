using BibliotecaDigital.Web.Exceptions;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Models.ViewModels;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BibliotecaDigital.Web.Controllers;

public class LeiturasController : ControladorBase
{
    private readonly ILeituraRepository _leituraRepository;
    private readonly ILivroRepository _livroRepository;
    private readonly ICirculacaoRepository _circulacaoRepository;

    public LeiturasController(
        ILeituraRepository leituraRepository,
        ILivroRepository livroRepository,
        ICirculacaoRepository circulacaoRepository)
    {
        _leituraRepository = leituraRepository;
        _livroRepository = livroRepository;
        _circulacaoRepository = circulacaoRepository;
    }

    public IActionResult Index()
    {
        var leitores = _circulacaoRepository.ObterLeitores().ToDictionary(leitor => leitor.Id, leitor => leitor.Nome);
        var livros = _livroRepository.ObterTodos().ToDictionary(livro => livro.Id, livro => livro.Titulo);

        var lista = _leituraRepository.ObterTodos()
            .Select(leitura => new LeituraListaViewModel
            {
                Leitura = leitura,
                NomeLeitor = leitores.TryGetValue(leitura.UsuarioId, out var nome) ? nome : $"Usuario {leitura.UsuarioId}",
                TituloLivro = livros.TryGetValue(leitura.LivroId, out var titulo) ? titulo : $"Livro {leitura.LivroId}"
            })
            .ToList();

        return View(lista);
    }

    public IActionResult Detalhes(int id)
    {
        var leitura = _leituraRepository.ObterPorId(id);

        if (leitura is null)
        {
            Alerta("Leitura nao encontrada.");
            return RedirectToAction(nameof(Index));
        }

        var leitor = _circulacaoRepository.ObterLeitor(leitura.UsuarioId);
        var livro = _livroRepository.ObterComDetalhes(leitura.LivroId);

        return View(new LeituraDetalhesViewModel
        {
            Leitura = leitura,
            NomeLeitor = leitor?.Nome ?? $"Usuario {leitura.UsuarioId}",
            TituloLivro = livro?.Titulo ?? $"Livro {leitura.LivroId}"
        });
    }

    public IActionResult Criar() => View(PrepararFormulario(new LeituraFormViewModel()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Criar(LeituraFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(PrepararFormulario(modelo));
        }

        return ExecutarRegraNaView(() =>
        {
            if (_leituraRepository.ObterPorUsuarioELivro(modelo.UsuarioId, modelo.LivroId) is not null)
            {
                throw new DominioException("Este leitor ja possui uma sessao de leitura registrada para a obra.");
            }

            var leitura = new Leitura(modelo.UsuarioId, modelo.LivroId, modelo.TotalPaginas);
            var id = _leituraRepository.Inserir(leitura);

            Sucesso("Sessao de leitura criada. Inicie a leitura para registrar o progresso.");
            return RedirectToAction(nameof(Detalhes), new { id });
        }, () => View(PrepararFormulario(modelo)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Iniciar(int id) => AlterarEstado(id, leitura => leitura.Iniciar(), "Leitura iniciada.");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Pausar(int id) => AlterarEstado(id, leitura => leitura.Pausar(), "Leitura pausada.");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Retomar(int id) => AlterarEstado(id, leitura => leitura.Retomar(), "Leitura retomada.");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Concluir(int id) => AlterarEstado(id, leitura => leitura.Concluir(), "Leitura concluida.");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RegistrarPagina(RegistrarPaginaViewModel modelo)
        => AlterarEstado(modelo.LeituraId, leitura => leitura.RegistrarPagina(modelo.Pagina), "Progresso atualizado.");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Excluir(int id) => ExecutarRegraComRedirecionamento(() =>
    {
        _leituraRepository.Remover(id);
        Sucesso("Sessao de leitura excluida.");
        return RedirectToAction(nameof(Index));
    });

    private IActionResult AlterarEstado(int id, Action<Leitura> operacao, string mensagemSucesso) => ExecutarRegra(() =>
    {
        var leitura = _leituraRepository.ObterPorId(id)
            ?? throw new RegistroNaoEncontradoException("Leitura", id);

        operacao(leitura);
        _leituraRepository.Atualizar(leitura);

        Sucesso(mensagemSucesso);
        return RedirectToAction(nameof(Detalhes), new { id });
    }, mensagem =>
    {
        Alerta(mensagem);
        return RedirectToAction(nameof(Detalhes), new { id });
    });

    private LeituraFormViewModel PrepararFormulario(LeituraFormViewModel modelo)
    {
        modelo.Leitores = _circulacaoRepository.ObterLeitores()
            .Select(leitor => new SelectListItem(leitor.Nome, leitor.Id.ToString()))
            .ToList();

        modelo.Livros = _livroRepository.ObterTodos()
            .Select(livro => new SelectListItem(livro.Titulo, livro.Id.ToString()))
            .ToList();

        return modelo;
    }
}
