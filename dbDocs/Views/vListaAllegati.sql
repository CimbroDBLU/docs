CREATE VIEW [dbo].[vListaAllegati]
	AS SELECT        A.ID AS IdAllegato, A.Descrizione, A.NomeFile, A.Tipo, TA.Descrizione AS DscTipoAllegato, A.IDFascicolo, A.IDElemento, A.Stato, A.Note, A.DataC, A.UtenteC, A.DataUM, A.UtenteUM, A.Origine, A.Chiave1 AS Campo1, 
                         A.Chiave2 AS Campo2, A.Chiave3 AS Campo3, A.Chiave4 AS Campo4, A.Chiave5 AS Campo5, '' AS Campo6, '' AS Campo7, '' AS Campo8, '' AS Campo9, '' AS Campo10
FROM            Allegati AS A INNER JOIN
                         TipiAllegati AS TA ON A.Tipo = TA.Codice
