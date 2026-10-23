-- KBotAdmin -> Interogări unice -> Din fișier.
-- NAME: Portare_parteneri_codfiscal_diagnostic
-- Select ONLY the seven failed DCs from the migration log.
-- AVACONT_SURSA is included automatically by KBotAdmin.
-- This diagnostic does NOT update permanent business tables.
-- The runner consumes SELECT output; an intentional duplicate-key error
-- carries the diagnostic summary into the existing KBotAdmin log instead.
-- DIAG_CF reports: N=missing CF under the exact migration rule;
-- FMT=codes hidden by a higher-priority value made only of punctuation;
-- STOC=missing-CF documents whose number/name mentions STOC or SOLD INITIAL
-- (clue only, NOT an automatic reclassification);
-- EX=up to three examples IdIntrare/Numar@SubunitId:GestiuneId:ExercitiuId.
-- Additional flags on the first example: P=0 missing old partner row,
-- P=1 row exists; SRC=1 source CF normalizes to a nonempty value.
-- All diagnostic work tables are temporary; on a diagnostic error the runner
-- rolls back, does not mark this database DONE, and continues to the next DC.
-- If no missing CF exists, the diagnostic succeeds and is recorded normally.

DROP TEMPORARY TABLE IF EXISTS VC_CF_DIAG_NIR;
CREATE TEMPORARY TABLE VC_CF_DIAG_NIR AS
SELECT N.IdIntrare AS NIR_ID,N.Numar AS NUMAR,N.SubunitId AS SUBUNIT_ID,
       N.GestiuneId AS GESTIUNE_ID,N.ExercitiuId AS EXERCITIU_ID,
       CASE WHEN P.IdPartener IS NULL THEN 0 ELSE 1 END AS PARTENER_GASIT,
       CASE WHEN NULLIF((CASE WHEN UPPER(REGEXP_REPLACE(COALESCE(N.CodFiscalSursa,''),'[^[:alnum:]]','')) REGEXP '^RO[0-9]' THEN SUBSTRING(UPPER(REGEXP_REPLACE(COALESCE(N.CodFiscalSursa,''),'[^[:alnum:]]','')),3) ELSE UPPER(REGEXP_REPLACE(COALESCE(N.CodFiscalSursa,''),'[^[:alnum:]]','')) END),'') IS NULL THEN 0 ELSE 1 END AS ARE_CF_SURSA,
       NULLIF((CASE WHEN UPPER(REGEXP_REPLACE(COALESCE(COALESCE(NULLIF(TRIM(N.CodPartener),''),NULLIF(TRIM(P.CodFiscal),''),NULLIF(TRIM(N.CodFiscalSursa),'')),''),'[^[:alnum:]]','')) REGEXP '^RO[0-9]' THEN SUBSTRING(UPPER(REGEXP_REPLACE(COALESCE(COALESCE(NULLIF(TRIM(N.CodPartener),''),NULLIF(TRIM(P.CodFiscal),''),NULLIF(TRIM(N.CodFiscalSursa),'')),''),'[^[:alnum:]]','')),3) ELSE UPPER(REGEXP_REPLACE(COALESCE(COALESCE(NULLIF(TRIM(N.CodPartener),''),NULLIF(TRIM(P.CodFiscal),''),NULLIF(TRIM(N.CodFiscalSursa),'')),''),'[^[:alnum:]]','')) END),'') AS CF_MIGRARE,
       COALESCE(NULLIF((CASE WHEN UPPER(REGEXP_REPLACE(COALESCE(N.CodPartener,''),'[^[:alnum:]]','')) REGEXP '^RO[0-9]' THEN SUBSTRING(UPPER(REGEXP_REPLACE(COALESCE(N.CodPartener,''),'[^[:alnum:]]','')),3) ELSE UPPER(REGEXP_REPLACE(COALESCE(N.CodPartener,''),'[^[:alnum:]]','')) END),''),NULLIF((CASE WHEN UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal,''),'[^[:alnum:]]','')) REGEXP '^RO[0-9]' THEN SUBSTRING(UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal,''),'[^[:alnum:]]','')),3) ELSE UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal,''),'[^[:alnum:]]','')) END),''),NULLIF((CASE WHEN UPPER(REGEXP_REPLACE(COALESCE(N.CodFiscalSursa,''),'[^[:alnum:]]','')) REGEXP '^RO[0-9]' THEN SUBSTRING(UPPER(REGEXP_REPLACE(COALESCE(N.CodFiscalSursa,''),'[^[:alnum:]]','')),3) ELSE UPPER(REGEXP_REPLACE(COALESCE(N.CodFiscalSursa,''),'[^[:alnum:]]','')) END),'')) AS CF_NORMALIZAT,
       CASE WHEN UPPER(COALESCE(N.NrFact,'')) LIKE '%STOC%'
              OR UPPER(COALESCE(N.FurnizorV,'')) LIKE '%STOC%'
              OR UPPER(COALESCE(N.NrFact,'')) LIKE '%SOLD INITIAL%'
              OR UPPER(COALESCE(N.FurnizorV,'')) LIKE '%SOLD INITIAL%'
            THEN 1 ELSE 0 END AS INDICIU_STOC
FROM VC_NIR N LEFT JOIN Parteneri P ON P.IdPartener=N.IdPartener
WHERE N.TipDoc<>'STOC_INITIAL';

SET @VC_CF_DIAG_MSG = (
  SELECT CONCAT('DIAG_CF N=',COUNT(*),';FMT=',COALESCE(SUM(CF_NORMALIZAT IS NOT NULL),0),
                ';STOC=',COALESCE(SUM(INDICIU_STOC),0),';EX=',
                COALESCE(SUBSTRING_INDEX(GROUP_CONCAT(CONCAT(NIR_ID,'/',COALESCE(NUMAR,0),'@',SUBUNIT_ID,':',GESTIUNE_ID,':',EXERCITIU_ID,
                  '(P',PARTENER_GASIT,',SRC',ARE_CF_SURSA,')') ORDER BY SUBUNIT_ID,GESTIUNE_ID,NUMAR SEPARATOR '|'),'|',3),'none'))
  FROM VC_CF_DIAG_NIR WHERE CF_MIGRARE IS NULL
);

DROP TEMPORARY TABLE IF EXISTS VC_CF_DIAG_MESAJ;
CREATE TEMPORARY TABLE VC_CF_DIAG_MESAJ (
  MESAJ VARCHAR(191) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL PRIMARY KEY
) ENGINE=InnoDB;
INSERT INTO VC_CF_DIAG_MESAJ (MESAJ) VALUES (LEFT(@VC_CF_DIAG_MSG,191));
INSERT INTO VC_CF_DIAG_MESAJ (MESAJ)
SELECT LEFT(@VC_CF_DIAG_MSG,191) WHERE EXISTS (
  SELECT 1 FROM VC_CF_DIAG_NIR WHERE CF_MIGRARE IS NULL
);

DROP TEMPORARY TABLE VC_CF_DIAG_MESAJ;
DROP TEMPORARY TABLE VC_CF_DIAG_NIR;
