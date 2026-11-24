using BibliotecaDigital.Web.Exceptions;
using BibliotecaDigital.Web.Models.ViewModels;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaDigital.Web.Controllers;

public class ExemplaresController : ControladorBase
{
    private readonly IExemplarRepository _exemplarRepository;
    private readonly ILivroRepository _livroRepository;

    public ExemplaresController(IExemplarRepository exemplarRepository, ILivroRepository livroRepository)
    {
        _exemplarRepository = exemplarRepository;
        _livroRepository = livroRepository;
    }

    public IActionResult Criar(int livroId)
    {
        var livro = _livroRepository.ObterComDetalhes(livroId);

        if (livro is null)
        {
            Alerta("Livro nao encontrado.");
            return RedirectToAction("Index", "Livros");
        }

        return View(new ExemplarFormViewModel { LivroId = livro.Id, TituloLivro = livro.Titulo });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Criar(ExemplarFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        return ExecutarRegraNaView(() =>
        {
            var livro = _livroRepository.ObterComDetalhes(modelo.LivroId)
                ?? throw new RegistroNaoEncontradoException("Livro", modelo.LivroId);

            var exemplar = livro.AdicionarExemplar(modelo.Formato, modelo.Arquivo);
            _exemplarRepository.Inserir(livro.Id, exemplar, modelo.LicencasTotais);

            Sucesso("Exemplar digital cadastrado com sucesso.");
            return RedirectToAction("Detalhes", "Livros", new { id = modelo.LivroId });
        }, () => View(modelo));
    }

    public IActionResult Editar(int id)
    {
        var exemplar = _exemplarRepository.ObterPorId(id);

        if (exemplar is null)
        {
            Alerta("Exemplar digital nao encontrado.");
            return RedirectToAction("Index", "Livros");
        }

        var livroId = _exemplarRepository.ObterLivroId(id);
        var livro = _livroRepository.ObterComDetalhes(livroId);
        var licencas = _exemplarRepository.ObterLicencasTotais(id);

        return View(ExemplarFormViewModel.DoExemplar(exemplar, livroId, livro?.Titulo ?? string.Empty, licencas));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(ExemplarFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        return ExecutarRegraNaView(() =>
        {
            var exemplar = _exemplarRepository.ObterPorId(modelo.Id)
                ?? throw new RegistroNaoEncontradoException("Exemplar digital", modelo.Id);

            var atualizado = Models.ExemplarDigital.Reconstruir(modelo.Id, modelo.Formato, modelo.Arquivo, modelo.Disponivel);
            _exemplarRepository.Atualizar(atualizado, modelo.LicencasTotais);

            Sucesso("Exemplar digital atualizado com sucesso.");
            return RedirectToAction("Detalhes", "Livros", new { id = modelo.LivroId });
        }, () => View(modelo));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Excluir(int id, int livroId) => ExecutarRegra(() =>
    {
        _exemplarRepository.Remover(id);
        Sucesso("Exemplar digital excluido.");
        return RedirectToAction("Detalhes", "Livros", new { id = livroId });
    }, mensagem =>
    {
        Alerta(mensagem);
        return RedirectToAction("Detalhes", "Livros", new { id = livroId });
    });
}
