-- MANUAL DATA MIGRATION, VERCON ONLINE ONLY. Do not execute from application code.
-- Prerequisite: VC_20261023_FURNIZORI_CF_STRUCTURA.sql and structure synchronization.
-- Run once per DC, with online writes paused. Safe to repeat: populated fiscal codes
-- take precedence. Old ID columns are READ ONLY HERE to migrate existing data;
-- the new online code never reads/writes supplier identities through those IDs.
-- Existing IDs, amounts, quantities, document versions and old columns are preserved.
-- No supplier is inferred from a NAME. New online writes always store CF directly.

START TRANSACTION;

DROP TEMPORARY TABLE IF EXISTS VC_CF_NIR_DATE;
CREATE TEMPORARY TABLE VC_CF_NIR_DATE AS
SELECT N.IdIntrare AS NIR_ID,
       CASE WHEN N.TipDoc='STOC_INITIAL' THEN NULL
            ELSE NULLIF((CASE WHEN UPPER(REGEXP_REPLACE(COALESCE(COALESCE(NULLIF(TRIM(N.CodPartener),''),NULLIF(TRIM(P.CodFiscal),''),NULLIF(TRIM(N.CodFiscalSursa),'')),''),'[^[:alnum:]]','')) REGEXP '^RO[0-9]' THEN SUBSTRING(UPPER(REGEXP_REPLACE(COALESCE(COALESCE(NULLIF(TRIM(N.CodPartener),''),NULLIF(TRIM(P.CodFiscal),''),NULLIF(TRIM(N.CodFiscalSursa),'')),''),'[^[:alnum:]]','')),3) ELSE UPPER(REGEXP_REPLACE(COALESCE(COALESCE(NULLIF(TRIM(N.CodPartener),''),NULLIF(TRIM(P.CodFiscal),''),NULLIF(TRIM(N.CodFiscalSursa),'')),''),'[^[:alnum:]]','')) END),'') END AS CF
FROM VC_NIR N LEFT JOIN Parteneri P ON P.IdPartener=N.IdPartener;

UPDATE VC_NIR N JOIN VC_CF_NIR_DATE S ON S.NIR_ID=N.IdIntrare
SET N.CodPartener=S.CF
WHERE NOT (N.CodPartener <=> S.CF);

DROP TEMPORARY TABLE IF EXISTS VC_CF_FACTURI_DATE;
CREATE TEMPORARY TABLE VC_CF_FACTURI_DATE AS
SELECT F.ID AS FACTURA_ID, NULLIF((CASE WHEN UPPER(REGEXP_REPLACE(COALESCE(COALESCE(NULLIF(TRIM(F.CodPartener),''),NULLIF(TRIM(P.CodFiscal),''),L.CF),''),'[^[:alnum:]]','')) REGEXP '^RO[0-9]' THEN SUBSTRING(UPPER(REGEXP_REPLACE(COALESCE(COALESCE(NULLIF(TRIM(F.CodPartener),''),NULLIF(TRIM(P.CodFiscal),''),L.CF),''),'[^[:alnum:]]','')),3) ELSE UPPER(REGEXP_REPLACE(COALESCE(COALESCE(NULLIF(TRIM(F.CodPartener),''),NULLIF(TRIM(P.CodFiscal),''),L.CF),''),'[^[:alnum:]]','')) END),'') AS CF
FROM VC_FACTURI F
LEFT JOIN Parteneri P ON P.IdPartener=F.PARTENER_ID
LEFT JOIN (
  SELECT R.FACTURA_ID, MIN(NULLIF(N.CodPartener,'')) AS CF
  FROM VC_NIR_FACTURI R JOIN VC_NIR N
    ON N.SubunitId=R.SUBUNIT_ID AND N.GestiuneId=R.GESTIUNE_ID
   AND N.ExercitiuId=R.EXERCITIU_ID AND N.IdIntrare=R.NIR_ID
  GROUP BY R.FACTURA_ID
  HAVING COUNT(DISTINCT NULLIF(N.CodPartener,''))=1
) L ON L.FACTURA_ID=F.ID;

UPDATE VC_FACTURI F JOIN VC_CF_FACTURI_DATE S ON S.FACTURA_ID=F.ID
SET F.CodPartener=S.CF
WHERE NOT (F.CodPartener <=> S.CF);

DROP TEMPORARY TABLE VC_CF_FACTURI_DATE;
DROP TEMPORARY TABLE VC_CF_NIR_DATE;

COMMIT;

-- REVIEW THESE RESULT SETS BEFORE PUBLICATION. Missing/conflicting fiscal codes
-- must be resolved explicitly; the application never falls back to an internal ID.
SELECT N.SubunitId,N.GestiuneId,N.ExercitiuId,N.IdIntrare,N.Numar,N.NrFact,N.FurnizorV,
       N.CodPartener,N.CodFiscalSursa
FROM VC_NIR N WHERE N.TipDoc<>'STOC_INITIAL' AND NULLIF(TRIM(N.CodPartener),'') IS NULL;

SELECT F.ID,F.NUMAR,F.DATA,F.CodPartener
FROM VC_FACTURI F WHERE F.REUNITA_IN IS NULL AND NULLIF(TRIM(F.CodPartener),'') IS NULL;

SELECT F.ID,F.NUMAR,F.CodPartener AS CF_FACTURA,N.IdIntrare,N.CodPartener AS CF_NIR
FROM VC_FACTURI F JOIN VC_NIR_FACTURI R ON R.FACTURA_ID=F.ID
JOIN VC_NIR N ON N.SubunitId=R.SUBUNIT_ID AND N.GestiuneId=R.GESTIUNE_ID
 AND N.ExercitiuId=R.EXERCITIU_ID AND N.IdIntrare=R.NIR_ID
WHERE NOT (F.CodPartener <=> N.CodPartener);
