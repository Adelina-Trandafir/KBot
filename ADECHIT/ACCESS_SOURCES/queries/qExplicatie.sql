-- Query: qExplicatie
-- Type: Append

INSERT INTO Explicatie_Lunara ( IDZ, DOC )
SELECT Expl.IDZ, Expl.DOC
FROM (SELECT Plati.IDZ, "Ch:" & [Numar] AS DOC
FROM Plati INNER JOIN Chitante ON Plati.IDPL = Chitante.IDPL
WHERE Plati.Anulata=False AND Plati.IDL Like [forms]![Prezenta]![IDL]
ORDER BY Plati.IDZ

UNION ALL 

SELECT Plati.IDZ, Left([FelDoc],2) & "." & [NrDoc] AS Expr1
FROM Plati INNER JOIN AlteDoc ON Plati.IDPL = AlteDoc.IDPL
WHERE Plati.Anulata=False AND Plati.IDL Like [forms]![Prezenta]![IDL]

UNION ALL 

SELECT Retur.IDZ, "Re: " & nz(Retur.nrdoc,'Fără nr.')
FROM Retur
WHERE Retur.Anulat=False AND Retur.IDL Like [Forms]![Prezenta]![IDL]
)  AS Expl
ORDER BY Expl.IDZ;
