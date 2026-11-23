# Guia para implementar os módulos 1, 2 e 4

O módulo 3 (empréstimos e devoluções) já está no repositório e serve de modelo. Este guia descreve o padrão a seguir para que os quatro módulos fiquem coerentes entre si.

Leia primeiro `Models/Emprestimo.cs`, `Repositories/Dapper/EmprestimoRepository.cs` e `Controllers/EmprestimosController.cs` — eles mostram o padrão completo de ponta a ponta.

---

## O que já existe (infraestrutura compartilhada)

Não altere estes arquivos sem combinar com o grupo:

| Arquivo | Papel |
| --- | --- |
| `Program.cs` | Injeção de dependência e pipeline HTTP. Cada módulo **acrescenta** suas linhas de `AddScoped`. |
| `Infrastructure/IFabricaConexao.cs` + `FabricaConexaoSqlite.cs` | Abre a conexão SQLite com `Foreign Keys = ON`. |
| `Infrastructure/InicializadorBanco.cs` | Executa `Database/script.sql` quando o banco ainda não existe. |
| `Infrastructure/DecimalModelBinder.cs` | Faz campos decimais aceitarem vírgula e ponto. |
| `Repositories/Interfaces/IRepository.cs` | Contrato genérico `IRepository<T> where T : class`. |
| `Repositories/Dapper/RepositorioDapperBase.cs` | Abre conexão e traduz erro de banco em `DominioException`. |
| `Controllers/ControladorBase.cs` | `Sucesso()`, `Alerta()` e os helpers `ExecutarRegra*`. |
| `Exceptions/DominioException.cs` | Classe base de toda exceção de negócio. |
| `Views/Shared/_Layout.cshtml` | Layout e menu. Cada módulo **acrescenta** seu link. |
| `wwwroot/css/site.css` | Folha de estilo. Reaproveite as classes existentes. |

---

## Padrão a seguir, camada por camada

### 1. Entidade de domínio (`Models/`)

- Campos privados; propriedades com `private set`.
- O construtor valida tudo e lança `DominioException` — nunca deixe nascer um objeto inválido.
- Regras de negócio viram **métodos** da entidade, não `if` no controller.
- Adicione um método estático `Reconstruir(...)` para o repositório reidratar a entidade vinda do banco.

```csharp
public sealed class Autor
{
    private string _nome = string.Empty;

    public Autor(string nome, string nacionalidade)
    {
        Nome = nome;
        Nacionalidade = nacionalidade;
    }

    public int Id { get; private set; }

    public string Nome
    {
        get => _nome;
        private set
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim().Length < 3)
            {
                throw new DominioException("O nome do autor deve ter ao menos tres caracteres.");
            }

            _nome = value.Trim();
        }
    }

    public void DefinirId(int id) => Id = id;

    public static Autor Reconstruir(int id, string nome, string nacionalidade)
        => new(nome, nacionalidade) { Id = id };
}
```

### 2. Exceções (`Exceptions/`)

Herde sempre de `DominioException`. A mensagem é exibida direto para o usuário, então escreva em português e sem jargão.

```csharp
public class IsbnDuplicadoException : DominioException
{
    public IsbnDuplicadoException(string isbn)
        : base($"Ja existe um livro cadastrado com o ISBN {isbn}.")
    {
    }
}
```

### 3. Repositório (`Repositories/Interfaces/` + `Repositories/Dapper/`)

- A interface herda de `IRepository<T>` e acrescenta o que o módulo precisar.
- A implementação herda de `RepositorioDapperBase` e envolve tudo em `Executar(...)`.
- **Todo SQL é parametrizado.** Nunca concatene valor de usuário na query.
- Use uma **classe** de linha (`LinhaAutor`) com propriedades `get; set;` para o Dapper materializar, e mapeie para a entidade com `Reconstruir(...)`. Não use `record` posicional: o Dapper exige que os tipos do construtor batam exatamente com os do SQLite e falha com `Int64`/`Double`.

```csharp
public sealed class AutorRepository : RepositorioDapperBase, IAutorRepository
{
    private sealed class LinhaAutor
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Nacionalidade { get; set; } = string.Empty;
    }

    public AutorRepository(IFabricaConexao fabricaConexao) : base(fabricaConexao)
    {
    }

    public IEnumerable<Autor> ObterTodos() => Executar(conexao =>
    {
        const string sql = "SELECT Id, Nome, Nacionalidade FROM Autor ORDER BY Nome;";
        return conexao.Query<LinhaAutor>(sql).Select(Mapear).ToList();
    }, "listar os autores");

    private static Autor Mapear(LinhaAutor linha)
        => Autor.Reconstruir(linha.Id, linha.Nome, linha.Nacionalidade);
}
```

### 4. ViewModel (`Models/ViewModels/`)

As entidades não têm construtor sem parâmetros, então **formulário nenhum faz bind direto nelas**. Crie um ViewModel com `get; set;` e `DataAnnotations`.

### 5. Controller (`Controllers/`)

- Herde de `ControladorBase`.
- Receba só interfaces pelo construtor.
- Valide `ModelState`, depois chame a entidade e o repositório.
- Envolva a regra em `ExecutarRegraNaView` (volta ao formulário) ou `ExecutarRegra` (redireciona).
- `[HttpPost]` + `[ValidateAntiForgeryToken]` em tudo que escreve.
- **Zero SQL aqui.**

### 6. View (`Views/<Entidade>/`)

- As pastas já existem, vazias.
- Siga `Views/Emprestimos/Index.cshtml` e `Views/Reservas/Criar.cshtml`.
- Use `cartao`, `cabecalho-pagina`, `formulario`, `campo`, `botao`, `etiqueta`, `tabela-rolavel` — já estilizadas.
- Formulários terminam com `@section Scripts { <partial name="_ValidationScriptsPartial" /> }`.

### 7. Registre no `Program.cs` e no menu

```csharp
builder.Services.AddScoped<IAutorRepository, AutorRepository>();
```

```html
<a class="@(controller == "Autores" ? "ativo" : null)" asp-controller="Autores" asp-action="Index">Autores</a>
```

---

## A fronteira com o módulo 3

Hoje o módulo 3 lê leitor e exemplar por conta própria, porque `Leitor` e `ExemplarDigital` ainda não existem:

| Arquivo | Papel |
| --- | --- |
| `Models/LeitorEmCirculacao.cs` | O que a circulação precisa saber do leitor: nome, bloqueio, limite, empréstimos ativos e multas em aberto. |
| `Models/ExemplarEmCirculacao.cs` | O que a circulação precisa saber do exemplar: título, valor de referência e licenças. |
| `Repositories/Interfaces/ICirculacaoRepository.cs` | Contrato que entrega esses dois objetos. |
| `Repositories/Dapper/CirculacaoRepository.cs` | Implementação que consulta `Usuario`, `Livro` e `ExemplarDigital` direto. |

**Quando os módulos 1 e 2 ficarem prontos**, só `CirculacaoRepository` muda: em vez de montar a consulta na mão, ele passa a receber `IUsuarioRepository` e `IExemplarRepository` e converter `Leitor` → `LeitorEmCirculacao` e `ExemplarDigital` → `ExemplarEmCirculacao`. Nenhum controller, entidade ou view do módulo 3 precisa ser alterado.

Combine com o Otávio antes de mexer nesses quatro arquivos.

---

## Banco de dados

`Database/script.sql` hoje contém:

- as cinco tabelas do módulo 3 (`Emprestimo`, `Reserva`, `Renovacao`, `Devolucao`, `Multa`), completas;
- `Usuario`, `Livro` e `ExemplarDigital` em **versão reduzida**, só com as colunas que a circulação consulta.

Cada módulo expande as suas tabelas no mesmo arquivo:

| Tabela | Dono | Colunas a acrescentar |
| --- | --- | --- |
| `Usuario` | Módulo 1 | `Documento`, `Senha`, `DataCadastro`, `DataAtualizacao`, endereço (`Logradouro`, `Numero`, `Cidade`, `Estado`, `Cep`), `Departamento` |
| `Livro` | Módulo 2 | `AnoPublicacao`, `Sinopse`, `Idioma`, `EditoraId` |
| `ExemplarDigital` | Módulo 2 | `Formato`, `Arquivo`, `TamanhoMb` |
| `Autor`, `Editora`, `Categoria`, `LivroAutor`, `LivroCategoria` | Módulo 2 | tabelas novas |
| `Leitura`, `Avaliacao`, `Comentario`, `Favorito` | Módulo 4 | tabelas novas |

Regras do script:

- mantenha a ordem: `DROP` na ordem inversa das dependências, depois `CREATE`, depois `INSERT`;
- **não use `DECIMAL(10,2)`** para dinheiro — use `REAL`, senão o Dapper quebra com tipo misto;
- datas são `TEXT` no formato `AAAA-MM-DD HH:MM:SS`;
- booleanos são `INTEGER` 0/1;
- de 5 a 10 registros fictícios por tabela, como pede o enunciado;
- não quebre os `INSERT` que já existem: os IDs de `Usuario`, `Livro` e `ExemplarDigital` são referenciados pelos empréstimos e reservas.

Depois de editar o script, apague `BibliotecaDigital.Web/biblioteca.db` e rode de novo para recriar a base.

---

## Antes de commitar

```bash
dotnet build
dotnet run --project BibliotecaDigital.Web
```

- O build precisa terminar com **0 erros e 0 avisos**.
- Abra as suas telas e teste o CRUD inteiro.
- Provoque de propósito os erros de regra e confirme que aparece o alerta amarelo, **nunca a tela de erro 500**.
- Commits em português, no imperativo, descrevendo o que o módulo passou a fazer.
- Cada integrante commita o próprio módulo com o próprio usuário do git — é assim que a colaboração aparece no histórico. Confira antes com:

```bash
git config user.name
git config user.email
```

---

## Checklist por módulo

### Módulo 1 — Gestão de usuários

- [ ] `Models/Endereco.cs` — composição dentro de `Usuario`
- [ ] `Models/Usuario.cs` — classe **abstrata**
- [ ] `Models/Leitor.cs` e `Models/Administrador.cs` — herança com `override`
- [ ] `Models/Enums/PerfilUsuario.cs`
- [ ] `IUsuarioRepository` + `UsuarioRepository`
- [ ] `UsuarioFormViewModel`
- [ ] `UsuariosController` com CRUD, bloquear e desbloquear
- [ ] Views em `Views/Usuarios/`
- [ ] Expandir a tabela `Usuario` no `script.sql`

> Este módulo carrega **herança e polimorfismo**, que valem 1,5 ponto na matriz de avaliação. Garanta pelo menos um método abstrato sobrescrito nas duas subclasses (por exemplo `ObterPermissoes()`).

### Módulo 2 — Catálogo e acervo digital

- [ ] `Models/Autor.cs`, `Editora.cs`, `Categoria.cs`
- [ ] `Models/Livro.cs` — composição com `List<ExemplarDigital>`, agregação com autores e categorias
- [ ] `Models/ExemplarDigital.cs`
- [ ] `Models/Enums/FormatoExemplar.cs`
- [ ] Repositórios e controllers de cada entidade
- [ ] Views em `Views/Livros/`, `Views/Autores/`, `Views/Editoras/`, `Views/Categorias/`, `Views/Exemplares/`
- [ ] Tabelas novas e colunas extras no `script.sql`

### Módulo 4 — Leitura e interação digital

- [ ] `Models/ProgressoLeitura.cs` — composição dentro de `Leitura`
- [ ] `Models/Leitura.cs` e `Models/HistoricoLeitura.cs`
- [ ] `Models/Avaliacao.cs`, `Comentario.cs`, `Favorito.cs`
- [ ] `Models/Enums/StatusLeitura.cs`
- [ ] Repositórios, controller e views em `Views/Leituras/`
- [ ] Tabelas novas no `script.sql`
