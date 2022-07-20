CREATE VIEW [dbo].[vRicercaElementi]
	AS 
SELECT        E.IDFascicolo, F.CodiceSoggetto, ISNULL(S.Nome, N'') AS NomeSoggetto, F.Descrizione AS DscFascicolo, F.Categoria AS CodCategoria, C.Descrizione AS DscCategoria, E.ID AS IdElemento, E.Revisione, 
                         E.Tipo AS TipoElemento, TE.Descrizione AS DscTipoElemento, E.Descrizione AS DscElemento, E.Stato, E.UtenteC, E.DataC, E.UtenteUM, E.DataUM, F.Chiave1 AS Campo1, F.Chiave2 AS Campo2, F.Chiave3 AS Campo3, 
                         F.Chiave4 AS Campo4, F.Chiave5 AS Campo5, E.Chiave1 AS Campo6, E.Chiave2 AS Campo7, E.Chiave3 AS Campo8, E.Chiave4 AS Campo9, E.Chiave5 AS Campo10
FROM            Elementi AS E INNER JOIN
                         Fascicoli AS F ON F.ID = E.IDFascicolo INNER JOIN
                         Categorie AS C ON F.Categoria = C.Codice INNER JOIN
                         TipiElementi AS TE ON E.Tipo = TE.Codice LEFT OUTER JOIN
                         Soggetti AS S ON S.Codice = F.CodiceSoggetto
