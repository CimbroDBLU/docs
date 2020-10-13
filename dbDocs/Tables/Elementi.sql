CREATE TABLE [dbo].[Elementi] (
    [ID]          UNIQUEIDENTIFIER NOT NULL,
    [Revisione]   SMALLINT         CONSTRAINT [DF_Elementi_Revisione] DEFAULT ((0)) NOT NULL,
    [Tipo]        NVARCHAR (20)    CONSTRAINT [DF_Elementi_Tipo] DEFAULT ('') NOT NULL,
    [Descrizione] NVARCHAR (255)   CONSTRAINT [DF_Elementi_Descrizione] DEFAULT ('') NOT NULL,
    [Stato]       SMALLINT         CONSTRAINT [DF_Elementi_Stato] DEFAULT ((0)) NOT NULL,
    [IDFascicolo] UNIQUEIDENTIFIER NULL,
    [Attributi]   NVARCHAR (MAX)   CONSTRAINT [DF_Elementi_Attributi] DEFAULT ('') NOT NULL,
    [DataC]       DATETIME         CONSTRAINT [DF_Elementi_DataC] DEFAULT (getdate()) NOT NULL,
    [UtenteC]     NVARCHAR (50)    CONSTRAINT [DF_Elementi_UtenteC] DEFAULT ('') NOT NULL,
    [DataUM]      DATETIME         CONSTRAINT [DF_Elementi_DataUM] DEFAULT (getdate()) NOT NULL,
    [UtenteUM]    NVARCHAR (50)    CONSTRAINT [DF_Elementi_UtenteUM] DEFAULT ('') NOT NULL,
    [Chiave1]     NVARCHAR (255)   DEFAULT ('') NOT NULL,
    [Chiave2]     NVARCHAR (255)   DEFAULT ('') NOT NULL,
    [Chiave3]     NVARCHAR (255)   DEFAULT ('') NOT NULL,
    [Chiave4]     NVARCHAR (255)   DEFAULT ('') NOT NULL,
    [Chiave5]     NVARCHAR (255)   DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_Elementi] PRIMARY KEY CLUSTERED ([ID] ASC, [Revisione] ASC),
    CONSTRAINT [FK_Elementi_Fascicoli] FOREIGN KEY ([IDFascicolo]) REFERENCES [dbo].[Fascicoli] ([ID]) ON DELETE CASCADE,
    CONSTRAINT [FK_Elementi_TipiElementi] FOREIGN KEY ([Tipo]) REFERENCES [dbo].[TipiElementi] ([Codice])
);

