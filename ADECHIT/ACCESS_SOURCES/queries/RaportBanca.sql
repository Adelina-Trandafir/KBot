-- Query: RaportBanca
-- Type: Select

SELECT X.Grupa, X.Nume, X.Data, X.Serie, X.Numar, X.Incasari, X.Plati
FROM (SELECT Grupe.Grupa, Platitori.Nume, Plati.Data, "A.D." AS Serie, alteDoc.NrDoc AS Numar, Plati.plata AS Incasari, 0 AS Plati FROM (Grupe INNER JOIN Platitori ON Grupe.IDG = Platitori.IDG) INNER JOIN (Plati INNER JOIN AlteDoc ON Plati.IDPL = AlteDoc.IDPL) ON Platitori.IDP = Plati.IDP WHERE (((Plati.Data) Between forms!Prezenta!rapoarte!di And forms!Prezenta!rapoarte!dsf) And ((Plati.Anulata)=False)))  AS X
GROUP BY X.Grupa, X.Nume, X.Data, X.Serie, X.Numar, X.Incasari, X.Plati
ORDER BY X.Data DESC , X.Numar DESC;
