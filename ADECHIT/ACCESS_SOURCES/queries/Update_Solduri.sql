-- Query: Update_Solduri
-- Type: Update

UPDATE Situatie_buget LEFT JOIN Solduri_Lunare ON Situatie_buget.IDP = Solduri_Lunare.IDP SET Situatie_buget.SoldInitial = Nz([Solduri_Lunare]![Sold],0), Situatie_buget.SID = [Solduri_Lunare]![SID], Situatie_buget.SIC = [Solduri_Lunare]![SIC];
