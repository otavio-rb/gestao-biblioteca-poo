using BibliotecaDigital.Web.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaDigital.Web.Controllers;

public abstract class ControladorBase : Controller
{
    protected const string ChaveSucesso = "MensagemSucesso";
    protected const string ChaveAlerta = "MensagemAlerta";

    protected void Sucesso(string mensagem) => TempData[ChaveSucesso] = mensagem;

    protected void Alerta(string mensagem) => TempData[ChaveAlerta] = mensagem;

    protected IActionResult ExecutarRegra(Func<IActionResult> acao, Func<string, IActionResult> aoFalhar)
    {
        try
        {
            return acao();
        }
        catch (DominioException excecao)
        {
            return aoFalhar(excecao.Message);
        }
    }

    protected IActionResult ExecutarRegraComRedirecionamento(Func<IActionResult> acao, string actionDeFallback = "Index")
    {
        return ExecutarRegra(acao, mensagem =>
        {
            Alerta(mensagem);
            return RedirectToAction(actionDeFallback);
        });
    }

    protected IActionResult ExecutarRegraNaView(Func<IActionResult> acao, Func<IActionResult> aoFalhar)
    {
        return ExecutarRegra(acao, mensagem =>
        {
            ModelState.AddModelError(string.Empty, mensagem);
            return aoFalhar();
        });
    }
}
