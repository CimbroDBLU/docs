CREATE TABLE [dbo].[TipiElementi] (
    [Codice]         NVARCHAR (20)  NOT NULL,
    [Descrizione]    NVARCHAR (255) NULL,
    [ViewAttributi]  NVARCHAR (255) DEFAULT ('') NULL,
    [ListaAttributi] NVARCHAR (MAX) DEFAULT ('') NOT NULL,
    [Categoria]      NVARCHAR (20)  DEFAULT (NULL) NULL,
    [Processo]       NVARCHAR (255) NULL,
    [AggregaAElemento] BIT NULL DEFAULT 0, 
    [RuoliCandidati] NVARCHAR(MAX) NULL, 
    [UtentiCandidati] NVARCHAR(MAX) NULL, 
    CONSTRAINT [PK_TipiElementi] PRIMARY KEY CLUSTERED ([Codice] ASC),
    CONSTRAINT [FK_TipiElementi_Categorie] FOREIGN KEY ([Categoria]) REFERENCES [dbo].[Categorie] ([Codice])
);


GO
CREATE NONCLUSTERED INDEX [IX_TipiElementi_Categoria]
    ON [dbo].[TipiElementi]([Categoria] ASC);

