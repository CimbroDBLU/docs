CREATE TABLE [dbo].[Soggetti] (
    [Codice]     NVARCHAR (20)  DEFAULT ('') NOT NULL,    
    [Nome]       NVARCHAR (255) DEFAULT ('') NOT NULL,
    [Indirizzo]  NVARCHAR (255) DEFAULT ('') NULL,
    [CAP]        NVARCHAR (20)  DEFAULT ('') NULL,
    [Localita]   NVARCHAR (255) DEFAULT ('') NULL,
    [Provincia]  NVARCHAR (20)  DEFAULT ('') NULL,
    [Note]       NVARCHAR (255) DEFAULT ('') NULL,
    [Stato]      INT            NOT NULL,
    [DataC]      DATETIME       DEFAULT (getdate()) NOT NULL,
    [UtenteC]    NVARCHAR (50)  DEFAULT ('') NOT NULL,
    [DataUM]     DATETIME       DEFAULT (getdate()) NOT NULL,
    [UtenteUM]   NVARCHAR (50)  DEFAULT ('') NOT NULL,
    [Attributi]  NVARCHAR (MAX) DEFAULT ('') NULL,
    [Nazione]    NVARCHAR (20)  DEFAULT ('') NULL,
    [Nomignolo]  NVARCHAR (50)  DEFAULT ('') NULL,
    [PartitaIVA] NVARCHAR (20)  DEFAULT ('') NULL,
    [NuovoCodice] NVARCHAR (20)  DEFAULT ('') NULL,
    CONSTRAINT [PK_Soggetti] PRIMARY KEY CLUSTERED ([Codice] ASC)
);

