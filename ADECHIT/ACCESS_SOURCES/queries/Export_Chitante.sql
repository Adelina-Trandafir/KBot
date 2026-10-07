-- Query: Export_Chitante
-- Type: Append

INSERT INTO tmpExport_Chitante ( IDP, IDS, IDL, IDG, IDPL, IDZ, LA, Inchisa, Grupa, Nume, Adresa, CNP, ZilePrezenta, ValoareContract, Data, Serie, Numar, Explicatie, Valoare )
SELECT Platitori_sub.IDP, Platitori_sub.IDS, Prezenta.IDL, Grupe.IDG, Plati.IDPL, Prezenta.IDZ, LunaD.LA, LunaD.Inchisa, Grupe.Grupa, Platitori_sub.Nume, Platitori_sub.Adresa, Platitori.CNP, Prezenta.ZilePrezenta, Prezenta.ValoareContract, Chitante.Data, Chitante.Serie, Chitante.Numar, Chitante.Explicatie, Chitante.Valoare
FROM (LunaD INNER JOIN (Prezenta INNER JOIN Grupe ON Prezenta.IDG = Grupe.IDG) ON LunaD.IDL = Prezenta.IDL) INNER JOIN (Platitori INNER JOIN ((Platitori_sub INNER JOIN Plati ON Platitori_sub.IDS = Plati.IDS) INNER JOIN Chitante ON Plati.IDPL = Chitante.IDPL) ON Platitori.IDP = Platitori_sub.IDP) ON Prezenta.IDZ = Plati.IDZ
WHERE (((Prezenta.IDL)=[Forms]![Prezenta]![IDL]))
ORDER BY Chitante.Numar DESC;
