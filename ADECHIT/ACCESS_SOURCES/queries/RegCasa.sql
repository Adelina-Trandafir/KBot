-- Query: RegCasa
-- Type: Select

SELECT X.Grupa, X.Nume, X.Data, X.Serie, X.Numar, X.Incasari, X.Plati
FROM (SELECT Grupe.Grupa, Platitori.Nume, Chitante.Data, Chitante.Serie, Chitante.Numar, Chitante.Valoare AS Incasari, 0 AS Plati
FROM Grupe INNER JOIN (Platitori INNER JOIN (Plati INNER JOIN Chitante ON Plati.IDPL = Chitante.IDPL) ON Platitori.IDP = Plati.IDP) ON Grupe.IDG = Platitori.IDG
WHERE (((Chitante.Data) Between [forms]![Prezenta]![rapoarte]![di] And [forms]![Prezenta]![rapoarte]![dsf]) AND ((Chitante.Anulata)=False))

UNION ALL SELECT Grupe.Grupa, Platitori.Nume, Retur.Data, "Tic" AS Fel, Retur.Explicatie, 0 AS Incasari, Retur.suma AS Plati
FROM (Grupe INNER JOIN Platitori ON Grupe.IDG = Platitori.IDG) INNER JOIN Retur ON Platitori.IDP = Retur.IDP
WHERE (((Retur.Data) Between [forms]![Prezenta]![rapoarte]![di] And [forms]![Prezenta]![rapoarte]![dsf]) AND ((Retur.Anulat)=False))

)  AS X
GROUP BY X.Grupa, X.Nume, X.Data, X.Serie, X.Numar, X.Incasari, X.Plati
ORDER BY X.Data DESC , X.Numar DESC;
