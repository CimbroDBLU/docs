CREATE TABLE [dbo].[EmailServer] (
    [Nome]         NVARCHAR (50)  NOT NULL,
    [email]        NVARCHAR (255) CONSTRAINT [DF_EmailCaselle_email] DEFAULT '' NOT NULL,
    [Server]       NVARCHAR (50)  NOT NULL,
    [Porta]        INT            NOT NULL,
    [SSL]          BIT            CONSTRAINT [DF_EmailCaselle_SSL] DEFAULT 0 NOT NULL,
    [Utente]       NVARCHAR (50)  NOT NULL,
    [Password]     NVARCHAR (50)  NOT NULL,
    [Intervallo]   INT            CONSTRAINT [DF_EmailCaselle_Intervallo] DEFAULT 1000 NOT NULL,
    [Attivo]       BIT            CONSTRAINT [DF_EmailCaselle_Attiva] DEFAULT 0 NOT NULL,
    [Cartella]     NVARCHAR (50)  CONSTRAINT [DF_EmailCaselle_CartellaBak] DEFAULT '' NOT NULL,
    [InUscita]     BIT            CONSTRAINT [DF_EmailServer_InUsccita] DEFAULT 0 NOT NULL,
    [NomeProcesso] NVARCHAR (255) DEFAULT '' NULL,
    [CartellaArchivio] NVARCHAR(50) NOT NULL DEFAULT '', 
    [NomeServerInUscita] NVARCHAR(50) NOT NULL DEFAULT '', 
    CONSTRAINT [PK_EmailCaselle] PRIMARY KEY CLUSTERED ([Nome] ASC)
);

