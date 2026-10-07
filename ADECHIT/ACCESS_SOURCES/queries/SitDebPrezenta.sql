-- Query: SitDebPrezenta
-- Type: Select

SELECT X.IDZ, X.IDP, CDbl(IIf([SID]-[sic]+[VALOARE]-[platita]>0,[SID]-[sic]+[VALOARE]-[platita],0)) AS SFD, CDbl(IIf([SID]-[sic]+[VALOARE]-[platita]<0,[SID]-[sic]+[VALOARE]-[platita],0)) AS SFC, X.Platita, X.Retur
FROM (SELECT Prezenta.IDZ, Prezenta.IDP, CDbl(Nz(IIf([SI]<0,0,[si]),0)) AS SID, Abs(CDbl(Nz(IIf([SI]>0,0,[si])))) AS SIC, Prezenta.ValoareContract, Prezenta.ValoareTotala AS Valoare, Sum(CDbl(Nz([Plata],0))) AS Platita, Sum(CDbl(Nz([retur]![Suma],0))) AS Retur FROM SoldInitial RIGHT JOIN ((Prezenta LEFT JOIN Retur ON Prezenta.IDZ = Retur.IDZ) LEFT JOIN Plati ON Prezenta.IDZ = Plati.IDZ) ON SoldInitial.IDP = Prezenta.IDP WHERE (((Prezenta.IDG)=[forms]![Prezenta]![IDG])) GROUP BY Prezenta.IDZ, Prezenta.IDP, CDbl(Nz(IIf([SI]<0,0,[si]),0)), Abs(CDbl(Nz(IIf([SI]>0,0,[si])))), Prezenta.ValoareContract, Prezenta.ValoareTotala)  AS X
GROUP BY X.IDZ, X.IDP, CDbl(IIf([SID]-[sic]+[VALOARE]-[platita]>0,[SID]-[sic]+[VALOARE]-[platita],0)), CDbl(IIf([SID]-[sic]+[VALOARE]-[platita]<0,[SID]-[sic]+[VALOARE]-[platita],0)), X.Platita, X.Retur;
