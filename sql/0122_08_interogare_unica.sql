-- =====================================================================================
-- ONE-TIME QUERY  0122_08_parteneri_nefolosite_ascunse        (slice 0122-08)
--
-- HIDES (Ascuns = 1) every row of Parteneri that nothing uses - neither K-BOT nor VERCON. NOTHING IS DELETED.
-- NOT run by hand. KBotAdmin, page «Interogari unice», name `0122_08_parteneri_nefolosite_ascunse`.
--
-- DO THIS FIRST:
--   1. sql/0122_08_parteneri_nefolosite_PREVIEW.sql in HeidiSQL on the database: it lists what would be hidden, changes nothing
--   2. run this query on ONE database (for example 000_DEMO), look at it in the application, only then tick the others
-- Have the one-time queries 0122_01 and 0122_01b already done on it, and the Migrator already run for the last unit
-- (the Migrator can bring partners back from Access).
--
-- WHAT COUNTS AS «USED». A Parteneri row is KEPT when ANY of these holds, and HIDDEN (Ascuns = 1) only when none does:
--   1. its IdPartener is on a DDF section A / B row, an ORD table row, a VC_NIR receipt or a VC_FACTURI invoice (the old keys)
--   2. its code in the unit (IdUnitate + CodPartener) is on a DDF section A / B row, an ORD table row or a bank statement row (FX_Extrase)
--   3. its CodFiscal (normalized: letters and digits, upper case, a leading RO before a digit dropped) appears as the supplier of:
--      a DDF (header or partner list), an ORD (beneficiary or table row), a VC_NIR receipt (CodPartener or CodFiscalSursa),
--      a VC_FACTURI invoice, a link supplier - angajament (Parteneri_Angajamente), a received e-invoice (EF_Primite)
--      or an e-invoice product correlation (EF_VERCON_CORELARI)
--   Tables or columns that do not exist in a database (for example VC_FACTURI.CodPartener before its structure script) hold no usage and are skipped.
--   Because the test is by CodFiscal, ALL rows of a used CodFiscal stay (the same supplier in several units): this query hides
--   what nothing uses, it does not merge duplicates.
--
-- WHAT «HIDDEN» MEANS: the partner is no longer listed in the window «Parteneri», offered in the DDF partner combo or in Vercon for a
-- new receipt. It stays in the database with all its data. Only rows with Ascuns empty or 0 are touched: a row already hidden stays as it is.
-- UNDO: the window «Parteneri» can show the hidden ones, and a row is un-hidden by setting Ascuns back to 0. The rows hidden by a run
-- are exactly the ones its PREVIEW listed (their DataModificare is also stamped with the time of the run).
-- All the reading is done first, the UPDATE is the LAST statement. SAFE TO RUN AGAIN: a second run finds nothing more to hide.
-- =====================================================================================

DROP TEMPORARY TABLE IF EXISTS p122d_raw;
DROP TEMPORARY TABLE IF EXISTS p122d_id;
DROP TEMPORARY TABLE IF EXISTS p122d_uc;
DROP TEMPORARY TABLE IF EXISTS p122d_cf;
DROP TEMPORARY TABLE IF EXISTS p122d_p;

CREATE TEMPORARY TABLE p122d_raw (v varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL);
CREATE TEMPORARY TABLE p122d_id (id int(11) NOT NULL, KEY (id));
CREATE TEMPORARY TABLE p122d_uc (IdUnitate int(11) NULL, cod varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL, KEY (IdUnitate, cod));

-- ---- 1. used BY ROW ID (the old foreign keys: such a partner is in use) ----
INSERT INTO p122d_id (id) SELECT IdPartener FROM FX_DDF_REV_SA WHERE IdPartener IS NOT NULL;
INSERT INTO p122d_id (id) SELECT IdPartener FROM FX_DDF_REV_SB WHERE IdPartener IS NOT NULL;
INSERT INTO p122d_id (id) SELECT IdPartener FROM FX_ORD_TBL WHERE IdPartener IS NOT NULL;
INSERT INTO p122d_id (id) SELECT IdPartener FROM VC_NIR WHERE IdPartener IS NOT NULL;
SET @p122d_sql = (SELECT IF(COUNT(*) = 1, 'INSERT INTO p122d_id (id) SELECT PARTENER_ID FROM VC_FACTURI WHERE PARTENER_ID IS NOT NULL', 'DO 0') FROM information_schema.COLUMNS
                          WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'VC_FACTURI' AND COLUMN_NAME = 'PARTENER_ID');
PREPARE p122d_st FROM @p122d_sql;
EXECUTE p122d_st;
DEALLOCATE PREPARE p122d_st;

-- ---- 2. used BY UNIT CODE (rows that still name the partner by its code in the unit, and the bank statements) ----
INSERT INTO p122d_uc (IdUnitate, cod) SELECT IdUnitate, CodPartener FROM FX_DDF_REV_SA WHERE CodPartener IS NOT NULL;
INSERT INTO p122d_uc (IdUnitate, cod) SELECT IdUnitate, CodPartener FROM FX_DDF_REV_SB WHERE CodPartener IS NOT NULL;
INSERT INTO p122d_uc (IdUnitate, cod) SELECT IdUnitate, CodPartener FROM FX_ORD_TBL WHERE CodPartener IS NOT NULL;
INSERT INTO p122d_uc (IdUnitate, cod) SELECT IdUnitate, CodPartener FROM FX_Extrase WHERE CodPartener IS NOT NULL;

-- ---- 3. used BY CODFISCAL (any spelling, normalized below) ----
INSERT INTO p122d_raw (v) SELECT CodFiscal FROM FX_DDF WHERE CodFiscal IS NOT NULL;
INSERT INTO p122d_raw (v) SELECT CodFiscal FROM FX_DDF_Parteneri WHERE CodFiscal IS NOT NULL;
INSERT INTO p122d_raw (v) SELECT CodFiscal FROM FX_ORD_PART WHERE CodFiscal IS NOT NULL;
INSERT INTO p122d_raw (v) SELECT CodPartener FROM FX_DDF_REV_SA WHERE CodPartener IS NOT NULL;
INSERT INTO p122d_raw (v) SELECT CodPartener FROM FX_DDF_REV_SB WHERE CodPartener IS NOT NULL;
INSERT INTO p122d_raw (v) SELECT CodPartener FROM FX_ORD_TBL WHERE CodPartener IS NOT NULL;
INSERT INTO p122d_raw (v) SELECT CodPartener FROM VC_NIR WHERE CodPartener IS NOT NULL;
INSERT INTO p122d_raw (v) SELECT CodFiscalSursa FROM VC_NIR WHERE CodFiscalSursa IS NOT NULL;
INSERT INTO p122d_raw (v) SELECT CodFiscal FROM Parteneri_Angajamente WHERE CodFiscal IS NOT NULL;
SET @p122d_sql = (SELECT IF(COUNT(*) = 1, 'INSERT INTO p122d_raw (v) SELECT CodPartener FROM VC_FACTURI WHERE CodPartener IS NOT NULL', 'DO 0') FROM information_schema.COLUMNS
                          WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'VC_FACTURI' AND COLUMN_NAME = 'CodPartener');
PREPARE p122d_st FROM @p122d_sql;
EXECUTE p122d_st;
DEALLOCATE PREPARE p122d_st;

SET @p122d_sql = (SELECT IF(COUNT(*) = 1, 'INSERT INTO p122d_raw (v) SELECT CuiNormalizat FROM EF_Primite WHERE CuiNormalizat IS NOT NULL', 'DO 0') FROM information_schema.COLUMNS
                          WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'EF_Primite' AND COLUMN_NAME = 'CuiNormalizat');
PREPARE p122d_st FROM @p122d_sql;
EXECUTE p122d_st;
DEALLOCATE PREPARE p122d_st;

SET @p122d_sql = (SELECT IF(COUNT(*) = 1, 'INSERT INTO p122d_raw (v) SELECT CUI FROM EF_VERCON_CORELARI WHERE CUI IS NOT NULL', 'DO 0') FROM information_schema.COLUMNS
                          WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'EF_VERCON_CORELARI' AND COLUMN_NAME = 'CUI');
PREPARE p122d_st FROM @p122d_sql;
EXECUTE p122d_st;
DEALLOCATE PREPARE p122d_st;

CREATE TEMPORARY TABLE p122d_cf AS
SELECT DISTINCT IF(x.c REGEXP '^RO[0-9]', SUBSTRING(x.c, 3), x.c) AS cf
  FROM (SELECT UPPER(REGEXP_REPLACE(COALESCE(v, ''), '[^A-Za-z0-9]', '')) AS c FROM p122d_raw) x
 WHERE x.c <> '';

-- ---- the normalized CodFiscal of every Parteneri row ----
CREATE TEMPORARY TABLE p122d_p AS
SELECT y.IdPartener, IF(y.c REGEXP '^RO[0-9]', SUBSTRING(y.c, 3), y.c) AS cf
  FROM (SELECT IdPartener, UPPER(REGEXP_REPLACE(COALESCE(CodFiscal, ''), '[^A-Za-z0-9]', '')) AS c FROM Parteneri) y;

-- ---- the hiding (LAST statement) ----
UPDATE Parteneri P
  JOIN p122d_p x ON x.IdPartener = P.IdPartener
  LEFT JOIN p122d_id i ON i.id = P.IdPartener
  LEFT JOIN p122d_cf c ON c.cf = x.cf AND x.cf <> ''
  LEFT JOIN p122d_uc u ON u.IdUnitate = P.IdUnitate AND u.cod = P.CodPartener
   SET P.Ascuns = 1
 WHERE COALESCE(P.Ascuns, 0) = 0 AND i.id IS NULL AND c.cf IS NULL AND u.cod IS NULL;

DROP TEMPORARY TABLE p122d_raw;
DROP TEMPORARY TABLE p122d_id;
DROP TEMPORARY TABLE p122d_uc;
DROP TEMPORARY TABLE p122d_cf;
DROP TEMPORARY TABLE p122d_p;
