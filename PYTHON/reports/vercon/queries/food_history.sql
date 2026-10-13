SELECT IdLista,NumarLA,DATE(DATALA) AS date,DEP AS difference,ValoareLista AS value
FROM {{TABLE:VC_LABC}} WHERE Anulat=0 ORDER BY DATALA,IdLista
