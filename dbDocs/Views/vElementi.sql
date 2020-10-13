

CREATE View vElementi  as 
SELECT     E.IdFascicolo, F.CodiceSoggetto, F.Descrizione AS DscFascicolo, C.Descrizione AS DscCategoria, F.Chiave1 AS Chiave1F, F.Chiave2 AS Chiave2F, F.Chiave3 AS Chiave3F, F.Chiave4 AS Chiave4F, F.Chiave5 AS Chiave5F, E.ID AS IdElemento, E.Revisione, E.Tipo, 
                  E.Descrizione AS DscElemento, TE.Descrizione AS DscTipo, E.Stato, E.Chiave1 AS Chiave1E, E.Chiave2 AS Chiave2E, E.Chiave3 AS Chiave3E, E.Chiave4 AS Chiave4E, E.Chiave5 AS Chiave5E
FROM        Elementi AS E INNER JOIN
                  Fascicoli AS F ON F.ID = E.IDFascicolo INNER JOIN
                  Categorie AS C ON F.Categoria = C.Codice INNER JOIN
                  TipiElementi AS TE ON E.Tipo = TE.Codice