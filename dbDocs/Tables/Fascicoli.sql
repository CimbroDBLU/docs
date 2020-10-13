CREATE TABLE [dbo].[Fascicoli] (
    [ID]             UNIQUEIDENTIFIER NOT NULL,
    [Descrizione]    NVARCHAR (255)   CONSTRAINT [DF_Fascicolo_Descrizione] DEFAULT ('') NOT NULL,
    [Categoria]      NVARCHAR (20)    CONSTRAINT [DF_Fascicolo_Categoria] DEFAULT ('') NOT NULL,
    [Chiave1]        NVARCHAR (255)   CONSTRAINT [DF_Fascicolo_Chiave1] DEFAULT ('') NOT NULL,
    [Chiave2]        NVARCHAR (255)   CONSTRAINT [DF_Fascicolo_Chiave2] DEFAULT ('') NOT NULL,
    [Chiave3]        NVARCHAR (255)   CONSTRAINT [DF_Fascicolo_Chiave3] DEFAULT ('') NOT NULL,
    [Chiave4]        NVARCHAR (255)   CONSTRAINT [DF_Fascicolo_Chiave4] DEFAULT ('') NOT NULL,
    [Chiave5]        NVARCHAR (255)   CONSTRAINT [DF_Fascicolo_Chiave5] DEFAULT ('') NOT NULL,
    [DataC]          DATETIME         CONSTRAINT [DF_Fascicolo_DataC] DEFAULT (getdate()) NOT NULL,
    [UtenteC]        NVARCHAR (50)    CONSTRAINT [DF_Fascicolo_UtenteC] DEFAULT ('') NOT NULL,
    [DataUM]         DATETIME         CONSTRAINT [DF_Fascicolo_DataUM] DEFAULT (getdate()) NOT NULL,
    [UtenteUM]       NVARCHAR (50)    CONSTRAINT [DF_Fascicolo_UtenteUM] DEFAULT ('') NOT NULL,
    [Attributi]      NVARCHAR (MAX)   CONSTRAINT [DF_Fascicolo_Attributi] DEFAULT ('') NOT NULL,
    [CodiceSoggetto] NVARCHAR (20)    NULL,
    CONSTRAINT [PK_Fascicolo] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_Fascicoli_Categorie] FOREIGN KEY ([Categoria]) REFERENCES [dbo].[Categorie] ([Codice]) ON DELETE SET DEFAULT ON UPDATE SET DEFAULT
);


GO
CREATE NONCLUSTERED INDEX [IX_Fascicoli_CodiceSoggetto]
    ON [dbo].[Fascicoli]([CodiceSoggetto] ASC);

