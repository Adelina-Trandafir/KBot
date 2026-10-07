-- Query: Update_detalii
-- Type: Update

UPDATE Explicatie_Lunara INNER JOIN Situatie_buget ON Explicatie_Lunara.IDZ = Situatie_buget.IDZ SET Situatie_buget.Detalii = ConcatRelated("[Doc]","[Explicatie_Lunara]","[IDZ]=" & [Explicatie_Lunara]![IDZ],"",";");
