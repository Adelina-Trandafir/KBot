-- Query: Salvare_Lunara
-- Type: Append

INSERT INTO SS_Buget ( IDG, IDP, IDL, IDZ, Luna, Anul, Grupa, Educator, Nume, CNP, ZilePrezenta, SID, SIC, ValoareContract, ValoareTotala, Plata, Restanta, Compensare, Anticipat, Plati, Retur, SFD, SFC, Detalii, Plecat )
SELECT Situatie_buget.IDG, Situatie_buget.IDP, Situatie_buget.IDL, Situatie_buget.IDZ, Situatie_buget.Luna, Situatie_buget.Anul, Situatie_buget.Grupa, Situatie_buget.Educator, Situatie_buget.Nume, Situatie_buget.CNP, Situatie_buget.ZilePrezenta, Situatie_buget.SID, Situatie_buget.SIC, Situatie_buget.ValoareContract, Situatie_buget.ValoareTotala, Situatie_buget.Plata, Situatie_buget.Restanta, Situatie_buget.Compensare, Situatie_buget.Anticipat, Situatie_buget.Plati, Situatie_buget.Retur, Situatie_buget.SFD, Situatie_buget.SFC, Situatie_buget.Detalii, Situatie_buget.Plecat
FROM Situatie_buget;
