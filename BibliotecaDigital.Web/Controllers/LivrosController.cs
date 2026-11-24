using BibliotecaDigital.Web.Exceptions;
using BibliotecaDigital.Web.Models;
using BibliotecaDigital.Web.Models.ViewModels;
using BibliotecaDigital.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BibliotecaDigital.Web.Controllers;

public class LivrosController : ControladorBase
{
    private readonly ILivroRepository _livroRepository;
    private readonly IAutorRepository _autorRepository;
    private readonly IEditoraRepository _editoraRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly ILeituraRepository _leituraRepository;
    private readonly IInteracaoRepository _interacaoRepository;
    private readonly ICirculacaoRepository _circulacaoRepository;

    public LivrosController(
        ILivroRepository livroRepository,
        IAutorRepository autorRepository,
        IEditoraRepository editoraRepository,
        ICategoriaRepository categoriaRepository,
        ILeituraRepository leituraRepository,
        IInteracaoRepository interacaoRepository,
        ICirculacaoRepository circulacaoRepository)
    {
        _livroRepository = livroRepository;
        _autorRepository = autorRepository;
        _editoraRepository = editoraRepository;
        _categoriaRepository = categoriaRepository;
        _leituraRepository = leituraRepository;
        _interacaoRepository = interacaoRepository;
        _circulacaoRepository = circulacaoRepository;
    }

    public IActionResult Index(string? termo)
    {
        ViewBag.Termo = termo;
        return View(_livroRepository.Pesquisar(termo).ToList());
    }

    public IActionResult Detalhes(int id)
    {
        var livro = _livroRepository.ObterComDetalhes(id);

        if (livro is null)
        {
            Alerta("Livro nao encontrado.");
            return RedirectToAction(nameof(Index));
        }

        var leitores = _circulacaoRepository.ObterLeitores().ToList();

        var modelo = new LivroDetalhesViewModel
        {
            Livro = livro,
            ValorReferencia = _livroRepository.ObterValorReferencia(id),
            Avaliacoes = _interacaoRepository.ObterAvaliacoesDoLivro(id).ToList(),
            Comentarios = _interacaoRepository.ObterComentariosDoLivro(id).ToList(),
            NomesDeUsuarios = leitores.ToDictionary(leitor => leitor.Id, leitor => leitor.Nome),
            Leitores = leitores.Select(leitor => new SelectListItem(leitor.Nome, leitor.Id.ToString())).ToList()
        };

        return View(modelo);
    }

    public IActionResult Criar() => View(PrepararFormulario(new LivroFormViewModel()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Criar(LivroFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(PrepararFormulario(modelo));
        }

        return ExecutarRegraNaView(() =>
        {
            if (_livroRepository.IsbnEmUso(LimparIsbn(modelo.Isbn)))
            {
                ModelState.AddModelError(nameof(modelo.Isbn), "Ja existe um livro cadastrado com este ISBN.");
                return View(PrepararFormulario(modelo));
            }

            var editora = _editoraRepository.ObterPorId(modelo.EditoraId)
                ?? throw new RegistroNaoEncontradoException("Editora", modelo.EditoraId);

            var primeiroAutor = _autorRepository.ObterPorId(modelo.AutorIds.First())
                ?? throw new RegistroNaoEncontradoException("Autor", modelo.AutorIds.First());

            var livro = new Livro(modelo.Titulo, modelo.Isbn, modelo.AnoPublicacao, modelo.Descricao, editora, primeiroAutor);

            var id = _livroRepository.Inserir(livro, modelo.ValorReferencia);
            _livroRepository.DefinirAutores(id, modelo.AutorIds);
            _livroRepository.DefinirCategorias(id, modelo.CategoriaIds);

            Sucesso($"Livro {livro.Titulo} cadastrado. Cadastre ao menos um exemplar digital para liberar emprestimos.");
            return RedirectToAction(nameof(Detalhes), new { id });
        }, () => View(PrepararFormulario(modelo)));
    }

    public IActionResult Editar(int id)
    {
        var livro = _livroRepository.ObterComDetalhes(id);

        if (livro is null)
        {
            Alerta("Livro nao encontrado.");
            return RedirectToAction(nameof(Index));
        }

        var modelo = LivroFormViewModel.DoLivro(livro, _livroRepository.ObterValorReferencia(id));
        return View(PrepararFormulario(modelo));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editar(LivroFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(PrepararFormulario(modelo));
        }

        return ExecutarRegraNaView(() =>
        {
            var livro = _livroRepository.ObterComDetalhes(modelo.Id);

            if (livro is null)
            {
                Alerta("Livro nao encontrado.");
                return RedirectToAction(nameof(Index));
            }

            if (_livroRepository.IsbnEmUso(LimparIsbn(modelo.Isbn), modelo.Id))
            {
                ModelState.AddModelError(nameof(modelo.Isbn), "Ja existe um livro cadastrado com este ISBN.");
                return View(PrepararFormulario(modelo));
            }

            var editora = _editoraRepository.ObterPorId(modelo.EditoraId)
                ?? throw new RegistroNaoEncontradoException("Editora", modelo.EditoraId);

            livro.Editar(modelo.Titulo, modelo.Isbn, modelo.AnoPublicacao, modelo.Descricao, editora);

            _livroRepository.Atualizar(livro, modelo.ValorReferencia);
            _livroRepository.DefinirAutores(livro.Id, modelo.AutorIds);
            _livroRepository.DefinirCategorias(livro.Id, modelo.CategoriaIds);

            Sucesso($"Livro {livro.Titulo} atualizado com sucesso.");
            return RedirectToAction(nameof(Detalhes), new { id = livro.Id });
        }, () => View(PrepararFormulario(modelo)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Excluir(int id) => ExecutarRegraComRedirecionamento(() =>
    {
        _livroRepository.Remover(id);
        Sucesso("Livro e respectivos exemplares digitais excluidos.");
        return RedirectToAction(nameof(Index));
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Avaliar(AvaliacaoFormViewModel modelo) => ExecutarRegra(() =>
    {
        var leitura = _leituraRepository.ObterPorUsuarioELivro(modelo.UsuarioId, modelo.LivroId);

        if (leitura is null)
        {
            throw new DominioException("Este leitor ainda nao possui registro de leitura para a obra.");
        }

        var existente = _interacaoRepository.ObterAvaliacao(modelo.UsuarioId, modelo.LivroId);

        if (existente is not null)
        {
            existente.AlterarNota(modelo.Nota);
            _interacaoRepository.SalvarAvaliacao(existente);
            Sucesso("Nota atualizada com sucesso.");
        }
        else
        {
            var avaliacao = new Avaliacao(leitura, modelo.Nota);
            _interacaoRepository.SalvarAvaliacao(avaliacao);
            Sucesso("Avaliacao registrada com sucesso.");
        }

        return RedirectToAction(nameof(Detalhes), new { id = modelo.LivroId });
    }, mensagem =>
    {
        Alerta(mensagem);
        return RedirectToAction(nameof(Detalhes), new { id = modelo.LivroId });
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Comentar(ComentarioFormViewModel modelo) => ExecutarRegra(() =>
    {
        if (!ModelState.IsValid)
        {
            throw new DominioException("Escreva um comentario de ate 500 caracteres e selecione o leitor.");
        }

        var leitura = _leituraRepository.ObterPorUsuarioELivro(modelo.UsuarioId, modelo.LivroId)
            ?? throw new DominioException("Este leitor ainda nao possui registro de leitura para a obra.");

        if (modelo.ComentarioPaiId is int paiId)
        {
            var pai = _interacaoRepository.ObterComentario(paiId)
                ?? throw new DominioException("O comentario respondido nao existe mais.");

            var resposta = pai.Responder(leitura, modelo.Texto);
            _interacaoRepository.InserirComentario(resposta);
        }
        else
        {
            var comentario = new Comentario(leitura, modelo.Texto);
            _interacaoRepository.InserirComentario(comentario);
        }

        Sucesso("Comentario publicado.");
        return RedirectToAction(nameof(Detalhes), new { id = modelo.LivroId });
    }, mensagem =>
    {
        Alerta(mensagem);
        return RedirectToAction(nameof(Detalhes), new { id = modelo.LivroId });
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExcluirComentario(int id, int livroId, int usuarioId) => ExecutarRegra(() =>
    {
        var comentario = _interacaoRepository.ObterComentario(id)
            ?? throw new DominioException("Comentario nao encontrado.");

        comentario.Excluir(usuarioId);
        _interacaoRepository.AtualizarComentario(comentario);

        Sucesso("Comentario removido.");
        return RedirectToAction(nameof(Detalhes), new { id = livroId });
    }, mensagem =>
    {
        Alerta(mensagem);
        return RedirectToAction(nameof(Detalhes), new { id = livroId });
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AlternarFavorito(int livroId, int usuarioId) => ExecutarRegra(() =>
    {
        if (usuarioId <= 0)
        {
            throw new DominioException("Selecione o leitor antes de favoritar a obra.");
        }

        _interacaoRepository.AlternarFavorito(usuarioId, livroId);
        Sucesso("Lista de favoritos atualizada.");
        return RedirectToAction(nameof(Detalhes), new { id = livroId });
    }, mensagem =>
    {
        Alerta(mensagem);
        return RedirectToAction(nameof(Detalhes), new { id = livroId });
    });

    private LivroFormViewModel PrepararFormulario(LivroFormViewModel modelo)
    {
        modelo.Editoras = _editoraRepository.ObterTodos()
            .Select(editora => new SelectListItem(editora.Nome, editora.Id.ToString()))
            .ToList();

        modelo.AutoresDisponiveis = _autorRepository.ObterTodos()
            .Select(autor => new SelectListItem($"{autor.Nome} - {autor.Nacionalidade}", autor.Id.ToString()))
            .ToList();

        modelo.CategoriasDisponiveis = _categoriaRepository.ObterTodos()
            .Select(categoria => new SelectListItem(categoria.Nome, categoria.Id.ToString()))
            .ToList();

        return modelo;
    }

    private static string LimparIsbn(string isbn)
        => (isbn ?? string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
}
