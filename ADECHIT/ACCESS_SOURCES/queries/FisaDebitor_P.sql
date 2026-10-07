-- Query: FisaDebitor_P
-- Type: Select

SELECT Platitori.IDP, Platitori.IDG, PS.IDS, Platitori.Nume, Platitori.Adresa, Platitori.CNP, Platitori.Frate, PS.Platitori_sub.Nume, PS.Platitori_sub.Adresa, PS.Platitori_sub.CUI, PS.Platitori_sub.J, PS.Platitori_sub.Cont, PS.Platitori_sub.Banca, PS.Platitori_sub.EMail, PS.Platitori_sub.TrimiteMail
FROM Platitori INNER JOIN (SELECT * FROM Platitori_sub WHERE Activ=True)  AS PS ON Platitori.IDP = PS.IDP;
