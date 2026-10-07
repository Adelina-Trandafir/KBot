-- Query: CNPPROST
-- Type: Update

UPDATE Platitori INNER JOIN (Platitori_sub INNER JOIN Delegati ON Platitori_sub.IDS = Delegati.IDS) ON Platitori.IDP = Platitori_sub.IDP SET Platitori.CNP = [Delegati]![CNP];
