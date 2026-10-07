-- Query: Update_Compensare
-- Type: Update

UPDATE Situatie_buget SET Situatie_buget.Compensare = IIf([Situatie_buget]![sfd]>0 And [Situatie_buget].[Compensare]>0,[Situatie_buget]![sic],[Situatie_buget]![Compensare]);
