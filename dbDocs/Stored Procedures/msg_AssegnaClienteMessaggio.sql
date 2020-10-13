-- =============================================
-- Author:		ape
-- Create date: 07/02/2020
-- Description:	assegna codicecliente a una mail ricevuta
-- =============================================
CREATE PROCEDURE [dbo].[msg_AssegnaClienteMessaggio] (
	@IdAllegato nvarchar(255)
   ,@CodiceSoggetto	 nvarchar(50)
   ,@NomeSoggetto  nvarchar(100) 
)

AS
BEGIN
	SET NOCOUNT ON;
	update [dbo].[Allegati]
	  set Chiave3 =@CodiceSoggetto
	  , Chiave4 =@NomeSoggetto
	  , Attributi =  JSON_MODIFY(JSON_MODIFY(Attributi,'$.CodiceSoggetto',@CodiceSoggetto),'$.NomeSoggetto',@NomeSoggetto)
	where tipo='EMAIL' and ID = @IdAllegato

END
