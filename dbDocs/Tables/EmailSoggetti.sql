CREATE TABLE [dbo].[EmailSoggetti] (
    [email]          NVARCHAR (255) NOT NULL,
    [CodiceSoggetto] NVARCHAR (20)  NULL,
    CONSTRAINT [PK_EmailSoggetti] PRIMARY KEY CLUSTERED ([email] ASC),
    CONSTRAINT [FK_Email_Soggetti] FOREIGN KEY ([CodiceSoggetto]) REFERENCES [dbo].[Soggetti] ([Codice])
);


GO
CREATE NONCLUSTERED INDEX [IX_EmailSoggetti_CodiceSoggetto]
    ON [dbo].[EmailSoggetti]([CodiceSoggetto] ASC);

