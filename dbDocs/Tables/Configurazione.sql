CREATE TABLE [dbo].[Configurazione] (
    [Nome]  NCHAR (10)    NOT NULL,
    [Value] VARCHAR (MAX) NULL,
    CONSTRAINT [PK_Configurazione] PRIMARY KEY CLUSTERED ([Nome] ASC)
);

