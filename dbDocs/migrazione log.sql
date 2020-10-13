

CREATE TABLE [dbo].[tmp_ms_xx_LogDoc] (
    [IdOggetto]   UNIQUEIDENTIFIER NULL,
    [Data]        DATETIME         NULL,
    [TipoOggetto] SMALLINT         NULL,
    [Utente]      NVARCHAR (20)    NULL,
    [Operazione]  SMALLINT         NULL
);


IF EXISTS (SELECT TOP 1 1 
           FROM   [dbo].[LogDoc])
    BEGIN
        INSERT INTO [dbo].[tmp_ms_xx_LogDoc] (IdOggetto, [Data], TipoOggetto, [Utente], [Operazione])
        SELECT Idelemento, [Data], tipoelemento, [Utente], [Operazione]
        FROM   [dbo].[LogDoc];
    END

DROP TABLE [dbo].[LogDoc];

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_LogDoc]', N'LogDoc';


GO
PRINT N'Creazione di [dbo].[LogDoc].[IX_LogDoc_Column]...';


GO
CREATE INDEX [IX_LogDoc_id_data] ON [dbo].[LogDoc] (IdOggetto,[Data] DESC)

