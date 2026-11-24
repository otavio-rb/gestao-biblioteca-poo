using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Models.ViewModels;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaDigital.Web.Controllers;

public class EditorasController : ControladorBase
{
    private readonly IEditoraRepository _editoraRepository;

    public EditorasController(IEditoraRepository editoraRepository)
    {
        _editoraRepository = editoraRepository;
    }

    public IActionResult Index() => View(_editoraRepository.ObterTodos().ToList());

    public IActionResult Criar() => View(new EditoraFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Criar(EditoraFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        return ExecutarRegraNaView(() =>
        {
            var editora = new Editora(modelo.Nome, modelo.Endereco);
            _editoraRepository.Inserir(editora);

            Sucesso($"Editora {editora.Nome} cadastrada com sucesso.");
            return RedirectToAction(nameof(Index));
        }, () => View(modelo));
    }

    public IActionResult Editar(int id)
    {
        var editora = _editoraRepository.ObterPorId(id);

        if (editora is null)
        {
            Alerta("Editora nao encontrada.");
            return RedirectToAction(nameof(Index));
        }

        return View(EditoraFormViewModel.DaEditora(editora));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(EditoraFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        return ExecutarRegraNaView(() =>
        {
            var editora = _editoraRepository.ObterPorId(modelo.Id);

            if (editora is null)
            {
                Alerta("Editora nao encontrada.");
                return RedirectToAction(nameof(Index));
            }

            editora.Editar(modelo.Nome, modelo.Endereco);
            _editoraRepository.Atualizar(editora);

            Sucesso($"Editora {editora.Nome} atualizada com sucesso.");
            return RedirectToAction(nameof(Index));
        }, () => View(modelo));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Excluir(int id) => ExecutarRegraComRedirecionamento(() =>
    {
        if (_editoraRepository.PossuiLivrosVinculados(id))
        {
            Alerta("Esta editora possui obras publicadas no catalogo e nao pode ser excluida.");
            return RedirectToAction(nameof(Index));
        }

        _editoraRepository.Remover(id);
        Sucesso("Editora excluida com sucesso.");
        return RedirectToAction(nameof(Index));
    });
}
