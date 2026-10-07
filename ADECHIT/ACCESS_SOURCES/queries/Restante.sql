-- Query: Restante
-- Type: Select

SELECT Sum(X.VL) AS Restanta, X.IDP, X.IDL
FROM (SELECT Prezenta.IDP, Prezenta.IDL, Sum(Prezenta.valoaretotala) AS VL
FROM Prezenta
GROUP BY Prezenta.IDP, Prezenta.IDL

UNION ALL 

SELECT Plati.IDP, Plati.IDL, -1*Sum(Plati.plata) AS VL
FROM Plati
GROUP BY Plati.IDP, Plati.IDL
)  AS X
GROUP BY X.IDP, X.IDL;
