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
    private readonly IAvaliacaoRepository _avaliacaoRepository;
    private readonly IComentarioRepository _comentarioRepository;
    private readonly IFavoritoRepository _favoritoRepository;

    public LeiturasController(
        ILeituraRepository leituraRepository,
        IAvaliacaoRepository avaliacaoRepository,
        IComentarioRepository comentarioRepository,
        IFavoritoRepository favoritoRepository)
    {
        _leituraRepository = leituraRepository;
        _avaliacaoRepository = avaliacaoRepository;
        _comentarioRepository = comentarioRepository;
        _favoritoRepository = favoritoRepository;
    }

    // Lista todas as leituras ou, se usuarioId vier preenchido, so as de um leitor.
    public IActionResult Index(int? usuarioId)
    {
        var modelo = new LeiturasIndexViewModel
        {
            UsuarioId = usuarioId,
            Leitores = CriarLista(_leituraRepository.ObterLeitores(), usuarioId)
        };

        if (usuarioId.HasValue)
        {
            modelo.Historico = _leituraRepository.ObterHistorico(usuarioId.Value);
            modelo.Leituras = modelo.Historico.GetLeituras().ToList();
            modelo.Favoritos = _favoritoRepository.ObterPorUsuario(usuarioId.Value).ToList();
        }
        else
        {
            modelo.Leituras = _leituraRepository.ObterTodos().ToList();
        }

        return View(modelo);
    }

    public IActionResult Detalhes(int id)
    {
        var leitura = _leituraRepository.ObterPorId(id);

        if (leitura is null)
        {
            Alerta("Leitura nao encontrada.");
            return RedirectToAction(nameof(Index));
        }

        var modelo = new LeituraDetalhesViewModel
        {
            Leitura = leitura,
            Avaliacao = _avaliacaoRepository.ObterPorUsuarioELivro(leitura.UsuarioId, leitura.LivroId),
            MediaLivro = _avaliacaoRepository.ObterMediaDoLivro(leitura.LivroId),
            Comentarios = _comentarioRepository.ObterPorLivro(leitura.LivroId).ToList(),
            EhFavorito = _favoritoRepository.Existe(leitura.UsuarioId, leitura.LivroId)
        };

        return View(modelo);
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
            var leitura = new Leitura(modelo.UsuarioId, modelo.LivroId, modelo.TotalPaginas);

            // O historico recusa o mesmo livro duas vezes para o mesmo leitor.
            var historico = _leituraRepository.ObterHistorico(modelo.UsuarioId);
            historico.Registrar(leitura);

            _leituraRepository.Inserir(leitura);

            Sucesso("Leitura cadastrada. Clique em Iniciar quando comecar a ler.");
            return RedirectToAction(nameof(Detalhes), new { id = leitura.Id });
        }, () => View(PrepararFormulario(modelo)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Iniciar(int id) => AlterarLeitura(id, leitura =>
    {
        leitura.Iniciar();
        _leituraRepository.Atualizar(leitura);
        return "Leitura iniciada. Boa leitura!";
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Pausar(int id) => AlterarLeitura(id, leitura =>
    {
        leitura.Pausar();
        _leituraRepository.Atualizar(leitura);
        return "Leitura pausada.";
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Retomar(int id) => AlterarLeitura(id, leitura =>
    {
        leitura.Retomar();
        _leituraRepository.Atualizar(leitura);
        return "Leitura retomada.";
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Concluir(int id) => AlterarLeitura(id, leitura =>
    {
        leitura.Concluir();
        _leituraRepository.Atualizar(leitura);
        return "Leitura concluida! Agora voce ja pode avaliar o livro.";
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RegistrarPagina(int id, int? pagina) => AlterarLeitura(id, leitura =>
    {
        if (pagina is null)
        {
            throw new DominioException("Informe a pagina em que voce parou.");
        }

        leitura.RegistrarPagina(pagina.Value);
        _leituraRepository.Atualizar(leitura);

        return leitura.EstaConcluida()
            ? "Voce chegou na ultima pagina: leitura concluida!"
            : $"Progresso salvo: {leitura.GetPercentual():0}% lido.";
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Avaliar(int id, int? nota) => AlterarLeitura(id, leitura =>
    {
        if (nota is null)
        {
            throw new DominioException("Escolha uma nota de 1 a 5.");
        }

        // Regra: uma avaliacao por usuario/livro. Se ja existe, a nota e alterada.
        var avaliacao = _avaliacaoRepository.ObterPorUsuarioELivro(leitura.UsuarioId, leitura.LivroId);

        if (avaliacao is null)
        {
            avaliacao = new Avaliacao(leitura, nota.Value);
            _avaliacaoRepository.Inserir(avaliacao);
            return "Avaliacao registrada.";
        }

        avaliacao.AlterarNota(nota.Value);
        _avaliacaoRepository.Atualizar(avaliacao);
        return "Nota alterada.";
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Comentar(int id, string? texto, int? comentarioPaiId) => AlterarLeitura(id, leitura =>
    {
        Comentario comentario;

        if (comentarioPaiId is null)
        {
            comentario = new Comentario(leitura, texto ?? string.Empty);
        }
        else
        {
            var pai = _comentarioRepository.ObterPorId(comentarioPaiId.Value)
                ?? throw new RegistroNaoEncontradoException("Comentario", comentarioPaiId.Value);

            comentario = pai.Responder(leitura, texto ?? string.Empty);
        }

        _comentarioRepository.Inserir(comentario);
        return comentarioPaiId is null ? "Comentario publicado." : "Resposta publicada.";
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditarComentario(int id, int comentarioId, string? texto) => AlterarLeitura(id, leitura =>
    {
        var comentario = _comentarioRepository.ObterPorId(comentarioId)
            ?? throw new RegistroNaoEncontradoException("Comentario", comentarioId);

        // O leitor desta leitura e quem esta agindo; a classe recusa se nao for o autor.
        comentario.Editar(leitura.UsuarioId, texto ?? string.Empty);
        _comentarioRepository.Atualizar(comentario);
        return "Comentario editado.";
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExcluirComentario(int id, int comentarioId) => AlterarLeitura(id, leitura =>
    {
        var comentario = _comentarioRepository.ObterPorId(comentarioId)
            ?? throw new RegistroNaoEncontradoException("Comentario", comentarioId);

        comentario.Excluir(leitura.UsuarioId);
        _comentarioRepository.Atualizar(comentario);
        return "Comentario excluido.";
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Favoritar(int id) => AlterarLeitura(id, leitura =>
    {
        // Regra: adicionar ou remover a qualquer momento.
        if (_favoritoRepository.Existe(leitura.UsuarioId, leitura.LivroId))
        {
            _favoritoRepository.Remover(leitura.UsuarioId, leitura.LivroId);
            return "Livro removido dos favoritos.";
        }

        _favoritoRepository.Adicionar(new Favorito(leitura.UsuarioId, leitura.LivroId));
        return "Livro adicionado aos favoritos.";
    });

    // Busca a leitura, executa a acao e volta para a tela de detalhes.
    // Se alguma regra for quebrada, a mensagem aparece no alerta amarelo.
    private IActionResult AlterarLeitura(int id, Func<Leitura, string> acao)
    {
        return ExecutarRegra(() =>
        {
            var leitura = _leituraRepository.ObterPorId(id)
                ?? throw new RegistroNaoEncontradoException("Leitura", id);

            Sucesso(acao(leitura));
            return RedirectToAction(nameof(Detalhes), new { id });
        }, mensagem =>
        {
            Alerta(mensagem);
            return RedirectToAction(nameof(Detalhes), new { id });
        });
    }

    private LeituraFormViewModel PrepararFormulario(LeituraFormViewModel modelo)
    {
        modelo.Leitores = CriarLista(_leituraRepository.ObterLeitores(), modelo.UsuarioId);
        modelo.Livros = CriarLista(_leituraRepository.ObterLivros(), modelo.LivroId);
        return modelo;
    }

    private static List<SelectListItem> CriarLista(Dictionary<int, string> opcoes, int? selecionado)
    {
        var lista = new List<SelectListItem>();

        foreach (var opcao in opcoes)
        {
            lista.Add(new SelectListItem(opcao.Value, opcao.Key.ToString(), opcao.Key == selecionado));
        }

        return lista;
    }
}
