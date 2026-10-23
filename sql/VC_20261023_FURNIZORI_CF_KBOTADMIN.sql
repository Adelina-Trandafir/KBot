-- KBotAdmin -> Interogări unice -> Din fișier.
-- NAME: VC_20261023_FURNIZORI_CF_KBOTADMIN
-- FIRST run VC_20261023_FURNIZORI_CF_STRUCTURA on the SAME checked databases
-- (or run the structure on AVACONT_SURSA and synchronize it to those databases).
-- The runner always includes AVACONT_SURSA first, then the checked DCs.
-- It owns the transaction and the Interogari_Unice ledger; no BEGIN/COMMIT here.
-- No schema changes on permanent tables. Only temporary work tables are created.
-- Pause online writes while migrating, then deploy the new VERCON online files.
-- Old IDs are only read to recover existing fiscal codes during this transition.
-- Existing identity columns, document amounts/quantities/versions remain unchanged.
-- Do not change this text/name after a successful run: the ledger identifies both.

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


-- KBotAdmin discards SELECT result sets. Stop the current database instead of
-- marking it DONE while fiscal identities are unresolved or inconsistent.
-- The guard deliberately inserts an existing primary key when a condition fails;
-- duplicate-key errors are not dependent on CHECK enforcement or SQL strict mode.
-- The error text identifies the condition. The runner rolls back ALL data changes
-- in this database, leaves it unmarked and reports the failure. Correct the data
-- and repeat the SAME query; already completed databases are skipped.
-- NIR_FARA_CF: a non-opening-stock receipt has no fiscal code to migrate.
-- FACTURA_FARA_CF: an unmerged supplier invoice has no fiscal code to migrate.
-- CF_FACTURA_DIFERIT_DE_NIR: an invoice and a linked receipt disagree.
DROP TEMPORARY TABLE IF EXISTS VC_CF_MIGRARE_GARDA;
CREATE TEMPORARY TABLE VC_CF_MIGRARE_GARDA (
  MOTIV VARCHAR(64) NOT NULL PRIMARY KEY
) ENGINE=InnoDB;
INSERT INTO VC_CF_MIGRARE_GARDA (MOTIV)
VALUES ('NIR_FARA_CF'),('FACTURA_FARA_CF'),('CF_FACTURA_DIFERIT_DE_NIR');

INSERT INTO VC_CF_MIGRARE_GARDA (MOTIV)
SELECT 'NIR_FARA_CF' WHERE EXISTS (
  SELECT 1 FROM VC_NIR N WHERE N.TipDoc<>'STOC_INITIAL'
  AND NULLIF(TRIM(N.CodPartener),'') IS NULL
);

INSERT INTO VC_CF_MIGRARE_GARDA (MOTIV)
SELECT 'FACTURA_FARA_CF' WHERE EXISTS (
  SELECT 1 FROM VC_FACTURI F WHERE F.REUNITA_IN IS NULL
  AND NULLIF(TRIM(F.CodPartener),'') IS NULL
);

INSERT INTO VC_CF_MIGRARE_GARDA (MOTIV)
SELECT 'CF_FACTURA_DIFERIT_DE_NIR' WHERE EXISTS (
  SELECT 1 FROM VC_FACTURI F JOIN VC_NIR_FACTURI R ON R.FACTURA_ID=F.ID
  JOIN VC_NIR N ON N.SubunitId=R.SUBUNIT_ID AND N.GestiuneId=R.GESTIUNE_ID
   AND N.ExercitiuId=R.EXERCITIU_ID AND N.IdIntrare=R.NIR_ID
  WHERE NOT (F.CodPartener <=> N.CodPartener)
);

DROP TEMPORARY TABLE VC_CF_MIGRARE_GARDA;
DROP TEMPORARY TABLE VC_CF_FACTURI_DATE;
DROP TEMPORARY TABLE VC_CF_NIR_DATE;
