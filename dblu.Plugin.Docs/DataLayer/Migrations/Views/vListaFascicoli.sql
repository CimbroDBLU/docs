CREATE VIEW [dbo].[vListaFascicoli]
	AS 
SELECT        F.Id as IdFascicolo, F.CodiceSoggetto, ISNULL(S.Nome, N'') AS NomeSoggetto, F.Descrizione AS DscFascicolo, F.Categoria AS CodCategoria, C.Descrizione AS DscCategoria, 
   F.UtenteC, F.DataC, F.UtenteUM, F.DataUM, F.Chiave1 AS Campo1, F.Chiave2 AS Campo2, F.Chiave3 AS Campo3, F.Chiave4 AS Campo4, F.Chiave5 AS Campo5, 
   '' AS Campo6, '' AS Campo7, '' AS Campo8, '' AS Campo9, '' AS Campo10
FROM   Fascicoli AS F INNER JOIN
            Categorie AS C ON F.Categoria = C.Codice LEFT OUTER JOIN
            Soggetti AS S ON S.Codice = F.CodiceSoggetto
