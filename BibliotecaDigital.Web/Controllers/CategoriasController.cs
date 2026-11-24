using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Models.ViewModels;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaDigital.Web.Controllers;

public class CategoriasController : ControladorBase
{
    private readonly ICategoriaRepository _categoriaRepository;

    public CategoriasController(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public IActionResult Index() => View(_categoriaRepository.ObterTodos().ToList());

    public IActionResult Criar() => View(new CategoriaFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Criar(CategoriaFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        return ExecutarRegraNaView(() =>
        {
            var categoria = new Categoria(modelo.Nome, modelo.Descricao);
            _categoriaRepository.Inserir(categoria);

            Sucesso($"Categoria {categoria.Nome} cadastrada com sucesso.");
            return RedirectToAction(nameof(Index));
        }, () => View(modelo));
    }

    public IActionResult Editar(int id)
    {
        var categoria = _categoriaRepository.ObterPorId(id);

        if (categoria is null)
        {
            Alerta("Categoria nao encontrada.");
            return RedirectToAction(nameof(Index));
        }

        return View(CategoriaFormViewModel.DaCategoria(categoria));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(CategoriaFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        return ExecutarRegraNaView(() =>
        {
            var categoria = _categoriaRepository.ObterPorId(modelo.Id);

            if (categoria is null)
            {
                Alerta("Categoria nao encontrada.");
                return RedirectToAction(nameof(Index));
            }

            categoria.Editar(modelo.Nome, modelo.Descricao);
            _categoriaRepository.Atualizar(categoria);

            Sucesso($"Categoria {categoria.Nome} atualizada com sucesso.");
            return RedirectToAction(nameof(Index));
        }, () => View(modelo));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Excluir(int id) => ExecutarRegraComRedirecionamento(() =>
    {
        _categoriaRepository.Remover(id);
        Sucesso("Categoria excluida com sucesso.");
        return RedirectToAction(nameof(Index));
    });
}
