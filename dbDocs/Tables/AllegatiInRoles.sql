CREATE TABLE [dbo].[AllegatiInRoles] (
    [Tipo]   NVARCHAR (20) NOT NULL,
    [RoleId] NVARCHAR (64) NOT NULL, 
    CONSTRAINT [PK_AllegatiInRoles] PRIMARY KEY ([Tipo], [RoleId])
);

