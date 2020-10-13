CREATE TABLE [dbo].[ElementiInRoles] (
    [Tipo]   NVARCHAR (20) NOT NULL,
    [RoleId] NVARCHAR (64) NOT NULL,
    CONSTRAINT [PK_ElementiInRoles] PRIMARY KEY CLUSTERED ([Tipo] ASC, [RoleId] ASC)
);

