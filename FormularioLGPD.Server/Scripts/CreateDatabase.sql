/* Scripts/CreateDatabase.sql*/
/*
Script para criação manual do banco de dados (caso necessário)
Execute este script no SQL Server Management Studio se preferir não usar migrations
*/

-- Criar banco de dados
CREATE DATABASE FormularioLGPD
GO

USE FormularioLGPD
GO

-- Tabela Titulares
CREATE TABLE Titulares (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Nome nvarchar(200) NOT NULL,
    CPF nvarchar(11) NOT NULL UNIQUE,
    RG nvarchar(20) NULL,
    DataNascimento datetime2 NOT NULL,
    EstadoCivil nvarchar(50) NOT NULL,
    Naturalidade nvarchar(100) NOT NULL,
    Endereco nvarchar(300) NULL,
    Telefone nvarchar(20) NULL,
    Email nvarchar(200) NOT NULL,
    Escolaridade nvarchar(100) NULL,
    SerieSemestre nvarchar(50) NULL,
    QualificacaoLegal int NOT NULL,
    IsAtivo bit NOT NULL DEFAULT 1,
    DataCadastro datetime2 NOT NULL DEFAULT GETUTCDATE()
)
GO

-- Tabela Dependentes
CREATE TABLE Dependentes (
    Id int IDENTITY(1,1) PRIMARY KEY,
    TitularId int NOT NULL,
    Nome nvarchar(200) NOT NULL,
    CPF nvarchar(11) NOT NULL,
    DataNascimento datetime2 NOT NULL,
    GrauParentesco nvarchar(50) NOT NULL,
    IsAtivo bit NOT NULL DEFAULT 1,
    DataCadastro datetime2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (TitularId) REFERENCES Titulares(Id) ON DELETE CASCADE
)
GO

-- Tabela TermosAceite
CREATE TABLE TermosAceite (
    Id int IDENTITY(1,1) PRIMARY KEY,
    TitularId int NOT NULL UNIQUE,
    NumeroTermo nvarchar(50) NOT NULL UNIQUE,
    ConteudoTermo nvarchar(max) NOT NULL,
    AceiteConfirmado bit NOT NULL,
    DataHoraAceite datetime2 NOT NULL,
    IpOrigem nvarchar(45) NOT NULL,
    UserAgent nvarchar(500) NOT NULL,
    HashIntegridade nvarchar(64) NOT NULL,
    CaminhoArquivoPDF nvarchar(500) NOT NULL,
    VersaoTermo nvarchar(10) NOT NULL DEFAULT '1.0',
    StatusTermo int NOT NULL DEFAULT 1,
    DataCriacao datetime2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (TitularId) REFERENCES Titulares(Id) ON DELETE CASCADE
)
GO

-- Tabela LogsAuditoria
CREATE TABLE LogsAuditoria (
    Id int IDENTITY(1,1) PRIMARY KEY,
    TermoAceiteId int NULL,
    TipoOperacao nvarchar(50) NOT NULL,
    Descricao nvarchar(500) NOT NULL,
    IpOrigem nvarchar(45) NOT NULL,
    UserAgent nvarchar(500) NULL,
    DataHoraOperacao datetime2 NOT NULL,
    DadosAntes nvarchar(max) NULL,
    DadosDepois nvarchar(max) NULL,
    StatusOperacao int NOT NULL,
    
    FOREIGN KEY (TermoAceiteId) REFERENCES TermosAceite(Id) ON DELETE SET NULL
)
GO

-- Índices para performance
CREATE INDEX IX_Dependentes_TitularId_CPF ON Dependentes(TitularId, CPF)
GO

CREATE INDEX IX_TermosAceite_DataHoraAceite ON TermosAceite(DataHoraAceite)
GO

CREATE INDEX IX_LogsAuditoria_DataHoraOperacao ON LogsAuditoria(DataHoraOperacao)
GO

CREATE INDEX IX_LogsAuditoria_TipoOperacao ON LogsAuditoria(TipoOperacao)
GO

-- Dados de exemplo para testes (opcional)
/*
INSERT INTO Titulares (Nome, CPF, DataNascimento, EstadoCivil, Naturalidade, Email, QualificacaoLegal)
VALUES ('João Silva', '12345678901', '1980-01-01', 'Solteiro', 'Boa Vista/RR', 'joao@email.com', 1)
GO
*/