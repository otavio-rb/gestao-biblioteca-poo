using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Models.ViewModels;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaDigital.Web.Controllers;

public class AutoresController : ControladorBase
{
    private readonly IAutorRepository _autorRepository;

    public AutoresController(IAutorRepository autorRepository)
    {
        _autorRepository = autorRepository;
    }

    public IActionResult Index() => View(_autorRepository.ObterTodos().ToList());

    public IActionResult Criar() => View(new AutorFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Criar(AutorFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        return ExecutarRegraNaView(() =>
        {
            var autor = new Autor(modelo.Nome, modelo.Nacionalidade);
            _autorRepository.Inserir(autor);

            Sucesso($"Autor {autor.Nome} cadastrado com sucesso.");
            return RedirectToAction(nameof(Index));
        }, () => View(modelo));
    }

    public IActionResult Editar(int id)
    {
        var autor = _autorRepository.ObterPorId(id);

        if (autor is null)
        {
            Alerta("Autor nao encontrado.");
            return RedirectToAction(nameof(Index));
        }

        return View(AutorFormViewModel.DoAutor(autor));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(AutorFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        return ExecutarRegraNaView(() =>
        {
            var autor = _autorRepository.ObterPorId(modelo.Id);

            if (autor is null)
            {
                Alerta("Autor nao encontrado.");
                return RedirectToAction(nameof(Index));
            }

            autor.Editar(modelo.Nome, modelo.Nacionalidade);
            _autorRepository.Atualizar(autor);

            Sucesso($"Autor {autor.Nome} atualizado com sucesso.");
            return RedirectToAction(nameof(Index));
        }, () => View(modelo));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Excluir(int id) => ExecutarRegraComRedirecionamento(() =>
    {
        if (_autorRepository.PossuiLivrosVinculados(id))
        {
            Alerta("Este autor possui obras no catalogo e nao pode ser excluido.");
            return RedirectToAction(nameof(Index));
        }

        _autorRepository.Remover(id);
        Sucesso("Autor excluido com sucesso.");
        return RedirectToAction(nameof(Index));
    });
}
