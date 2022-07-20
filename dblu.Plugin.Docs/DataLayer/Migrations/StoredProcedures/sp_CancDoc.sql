

CREATE PROC [dbo].[sp_CancDoc]
	(@relativePath varchar(MAX)=NULL
	,@name varchar(512)=NULL
	,@path_locator VARCHAR(4000)=NULL 
	)
AS 
BEGIN

	SET NOCOUNT ON

	DECLARE @path_locator_hid HIERARCHYID; 

	BEGIN TRY
		BEGIN TRAN deldocument

			-- test input
			IF @path_locator IS null AND (COALESCE(@relativePath,'')='' OR COALESCE(@name,'')='')
				THROW 50001, 'no input given', 1; 
   
			IF coalesce(@relativePath,'') > ''  BEGIN
				SET @relativePath=LTRIM(RTRIM(@relativePath));
				IF LEFT(@relativePath,1)='\'
						SET @relativePath=SUBSTRING(@relativePath, 2, LEN(@relativePath)-1);
				IF RIGHT(@relativePath,1)='\'
						SET @relativePath=LEFT(@relativePath, LEN(@relativePath)-1);
			END 
              
			-- delete doc
			IF @path_Locator IS NULL   begin
				SELECT @path_locator_hid=getPathLocator(FileTableRootPath('dbo.Docs')+'\'+@relativePath+'\'+@name);
			END    
			ELSE BEGIN
				SET @path_locator_hid=CONVERT(HIERARCHYID,@path_locator);
			end

			DELETE FROM [dbo].[Docs] 
				WHERE path_locator=@path_locator_hid;

			IF @@ROWCOUNT=0 
					THROW 50002, 'document not found', 1;

		COMMIT TRAN deldocument;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT>0
			ROLLBACK TRAN deldocument;
		THROW;

	END CATCH
END
