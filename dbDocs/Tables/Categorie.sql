CREATE TABLE [dbo].[Categorie] (
    [Codice]         NVARCHAR (20)  NOT NULL,
    [Descrizione]    NVARCHAR (255) NULL,
    [ViewAttributi]  NVARCHAR (255) DEFAULT ('') NULL,
    [ListaAttributi] NVARCHAR (MAX) DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_Categorie] PRIMARY KEY CLUSTERED ([Codice] ASC)
);

