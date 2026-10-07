-- Query: DocumenteAnulate
-- Type: Union

SELECT Platitori.IDP, Prezenta.IDL, Prezenta.IDZ, Platitori.Nume, Platitori.CNP, Grupe.Grupa, Plati.Data as Dt, Switch(Nz([tip],'X')='0',[altedoc]![nrdoc] & "-" & [altedoc]![datadoc],Nz([tip],'X')='2',[chitante]![numar] & "-" & [chitante]![data],Nz([tip],'X')='X','') AS Document, CDbl(IIf(Nz([tip],"X")="X",0,[plati]![plata])) AS Plati, 0 as Retur, IIf(Nz([tip],"X")="X",'',[plati]![motivul]) AS Motiv, Switch(Nz([tip],'X')='0','Alte plăți',Nz([tip],'X')='2','Chitanțe',Nz([tip],'X')='X','') AS TIPUL
FROM (((Platitori INNER JOIN (Grupe INNER JOIN Prezenta ON Grupe.IDG = Prezenta.IDG) ON Platitori.IDP = Prezenta.IDP) LEFT JOIN (SELECT * FROM Plati WHERE Anulata = True)  AS xPlati ON Prezenta.IDZ = xPlati.IDZ) LEFT JOIN Chitante ON xPlati.IDPL = Chitante.IDPL) LEFT JOIN AlteDoc ON xPlati.IDPL = AlteDoc.IDPL
WHERE (((Prezenta.IDL) Like [forms]![Prezenta]![IDl]) AND ((CDbl(IIf(Nz([tip],"X")="X",0,[plati]![plata])))<>0))

UNION ALL SELECT Platitori.IDP, Prezenta.IDL, Prezenta.IDZ, Platitori.Nume, Platitori.CNP, Grupe.Grupa, Retur.Data, [nrdoc] & " - " & [data] AS Document, 0, Retur.suma AS Valoare, Retur.motivul AS Motiv, "Restituire sumă" AS TIPUL
FROM (Platitori INNER JOIN (Grupe INNER JOIN Prezenta ON Grupe.IDG = Prezenta.IDG) ON Platitori.IDP = Prezenta.IDP) INNER JOIN Retur ON Prezenta.IDZ = Retur.IDZ
WHERE (((Prezenta.IDL) Like [forms]![Prezenta]![IDl]) AND ((Retur.Anulat)=True));
