using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BibliotecaDigital.Web.Exceptions;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Models.ViewModels;
using BibliotecaDigital.Web.Repositories.Interfaces;

namespace BibliotecaDigital.Web.Controllers
{
    public class UsuariosController : ControladorBase
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuariosController(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<IActionResult> Index()
        {
            var usuarios = await _usuarioRepository.ListarTodosAsync();
            return View(usuarios);
        }

        public IActionResult Criar()
        {
            return View(new UsuarioFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(UsuarioFormViewModel model)
        {
            try
            {
                var endereco = new Endereco(model.Rua, model.Numero, model.Cidade, model.Estado, model.CEP);

                Usuario novoUsuario = model.EhAdministrador
                    ? new Administrador(model.Nome, model.Email, model.Senha, endereco, model.Departamento ?? "Geral")
                    : new Leitor(model.Nome, model.Email, model.Senha, endereco);

                await _usuarioRepository.CadastrarAsync(novoUsuario);
                TempData["Sucesso"] = $"Usuário {novoUsuario.Nome} cadastrado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (DominioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Bloquear(int id)
        {
            try
            {
                var usuario = await _usuarioRepository.BuscarPorIdAsync(id)
                    ?? throw new RegistroNaoEncontradoException("Usuário", id);

                usuario.Bloquear();
                await _usuarioRepository.AtualizarStatusBloqueioAsync(usuario);
                TempData["Sucesso"] = $"Usuário {usuario.Nome} bloqueado.";
            }
            catch (DominioException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desbloquear(int id)
        {
            try
            {
                var usuario = await _usuarioRepository.BuscarPorIdAsync(id)
                    ?? throw new RegistroNaoEncontradoException("Usuário", id);

                usuario.Desbloquear();
                await _usuarioRepository.AtualizarStatusBloqueioAsync(usuario);
                TempData["Sucesso"] = $"Usuário {usuario.Nome} desbloqueado.";
            }
            catch (DominioException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _usuarioRepository.BuscarPorIdAsync(id);

            if (usuario == null)
            {
                TempData["Erro"] = "Usuário não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            return View(UsuarioFormViewModel.DoUsuario(usuario));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(UsuarioFormViewModel model)
        {
            try
            {
                var usuario = await _usuarioRepository.BuscarPorIdAsync(model.Id)
                    ?? throw new RegistroNaoEncontradoException("Usuário", model.Id);

                var endereco = usuario.EnderecoResidencial;
                endereco.AtualizarEndereco(model.Rua, model.Numero, model.Cidade, model.Estado, model.CEP);

                usuario.AtualizarDados(model.Nome, model.Email, endereco);
                await _usuarioRepository.AtualizarAsync(usuario);

                TempData["Sucesso"] = $"Usuário {usuario.Nome} atualizado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (DominioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir(int id)
        {
            try
            {
                var usuario = await _usuarioRepository.BuscarPorIdAsync(id)
                    ?? throw new RegistroNaoEncontradoException("Usuário", id);

                if (await _usuarioRepository.PossuiCirculacaoAsync(id))
                    throw new DominioException(
                        $"{usuario.Nome} possui empréstimos ou reservas registrados e não pode ser excluído.");

                await _usuarioRepository.ExcluirAsync(id);
                TempData["Sucesso"] = $"Usuário {usuario.Nome} excluído.";
            }
            catch (DominioException ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}