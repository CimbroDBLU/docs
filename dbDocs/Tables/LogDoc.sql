/*CREATE TABLE [dbo].[LogDoc] (
    [ID]           UNIQUEIDENTIFIER NOT NULL,
    [Data]         DATETIME         NULL,
    [Utente]       NVARCHAR (20)    NULL,
    [TipoElemento] NVARCHAR (20)    NULL,
    [IDElemento]   UNIQUEIDENTIFIER NULL,
    [Operazione]   NVARCHAR (20)    NULL,
    CONSTRAINT [PK_Log] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Log1]
    ON [dbo].[LogDoc]([ID] ASC);

*/
CREATE TABLE [dbo].[LogDoc] (
    [IdOggetto]   UNIQUEIDENTIFIER NULL,
    [Data]         DATETIME         NULL,
    [TipoOggetto] SMALLINT   NULL,
    [Utente]       NVARCHAR (20)    NULL,
    [Operazione]  SMALLINT         NULL
);

GO

CREATE INDEX [IX_LogDoc_id_data] ON [dbo].[LogDoc] (IdOggetto,[Data] DESC)

GO
CREATE NONCLUSTERED INDEX [IX_LogDoc_Column]
    ON [dbo].[LogDoc]([IdOggetto] ASC, [Data] DESC);

