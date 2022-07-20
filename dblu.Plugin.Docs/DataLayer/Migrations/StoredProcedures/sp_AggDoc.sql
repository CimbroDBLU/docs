
CREATE PROC [dbo].[sp_AggDoc]
	(@relativePath varchar(max)=null
	,@name varchar(4000)=null
	,@stream VARBINARY(MAX)=NULL
	,@path_locator VARCHAR(512)=null
	)
AS 
BEGIN
	DECLARE @path_locator_hid HIERARCHYID;

	BEGIN TRY
		BEGIN TRAN upddocument
              
			-- test input
			IF @path_locator IS null AND (COALESCE(@relativePath,'')='' OR COALESCE(@name,'')='')
				THROW 50001, 'no input given', 1; 
   
			IF coalesce(@relativePath,'') > ''
			BEGIN
			SET @relativePath=LTRIM(RTRIM(@relativePath));
					IF LEFT(@relativePath,1)='\'
							SET @relativePath=SUBSTRING(@relativePath, 2, LEN(@relativePath)-1);
					IF RIGHT(@relativePath,1)='\'
							SET @relativePath=LEFT(@relativePath, LEN(@relativePath)-1);
			END 
              
			-- find pathlocator for doc
			IF @path_Locator IS NULL   begin
				SELECT @path_locator_hid=getPathLocator(FileTableRootPath('dbo.document')+'\'+@relativePath+'\'+@name);
			END  
			ELSE BEGIN
				SET @path_locator_hid=CONVERT(HIERARCHYID,@path_locator);
			end
       
			-- update doc
			UPDATE dbo.Docs  SET file_stream=@stream
				WHERE path_locator=@path_locator_hid;

			IF @@ROWCOUNT=0 
				THROW 50002, 'document not found', 1;

		COMMIT TRAN updocument;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT>0
			ROLLBACK TRAN upddocument;
		THROW;
	END CATCH

END
