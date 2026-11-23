PRAGMA foreign_keys = ON;

DROP TABLE IF EXISTS Multa;
DROP TABLE IF EXISTS Devolucao;
DROP TABLE IF EXISTS Renovacao;
DROP TABLE IF EXISTS Emprestimo;
DROP TABLE IF EXISTS Reserva;
DROP TABLE IF EXISTS ExemplarDigital;
DROP TABLE IF EXISTS Livro;
DROP TABLE IF EXISTS Usuario;

CREATE TABLE Usuario (
    Id                INTEGER PRIMARY KEY AUTOINCREMENT,
    Tipo              TEXT    NOT NULL CHECK (Tipo IN ('LEITOR', 'ADMINISTRADOR')),
    Nome              TEXT    NOT NULL,
    Email             TEXT    NOT NULL UNIQUE,
    EstaBloqueado     INTEGER NOT NULL DEFAULT 0,
    LimiteEmprestimos INTEGER NULL
);

CREATE TABLE Livro (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Titulo          TEXT    NOT NULL,
    Isbn            TEXT    NOT NULL UNIQUE,
    ValorReferencia REAL    NOT NULL
);

CREATE TABLE ExemplarDigital (
    Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
    LivroId             INTEGER NOT NULL,
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

CREATE INDEX IX_Exemplar_Livro       ON ExemplarDigital (LivroId);
CREATE INDEX IX_Reserva_Exemplar     ON Reserva (ExemplarId);
CREATE INDEX IX_Reserva_Leitor       ON Reserva (LeitorId);
CREATE INDEX IX_Emprestimo_Leitor    ON Emprestimo (LeitorId);
CREATE INDEX IX_Emprestimo_Exemplar  ON Emprestimo (ExemplarId);
CREATE INDEX IX_Renovacao_Emprestimo ON Renovacao (EmprestimoId);

INSERT INTO Usuario (Tipo, Nome, Email, EstaBloqueado, LimiteEmprestimos) VALUES
('ADMINISTRADOR', 'Otavio Ricardo Barbosa dos Santos',     'otavio.santos@biblioteca.com',  0, NULL),
('ADMINISTRADOR', 'Felipe dos Santos Lofrano',             'felipe.lofrano@biblioteca.com', 0, NULL),
('LEITOR',        'Gabriel Enzo dos Santos Teodoro Souza', 'gabriel.souza@email.com',       0, 3),
('LEITOR',        'Claudio Mauricio Bispo Junior',         'claudio.bispo@email.com',       0, 5),
('LEITOR',        'Mariana Alves Ferreira',                'mariana.ferreira@email.com',    0, 3),
('LEITOR',        'Rafael Nogueira Lima',                  'rafael.lima@email.com',         1, 3),
('LEITOR',        'Beatriz Carvalho Pinto',                'beatriz.pinto@email.com',       0, 4),
('LEITOR',        'Lucas Pereira Monteiro',                'lucas.monteiro@email.com',      0, 3);

INSERT INTO Livro (Titulo, Isbn, ValorReferencia) VALUES
('Dom Casmurro',                '9788535910663',  49.90),
('A Hora da Estrela',           '9788532511010',  44.50),
('Capitaes da Areia',           '9788535911442',  52.00),
('1984',                        '9780451524935',  59.90),
('A Mao Esquerda da Escuridao', '9788565765138',  64.00),
('Cem Anos de Solidao',         '9788501012074',  69.90),
('Codigo Limpo',                '9788576082675', 129.90),
('O Programador Pragmatico',    '9788577807642', 119.00);

INSERT INTO ExemplarDigital (LivroId, LicencasTotais, LicencasDisponiveis) VALUES
(1, 5, 4),
(1, 3, 3),
(2, 4, 4),
(3, 4, 3),
(4, 6, 6),
(4, 2, 2),
(5, 3, 2),
(6, 5, 5),
(7, 4, 4),
(8, 3, 3);

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
