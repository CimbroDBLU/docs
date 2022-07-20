


CREATE   procedure [dbo].[sp_GetSoggettoEmail]( @mittente nvarchar(255), @Codice nvarchar(20) out , @Nome nvarchar(255) out)
as begin

   set nocount on
   select @Nome='', @Codice='' 

   --select  @Codice=CodiceSoggetto from EmailSoggetti where email=@mittente
   --select @Nome=Nome from soggetti where Codice=@codice

end