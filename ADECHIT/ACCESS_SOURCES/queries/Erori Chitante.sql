-- Query: Erori Chitante
-- Type: Select

SELECT Platitori.Nume, Plati.IDZ, Chitante.Data, Chitante.Numar, Chitante.Explicatie, Chitante.Valoare, Chitante.Anulata
FROM (Platitori INNER JOIN Prezenta ON Platitori.IDP = Prezenta.IDP) INNER JOIN (Plati INNER JOIN Chitante ON Plati.IDPL = Chitante.IDPL) ON Prezenta.IDZ = Plati.IDZ;
