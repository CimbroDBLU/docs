CREATE TABLE [dbo].[Allegati] (
    [ID]          UNIQUEIDENTIFIER NOT NULL,
    [Descrizione] NVARCHAR (255)   CONSTRAINT [DF_Allegati_Descrizione] DEFAULT ('') NOT NULL,
    [NomeFile]    NVARCHAR (255)   NULL,
    [Tipo]        NVARCHAR (20)    CONSTRAINT [DF_Allegati_Tipo] DEFAULT ('') NOT NULL,
    [IDFascicolo] UNIQUEIDENTIFIER NULL,
    [IDElemento]  UNIQUEIDENTIFIER NULL,
    [Stato]       SMALLINT         CONSTRAINT [DF_Allegati_Stato] DEFAULT ((1)) NULL,
    [Attributi]   NVARCHAR (MAX)   CONSTRAINT [DF_Allegati_Attributi] DEFAULT ('') NOT NULL,
    [Note]        NVARCHAR (MAX)   CONSTRAINT [DF_Allegati_Note] DEFAULT ('') NOT NULL,
    [DataC]       DATETIME         CONSTRAINT [DF_Allegati_DataC] DEFAULT (getdate()) NOT NULL,
    [UtenteC]     NVARCHAR (50)    CONSTRAINT [DF_Allegati_UtenteC] DEFAULT ('') NOT NULL,
    [DataUM]      DATETIME         CONSTRAINT [DF_Allegati_DataUM] DEFAULT (getdate()) NOT NULL,
    [UtenteUM]    NVARCHAR (50)    CONSTRAINT [DF_Allegati_UtenteUM] DEFAULT ('') NOT NULL,
    [Chiave1]     NVARCHAR (255)   DEFAULT ('') NOT NULL,
    [Chiave2]     NVARCHAR (255)   DEFAULT ('') NOT NULL,
    [Chiave3]     NVARCHAR (255)   DEFAULT ('') NOT NULL,
    [Chiave4]     NVARCHAR (255)   DEFAULT ('') NOT NULL,
    [Chiave5]     NVARCHAR (255)   DEFAULT ('') NOT NULL,
    [Origine]     NVARCHAR (50)    CONSTRAINT [DF_Allegati_Origine] DEFAULT ('') NULL,
    CONSTRAINT [PK_Allegati] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_Allegati_Fascicoli] FOREIGN KEY ([IDFascicolo]) REFERENCES [dbo].[Fascicoli] ([ID]) ON DELETE CASCADE,
    CONSTRAINT [FK_Allegati_TipiAllegati] FOREIGN KEY ([Tipo]) REFERENCES [dbo].[TipiAllegati] ([Codice])
);


GO
CREATE NONCLUSTERED INDEX [IX_Allegati]
    ON [dbo].[Allegati]([Origine] ASC);

