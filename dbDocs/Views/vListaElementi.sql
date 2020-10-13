CREATE VIEW [dbo].[vListaElementi]
	AS 
SELECT        E.IDFascicolo, F.CodiceSoggetto, E.ID AS IdElemento, E.Revisione, 
              E.Tipo AS TipoElemento, TE.Descrizione AS DscTipoElemento, E.Descrizione AS DscElemento, E.Stato, E.UtenteC, E.DataC, E.UtenteUM, E.DataUM, 
              E.Chiave1 AS Campo1, E.Chiave2 AS Campo2, E.Chiave3 AS Campo3, E.Chiave4 AS Campo4, E.Chiave5 AS Campo5, 
              '' AS Campo6, '' AS Campo7, '' AS Campo8, '' AS Campo9, '' AS Campo10
FROM          Elementi AS E INNER JOIN
                         Fascicoli AS F ON F.ID = E.IDFascicolo inner join 
                         TipiElementi AS TE ON E.Tipo = TE.Codice 

