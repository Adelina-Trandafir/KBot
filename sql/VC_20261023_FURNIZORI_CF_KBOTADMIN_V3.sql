-- KBotAdmin -> Interogări unice -> Din fișier.
-- NAME: Portare_parteneri_codfiscal_v3
-- Use a NEW name: do not change Portare_parteneri_codfiscal or its recorded SQL.
-- Select the seven failed DCs (running on all original targets is also harmless).
-- AVACONT_SURSA is always included first by the runner.
-- Prerequisite: VC_20261023_FURNIZORI_CF_STRUCTURA.sql already applied.
-- The runner owns the transaction and the ledger. No BEGIN/COMMIT here.
-- No permanent DDL, no supplier creation, no guessed CF and no document reclassification.
-- Document values, quantities, versions and old identity/history columns stay unchanged.
-- User clarification: partners with Ascuns=1 have no NIRs or other documents.
-- Parteneri is never updated. Supplier lookups below only consider Ascuns<>1
-- (NULL is treated as 0). No special hidden-document handling is introduced.
-- Old partner IDs are read ONLY in this one-time transition to recover stored CF.
-- Online VERCON uses ONLY CF afterwards, with no ID/name/history fallback.
-- Each candidate is normalized BEFORE choosing the first available fiscal value.
-- Missing CF is allowed for unresolved historical documents; it stays NULL.
-- DONE means this conversion ran; it does NOT mean every document has a known supplier.
-- Missing-CF documents cannot be associated to eFactura until their CF is explicitly supplied.
-- The only blocking guards below are contradictions between KNOWN fiscal codes.
-- No inference from supplier names or words STOC/SOLD INITIAL.

DROP TEMPORARY TABLE IF EXISTS VC_CF_V3_NIR;
CREATE TEMPORARY TABLE VC_CF_V3_NIR AS
SELECT N.IdIntrare AS NIR_ID,
       CASE WHEN N.TipDoc='STOC_INITIAL' THEN NULL
            ELSE COALESCE(NULLIF((CASE WHEN UPPER(REGEXP_REPLACE(COALESCE(N.CodPartener,''),'[^[:alnum:]]','')) REGEXP '^RO[0-9]' THEN SUBSTRING(UPPER(REGEXP_REPLACE(COALESCE(N.CodPartener,''),'[^[:alnum:]]','')),3) ELSE UPPER(REGEXP_REPLACE(COALESCE(N.CodPartener,''),'[^[:alnum:]]','')) END),''),NULLIF((CASE WHEN UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal,''),'[^[:alnum:]]','')) REGEXP '^RO[0-9]' THEN SUBSTRING(UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal,''),'[^[:alnum:]]','')),3) ELSE UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal,''),'[^[:alnum:]]','')) END),''),NULLIF((CASE WHEN UPPER(REGEXP_REPLACE(COALESCE(N.CodFiscalSursa,''),'[^[:alnum:]]','')) REGEXP '^RO[0-9]' THEN SUBSTRING(UPPER(REGEXP_REPLACE(COALESCE(N.CodFiscalSursa,''),'[^[:alnum:]]','')),3) ELSE UPPER(REGEXP_REPLACE(COALESCE(N.CodFiscalSursa,''),'[^[:alnum:]]','')) END),'')) END AS CF
FROM VC_NIR N LEFT JOIN Parteneri P ON P.IdPartener=N.IdPartener AND COALESCE(P.Ascuns,0)<>1;

UPDATE VC_NIR N JOIN VC_CF_V3_NIR S ON S.NIR_ID=N.IdIntrare
SET N.CodPartener=S.CF
WHERE NOT (N.CodPartener <=> S.CF);

DROP TEMPORARY TABLE IF EXISTS VC_CF_V3_FACTURI;
CREATE TEMPORARY TABLE VC_CF_V3_FACTURI AS
SELECT F.ID AS FACTURA_ID, COALESCE(NULLIF((CASE WHEN UPPER(REGEXP_REPLACE(COALESCE(F.CodPartener,''),'[^[:alnum:]]','')) REGEXP '^RO[0-9]' THEN SUBSTRING(UPPER(REGEXP_REPLACE(COALESCE(F.CodPartener,''),'[^[:alnum:]]','')),3) ELSE UPPER(REGEXP_REPLACE(COALESCE(F.CodPartener,''),'[^[:alnum:]]','')) END),''),NULLIF((CASE WHEN UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal,''),'[^[:alnum:]]','')) REGEXP '^RO[0-9]' THEN SUBSTRING(UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal,''),'[^[:alnum:]]','')),3) ELSE UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal,''),'[^[:alnum:]]','')) END),''),NULLIF((CASE WHEN UPPER(REGEXP_REPLACE(COALESCE(L.CF,''),'[^[:alnum:]]','')) REGEXP '^RO[0-9]' THEN SUBSTRING(UPPER(REGEXP_REPLACE(COALESCE(L.CF,''),'[^[:alnum:]]','')),3) ELSE UPPER(REGEXP_REPLACE(COALESCE(L.CF,''),'[^[:alnum:]]','')) END),'')) AS CF
FROM VC_FACTURI F
LEFT JOIN Parteneri P ON P.IdPartener=F.PARTENER_ID AND COALESCE(P.Ascuns,0)<>1
LEFT JOIN (
  SELECT R.FACTURA_ID, MIN(NULLIF(N.CodPartener,'')) AS CF
  FROM VC_NIR_FACTURI R JOIN VC_NIR N
    ON N.SubunitId=R.SUBUNIT_ID AND N.GestiuneId=R.GESTIUNE_ID
   AND N.ExercitiuId=R.EXERCITIU_ID AND N.IdIntrare=R.NIR_ID
  GROUP BY R.FACTURA_ID
  HAVING COUNT(DISTINCT NULLIF(N.CodPartener,''))=1
) L ON L.FACTURA_ID=F.ID;

UPDATE VC_FACTURI F JOIN VC_CF_V3_FACTURI S ON S.FACTURA_ID=F.ID
SET F.CodPartener=S.CF
WHERE NOT (F.CodPartener <=> S.CF);

-- Deliberate duplicate-key guards: on contradiction the runner rolls back all
-- data changes in the current database and does NOT mark it completed.
DROP TEMPORARY TABLE IF EXISTS VC_CF_V3_GARDA;
CREATE TEMPORARY TABLE VC_CF_V3_GARDA (
  MOTIV VARCHAR(64) NOT NULL PRIMARY KEY
) ENGINE=InnoDB;
INSERT INTO VC_CF_V3_GARDA (MOTIV)
VALUES ('CF_FACTURA_DIFERIT_DE_NIR'),('FACTURA_CU_NIR_CF_DIFERITE');

INSERT INTO VC_CF_V3_GARDA (MOTIV)
SELECT 'CF_FACTURA_DIFERIT_DE_NIR' WHERE EXISTS (
  SELECT 1 FROM VC_FACTURI F JOIN VC_NIR_FACTURI R ON R.FACTURA_ID=F.ID
  JOIN VC_NIR N ON N.SubunitId=R.SUBUNIT_ID AND N.GestiuneId=R.GESTIUNE_ID
   AND N.ExercitiuId=R.EXERCITIU_ID AND N.IdIntrare=R.NIR_ID
  WHERE NULLIF(TRIM(F.CodPartener),'') IS NOT NULL
    AND NULLIF(TRIM(N.CodPartener),'') IS NOT NULL
    AND F.CodPartener<>N.CodPartener
);

INSERT INTO VC_CF_V3_GARDA (MOTIV)
SELECT 'FACTURA_CU_NIR_CF_DIFERITE' WHERE EXISTS (
  SELECT R.FACTURA_ID
  FROM VC_NIR_FACTURI R JOIN VC_NIR N
    ON N.SubunitId=R.SUBUNIT_ID AND N.GestiuneId=R.GESTIUNE_ID
   AND N.ExercitiuId=R.EXERCITIU_ID AND N.IdIntrare=R.NIR_ID
  GROUP BY R.FACTURA_ID
  HAVING COUNT(DISTINCT NULLIF(N.CodPartener,''))>1
);

DROP TEMPORARY TABLE VC_CF_V3_GARDA;
DROP TEMPORARY TABLE VC_CF_V3_FACTURI;
DROP TEMPORARY TABLE VC_CF_V3_NIR;
