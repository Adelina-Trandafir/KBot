-- Query: FisaDebitor_D
-- Type: Select

SELECT Prezenta.IDP, Prezenta_sub.IDZ, Prezenta_sub.DI, Prezenta_sub.DSF, Prezenta_sub.NrZile, Prezenta_sub.Absent
FROM (LunaD INNER JOIN Prezenta ON LunaD.IDL = Prezenta.IDL) INNER JOIN Prezenta_sub ON Prezenta.IDZ = Prezenta_sub.IDZ
WHERE (((dhLastDayInMonth(CDate("01/" & [luna] & "/" & [anul])))<=dhLastDayInMonth(CDate("01/" & [forms]![prezenta]![luni]![luna] & "/" & [forms]![prezenta]![luni]![anul]))));
