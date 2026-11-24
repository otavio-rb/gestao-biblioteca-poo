PRAGMA foreign_keys = ON;

DROP TABLE IF EXISTS Favorito;
DROP TABLE IF EXISTS Comentario;
DROP TABLE IF EXISTS Avaliacao;
DROP TABLE IF EXISTS Leitura;
DROP TABLE IF EXISTS Multa;
DROP TABLE IF EXISTS Devolucao;
DROP TABLE IF EXISTS Renovacao;
DROP TABLE IF EXISTS Emprestimo;
DROP TABLE IF EXISTS Reserva;
DROP TABLE IF EXISTS ExemplarDigital;
DROP TABLE IF EXISTS LivroCategoria;
DROP TABLE IF EXISTS LivroAutor;
DROP TABLE IF EXISTS Livro;
DROP TABLE IF EXISTS Categoria;
DROP TABLE IF EXISTS Editora;
DROP TABLE IF EXISTS Autor;
DROP TABLE IF EXISTS Usuario;
DROP TABLE IF EXISTS Endereco;

CREATE TABLE Endereco (
    Id     INTEGER PRIMARY KEY AUTOINCREMENT,
    Rua    TEXT    NOT NULL,
    Numero TEXT    NOT NULL,
    Cidade TEXT    NOT NULL,
    Estado TEXT    NOT NULL,
    CEP    TEXT    NOT NULL
);

CREATE TABLE Usuario (
    Id                INTEGER PRIMARY KEY AUTOINCREMENT,
    Tipo              TEXT    NOT NULL CHECK (Tipo IN ('LEITOR', 'ADMINISTRADOR')),
    Nome              TEXT    NOT NULL,
    Email             TEXT    NOT NULL UNIQUE,
    EstaBloqueado     INTEGER NOT NULL DEFAULT 0,
    LimiteEmprestimos INTEGER NULL,
    Senha             TEXT    NOT NULL DEFAULT '123456',
    Departamento      TEXT    NULL,
    EnderecoId        INTEGER NULL,
    CONSTRAINT FK_Usuario_Endereco FOREIGN KEY (EnderecoId) REFERENCES Endereco (Id)
);

CREATE TABLE Autor (
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome          TEXT NOT NULL,
    Nacionalidade TEXT NOT NULL
);

CREATE TABLE Editora (
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome     TEXT NOT NULL,
    Endereco TEXT NOT NULL
);

CREATE TABLE Categoria (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome      TEXT NOT NULL,
    Descricao TEXT NOT NULL DEFAULT ''
);

CREATE TABLE Livro (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Titulo          TEXT    NOT NULL,
    Isbn            TEXT    NOT NULL UNIQUE,
    AnoPublicacao   INTEGER NOT NULL,
    Descricao       TEXT    NOT NULL DEFAULT '',
    EditoraId       INTEGER NOT NULL,
    ValorReferencia REAL    NOT NULL,
    CONSTRAINT FK_Livro_Editora FOREIGN KEY (EditoraId) REFERENCES Editora (Id)
);

CREATE TABLE LivroAutor (
    LivroId INTEGER NOT NULL,
    AutorId INTEGER NOT NULL,
    PRIMARY KEY (LivroId, AutorId),
    CONSTRAINT FK_LivroAutor_Livro FOREIGN KEY (LivroId) REFERENCES Livro (Id) ON DELETE CASCADE,
    CONSTRAINT FK_LivroAutor_Autor FOREIGN KEY (AutorId) REFERENCES Autor (Id) ON DELETE CASCADE
);

CREATE TABLE LivroCategoria (
    LivroId     INTEGER NOT NULL,
    CategoriaId INTEGER NOT NULL,
    PRIMARY KEY (LivroId, CategoriaId),
    CONSTRAINT FK_LivroCategoria_Livro     FOREIGN KEY (LivroId)     REFERENCES Livro (Id)     ON DELETE CASCADE,
    CONSTRAINT FK_LivroCategoria_Categoria FOREIGN KEY (CategoriaId) REFERENCES Categoria (Id) ON DELETE CASCADE
);

CREATE TABLE ExemplarDigital (
    Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
    LivroId             INTEGER NOT NULL,
    Formato             TEXT    NOT NULL,
    Arquivo             TEXT    NOT NULL,
    LicencasTotais      INTEGER NOT NULL,
    LicencasDisponiveis INTEGER NOT NULL,
    CONSTRAINT FK_Exemplar_Livro FOREIGN KEY (LivroId) REFERENCES Livro (Id) ON DELETE CASCADE,
    CONSTRAINT CK_Exemplar_Licencas CHECK (LicencasDisponiveis BETWEEN 0 AND LicencasTotais)
);

CREATE TABLE Reserva (
    Id                 INTEGER PRIMARY KEY AUTOINCREMENT,
    LeitorId           INTEGER NOT NULL,
    ExemplarId         INTEGER NOT NULL,
    DataReserva        TEXT    NOT NULL,
    DataLimiteRetirada TEXT    NOT NULL,
    PosicaoFila        INTEGER NOT NULL,
    Status             INTEGER NOT NULL,
    CONSTRAINT FK_Reserva_Leitor   FOREIGN KEY (LeitorId)   REFERENCES Usuario (Id),
    CONSTRAINT FK_Reserva_Exemplar FOREIGN KEY (ExemplarId) REFERENCES ExemplarDigital (Id) ON DELETE CASCADE
);

CREATE TABLE Emprestimo (
    Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
    LeitorId            INTEGER NOT NULL,
    ExemplarId          INTEGER NOT NULL,
    ReservaId           INTEGER NULL,
    ValorReferenciaObra REAL    NOT NULL,
    DataRetirada        TEXT    NOT NULL,
    DataPrevista        TEXT    NOT NULL,
    DataDevolucao       TEXT    NULL,
    Status              INTEGER NOT NULL,
    CONSTRAINT FK_Emprestimo_Leitor   FOREIGN KEY (LeitorId)   REFERENCES Usuario (Id),
    CONSTRAINT FK_Emprestimo_Exemplar FOREIGN KEY (ExemplarId) REFERENCES ExemplarDigital (Id),
    CONSTRAINT FK_Emprestimo_Reserva  FOREIGN KEY (ReservaId)  REFERENCES Reserva (Id)
);

CREATE TABLE Renovacao (
    Id               INTEGER PRIMARY KEY AUTOINCREMENT,
    EmprestimoId     INTEGER NOT NULL,
    DataRenovacao    TEXT    NOT NULL,
    NovaDataPrevista TEXT    NOT NULL,
    Sequencia        INTEGER NOT NULL,
    CONSTRAINT FK_Renovacao_Emprestimo FOREIGN KEY (EmprestimoId) REFERENCES Emprestimo (Id) ON DELETE CASCADE,
    CONSTRAINT CK_Renovacao_Sequencia CHECK (Sequencia BETWEEN 1 AND 2)
);

CREATE TABLE Devolucao (
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    EmprestimoId  INTEGER NOT NULL UNIQUE,
    DataDevolucao TEXT    NOT NULL,
    HouveAtraso   INTEGER NOT NULL,
    CONSTRAINT FK_Devolucao_Emprestimo FOREIGN KEY (EmprestimoId) REFERENCES Emprestimo (Id) ON DELETE CASCADE
);

CREATE TABLE Multa (
    Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
    EmprestimoId        INTEGER NOT NULL UNIQUE,
    ValorDiario         REAL    NOT NULL,
    DiasAtraso          INTEGER NOT NULL,
    ValorReferenciaObra REAL    NOT NULL,
    ValorTotal          REAL    NOT NULL,
    Paga                INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT FK_Multa_Emprestimo FOREIGN KEY (EmprestimoId) REFERENCES Emprestimo (Id) ON DELETE CASCADE,
    CONSTRAINT CK_Multa_Teto CHECK (ValorTotal <= ValorReferenciaObra)
);

CREATE TABLE Leitura (
    Id                INTEGER PRIMARY KEY AUTOINCREMENT,
    UsuarioId         INTEGER NOT NULL,
    LivroId           INTEGER NOT NULL,
    Status            INTEGER NOT NULL,
    PaginaAtual       INTEGER NOT NULL DEFAULT 0,
    TotalPaginas      INTEGER NOT NULL,
    UltimaAtualizacao TEXT    NOT NULL,
    DataInicio        TEXT    NULL,
    DataConclusao     TEXT    NULL,
    CONSTRAINT UQ_Leitura UNIQUE (UsuarioId, LivroId),
    CONSTRAINT FK_Leitura_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Leitura_Livro   FOREIGN KEY (LivroId)   REFERENCES Livro (Id)   ON DELETE CASCADE,
    CONSTRAINT CK_Leitura_Pagina  CHECK (PaginaAtual BETWEEN 0 AND TotalPaginas)
);

CREATE TABLE Avaliacao (
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    UsuarioId     INTEGER NOT NULL,
    LivroId       INTEGER NOT NULL,
    Nota          INTEGER NOT NULL CHECK (Nota BETWEEN 1 AND 5),
    DataAvaliacao TEXT    NOT NULL,
    CONSTRAINT UQ_Avaliacao UNIQUE (UsuarioId, LivroId),
    CONSTRAINT FK_Avaliacao_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Avaliacao_Livro   FOREIGN KEY (LivroId)   REFERENCES Livro (Id)   ON DELETE CASCADE
);

CREATE TABLE Comentario (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    UsuarioId       INTEGER NOT NULL,
    LivroId         INTEGER NOT NULL,
    ComentarioPaiId INTEGER NULL,
    Texto           TEXT    NOT NULL,
    DataCriacao     TEXT    NOT NULL,
    Editado         INTEGER NOT NULL DEFAULT 0,
    Excluido        INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT FK_Comentario_Usuario FOREIGN KEY (UsuarioId)       REFERENCES Usuario (Id)    ON DELETE CASCADE,
    CONSTRAINT FK_Comentario_Livro   FOREIGN KEY (LivroId)         REFERENCES Livro (Id)      ON DELETE CASCADE,
    CONSTRAINT FK_Comentario_Pai     FOREIGN KEY (ComentarioPaiId) REFERENCES Comentario (Id) ON DELETE CASCADE
);

CREATE TABLE Favorito (
    UsuarioId  INTEGER NOT NULL,
    LivroId    INTEGER NOT NULL,
    DataAdicao TEXT    NOT NULL,
    PRIMARY KEY (UsuarioId, LivroId),
    CONSTRAINT FK_Favorito_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Favorito_Livro   FOREIGN KEY (LivroId)   REFERENCES Livro (Id)   ON DELETE CASCADE
);

CREATE INDEX IX_Livro_Editora        ON Livro (EditoraId);
CREATE INDEX IX_Exemplar_Livro       ON ExemplarDigital (LivroId);
CREATE INDEX IX_Reserva_Exemplar     ON Reserva (ExemplarId);
CREATE INDEX IX_Reserva_Leitor       ON Reserva (LeitorId);
CREATE INDEX IX_Emprestimo_Leitor    ON Emprestimo (LeitorId);
CREATE INDEX IX_Emprestimo_Exemplar  ON Emprestimo (ExemplarId);
CREATE INDEX IX_Renovacao_Emprestimo ON Renovacao (EmprestimoId);
CREATE INDEX IX_Leitura_Usuario      ON Leitura (UsuarioId);
CREATE INDEX IX_Comentario_Livro     ON Comentario (LivroId);

INSERT INTO Usuario (Tipo, Nome, Email, EstaBloqueado, LimiteEmprestimos) VALUES
('ADMINISTRADOR', 'Otavio Ricardo Barbosa dos Santos',     'otavio.santos@biblioteca.com',  0, NULL),
('ADMINISTRADOR', 'Felipe dos Santos Lofrano',             'felipe.lofrano@biblioteca.com', 0, NULL),
('LEITOR',        'Gabriel Enzo dos Santos Teodoro Souza', 'gabriel.souza@email.com',       0, 3),
('LEITOR',        'Claudio Mauricio Bispo Junior',         'claudio.bispo@email.com',       0, 5),
('LEITOR',        'Mariana Alves Ferreira',                'mariana.ferreira@email.com',    0, 3),
('LEITOR',        'Rafael Nogueira Lima',                  'rafael.lima@email.com',         1, 3),
('LEITOR',        'Beatriz Carvalho Pinto',                'beatriz.pinto@email.com',       0, 4),
('LEITOR',        'Lucas Pereira Monteiro',                'lucas.monteiro@email.com',      0, 3);

INSERT INTO Autor (Nome, Nacionalidade) VALUES
('Machado de Assis',       'Brasileira'),
('Clarice Lispector',      'Brasileira'),
('Jorge Amado',            'Brasileira'),
('George Orwell',          'Britanica'),
('Ursula K. Le Guin',      'Estadunidense'),
('Gabriel Garcia Marquez', 'Colombiana'),
('Robert C. Martin',       'Estadunidense'),
('Andrew Hunt',            'Estadunidense');

INSERT INTO Editora (Nome, Endereco) VALUES
('Companhia das Letras', 'Rua Bandeira Paulista, 702 - Sao Paulo/SP'),
('Editora Record',       'Rua Argentina, 171 - Rio de Janeiro/RJ'),
('Penguin Books',        '80 Strand - Londres, Reino Unido'),
('Alta Books',           'Rua Viuva Claudio, 291 - Rio de Janeiro/RJ'),
('Editora Rocco',        'Rua Evaristo da Veiga, 65 - Rio de Janeiro/RJ'),
('Novatec Editora',      'Rua Luis Antonio dos Santos, 110 - Sao Paulo/SP');

INSERT INTO Categoria (Nome, Descricao) VALUES
('Romance',               'Narrativas longas de ficcao centradas em personagens.'),
('Ficcao Cientifica',     'Obras especulativas sobre ciencia, tecnologia e futuro.'),
('Tecnologia',            'Livros tecnicos de computacao e engenharia de software.'),
('Literatura Brasileira', 'Obras produzidas por autores brasileiros.'),
('Distopia',              'Sociedades imaginarias opressivas e seus conflitos.'),
('Realismo Magico',       'Narrativas realistas atravessadas por elementos fantasticos.');

INSERT INTO Livro (Titulo, Isbn, AnoPublicacao, Descricao, EditoraId, ValorReferencia) VALUES
('Dom Casmurro',                '9788535910663', 1899, 'Bentinho recorda a juventude e a duvida sobre Capitu.',                    1,  49.90),
('A Hora da Estrela',           '9788532511010', 1977, 'A trajetoria de Macabea narrada por Rodrigo S.M.',                        5,  44.50),
('Capitaes da Areia',           '9788535911442', 1937, 'O cotidiano de meninos de rua em Salvador nos anos 1930.',                1,  52.00),
('1984',                        '9780451524935', 1949, 'Winston Smith enfrenta a vigilancia total do Grande Irmao.',              3,  59.90),
('A Mao Esquerda da Escuridao', '9788565765138', 1969, 'Um enviado humano negocia com um povo de genero fluido em Gethen.',       2,  64.00),
('Cem Anos de Solidao',         '9788501012074', 1967, 'A saga da familia Buendia na mitica Macondo.',                            2,  69.90),
('Codigo Limpo',                '9788576082675', 2008, 'Praticas e principios para escrever software legivel e sustentavel.',     4, 129.90),
('O Programador Pragmatico',    '9788577807642', 1999, 'Habitos e tecnicas para a pratica diaria do desenvolvimento.',            6, 119.00);

INSERT INTO LivroAutor (LivroId, AutorId) VALUES
(1, 1), (2, 2), (3, 3), (4, 4), (5, 5), (6, 6), (7, 7), (8, 8), (8, 7);

INSERT INTO LivroCategoria (LivroId, CategoriaId) VALUES
(1, 1), (1, 4),
(2, 1), (2, 4),
(3, 1), (3, 4),
(4, 2), (4, 5),
(5, 2),
(6, 1), (6, 6),
(7, 3),
(8, 3);

INSERT INTO ExemplarDigital (LivroId, Formato, Arquivo, LicencasTotais, LicencasDisponiveis) VALUES
(1, 'EPUB',      'dom-casmurro.epub',           5, 4),
(1, 'PDF',       'dom-casmurro.pdf',            3, 3),
(2, 'EPUB',      'a-hora-da-estrela.epub',      4, 4),
(3, 'EPUB',      'capitaes-da-areia.epub',      4, 3),
(4, 'EPUB',      '1984.epub',                   6, 6),
(4, 'AUDIOBOOK', '1984-audiobook.m4b',          2, 2),
(5, 'PDF',       'mao-esquerda-escuridao.pdf',  3, 2),
(6, 'EPUB',      'cem-anos-de-solidao.epub',    5, 5),
(7, 'PDF',       'codigo-limpo.pdf',            4, 4),
(8, 'MOBI',      'programador-pragmatico.mobi', 3, 3);

INSERT INTO Reserva (LeitorId, ExemplarId, DataReserva, DataLimiteRetirada, PosicaoFila, Status) VALUES
(5, 5, '2026-10-20 09:15:00', '2026-10-23 09:15:00', 1, 3),
(7, 5, '2026-11-05 15:40:00', '2026-11-08 15:40:00', 1, 1),
(8, 9, '2026-11-10 18:05:00', '2026-11-13 18:05:00', 1, 1),
(4, 4, '2026-09-12 10:00:00', '2026-09-15 10:00:00', 1, 5),
(3, 1, '2026-11-18 08:30:00', '2026-11-21 08:30:00', 2, 4);

INSERT INTO Emprestimo (LeitorId, ExemplarId, ReservaId, ValorReferenciaObra, DataRetirada, DataPrevista, DataDevolucao, Status) VALUES
(3, 1,  NULL, 49.90,  '2026-11-10 10:00:00', '2026-11-24 10:00:00', NULL,                  1),
(4, 4,  NULL, 52.00,  '2026-11-01 14:20:00', '2026-11-29 14:20:00', NULL,                  2),
(5, 5,  1,    59.90,  '2026-10-20 09:30:00', '2026-11-03 09:30:00', '2026-11-02 17:10:00', 3),
(6, 9,  NULL, 129.90, '2026-09-05 11:00:00', '2026-09-19 11:00:00', '2026-10-01 09:45:00', 4),
(7, 8,  NULL, 69.90,  '2026-10-01 08:00:00', '2026-10-29 08:00:00', '2026-10-14 20:30:00', 3),
(8, 10, NULL, 119.00, '2026-10-25 16:00:00', '2026-11-08 16:00:00', '2026-11-12 10:20:00', 4),
(5, 7,  NULL, 64.00,  '2026-10-01 09:00:00', '2026-10-15 09:00:00', NULL,                  1);

INSERT INTO Renovacao (EmprestimoId, DataRenovacao, NovaDataPrevista, Sequencia) VALUES
(2, '2026-11-14 09:00:00', '2026-11-29 14:20:00', 1),
(5, '2026-10-08 13:25:00', '2026-10-29 08:00:00', 1);

INSERT INTO Devolucao (EmprestimoId, DataDevolucao, HouveAtraso) VALUES
(3, '2026-11-02 17:10:00', 0),
(4, '2026-10-01 09:45:00', 1),
(5, '2026-10-14 20:30:00', 0),
(6, '2026-11-12 10:20:00', 1);

INSERT INTO Multa (EmprestimoId, ValorDiario, DiasAtraso, ValorReferenciaObra, ValorTotal, Paga) VALUES
(4, 1.50, 12, 129.90, 18.00, 1),
(6, 1.50,  4, 119.00,  6.00, 0);

INSERT INTO Leitura (UsuarioId, LivroId, Status, PaginaAtual, TotalPaginas, UltimaAtualizacao, DataInicio, DataConclusao) VALUES
(3, 1, 2, 120, 256, '2026-11-18 21:40:00', '2026-11-10 10:30:00', NULL),
(4, 3, 2,  80, 280, '2026-11-15 22:05:00', '2026-11-01 15:00:00', NULL),
(5, 4, 4, 328, 328, '2026-11-02 16:50:00', '2026-10-20 10:00:00', '2026-11-02 16:50:00'),
(6, 7, 4, 432, 432, '2026-09-30 19:15:00', '2026-09-05 12:00:00', '2026-09-30 19:15:00'),
(7, 6, 4, 448, 448, '2026-10-14 19:55:00', '2026-10-01 09:00:00', '2026-10-14 19:55:00'),
(8, 8, 3, 150, 352, '2026-11-09 23:10:00', '2026-10-25 17:00:00', NULL),
(3, 4, 1,   0, 328, '2026-11-20 08:00:00', NULL,                  NULL);

INSERT INTO Avaliacao (UsuarioId, LivroId, Nota, DataAvaliacao) VALUES
(5, 4, 5, '2026-11-02 17:00:00'),
(6, 7, 4, '2026-09-30 20:00:00'),
(7, 6, 5, '2026-10-14 20:10:00');

INSERT INTO Comentario (UsuarioId, LivroId, ComentarioPaiId, Texto, DataCriacao, Editado, Excluido) VALUES
(5, 4, NULL, 'A segunda parte do livro e angustiante na medida certa.',             '2026-11-02 17:05:00', 0, 0),
(6, 7, NULL, 'Os capitulos sobre nomes significativos mudaram meu jeito de codar.', '2026-09-30 20:05:00', 0, 0),
(7, 6, NULL, 'Macondo e um personagem a parte nessa historia.',                     '2026-10-14 20:15:00', 0, 0),
(3, 1, NULL, 'A duvida sobre Capitu atravessa o livro inteiro.',                    '2026-11-11 09:30:00', 0, 0),
(8, 8, NULL, 'Recomendo ler junto com o Codigo Limpo.',                             '2026-11-09 23:20:00', 1, 0),
(4, 3, NULL, 'Comentario removido pelo proprio autor.',                             '2026-11-05 12:00:00', 0, 1);

INSERT INTO Favorito (UsuarioId, LivroId, DataAdicao) VALUES
(3, 1, '2026-11-10 10:35:00'),
(5, 4, '2026-11-02 17:12:00'),
(6, 7, '2026-09-30 20:20:00'),
(7, 6, '2026-10-14 20:25:00'),
(8, 8, '2026-10-25 17:10:00'),
(4, 3, '2026-11-01 15:05:00');
