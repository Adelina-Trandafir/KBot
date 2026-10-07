-- Query: qPrezenta
-- Type: Append

INSERT INTO Situatie_buget ( IDG, IDP, IDL, IDZ, Luna, Anul, Grupa, Educator, Nume, CNP, Plecat, ZilePrezenta, ValoareContract, ValoareTotala, Plata, Plati, Retur )
SELECT Grupe.IDG, Platitori.IDP, LunaD.IDL, Prezenta.IDZ, LunaD.LunaT, LunaD.Anul, Grupe.Grupa, Grupe.Educator, Platitori.Nume, Platitori.CNP, Platitori.Plecat, Prezenta.ZilePrezenta, Prezenta.ValoareContract, Prezenta.ValoareTotala, Sum(IIf([tip]="2" And [anulata]=False,[xplati]![Plata],0)) AS Plata, Sum(IIf([tip]<>"2",[xplati].[plata],0)) AS Plati, CDbl(Nz([Suma],0)) AS Retur
FROM Platitori INNER JOIN (LunaD INNER JOIN (Grupe INNER JOIN ((Prezenta LEFT JOIN (SELECT Retur.IDP, Retur.IDZ, Retur.IDL, Sum(Retur.Suma) AS Suma FROM Retur GROUP BY Retur.IDP, Retur.IDZ, Retur.IDL, Retur.Anulat HAVING Retur.Anulat=False)  AS xRetur ON Prezenta.IDZ = xRetur.IDZ) LEFT JOIN (SELECT * FROM Plati WHERE Anulata = False)  AS xPlati ON Prezenta.IDZ = xPlati.IDZ) ON Grupe.IDG = Prezenta.IDG) ON LunaD.IDL = Prezenta.IDL) ON Platitori.IDP = Prezenta.IDP
GROUP BY Grupe.IDG, Platitori.IDP, LunaD.IDL, Prezenta.IDZ, LunaD.LunaT, LunaD.Anul, Grupe.Grupa, Grupe.Educator, Platitori.Nume, Platitori.CNP, Platitori.Plecat, Prezenta.ZilePrezenta, Prezenta.ValoareContract, Prezenta.ValoareTotala, CDbl(Nz([Suma],0))
HAVING (((Grupe.IDG) Like [vIDG]) And ((LunaD.IDL) Like Forms!Prezenta!IDL))
ORDER BY Platitori.IDP, Grupe.Grupa, Platitori.Nume, Platitori.CNP;
