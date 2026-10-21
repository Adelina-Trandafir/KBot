-- =====================================================================================
-- ONE-TIME QUERY  0122_01_codfiscal_in_codpartener        (slice 0122-01)
--
-- NOT run by hand. Paste this text into KBotAdmin, page «Interogari unice», name it
-- `0122_01_codfiscal_in_codpartener`, tick the databases, press the preview and then run. The server runs it once on
-- AVACONT_SURSA and once on every ticked unit database, each in its own database (so the table names below are NOT
-- qualified), and writes it in each database's `Interogari_Unice`.
--
-- BEFORE it, in this order:
--   1. sql/P122_01_vc_nir_codpartener.sql on AVACONT_SURSA (the new column VC_NIR.CodPartener)
--   2. the schema sync of the structure to the unit databases (KBotAdmin, «Sincronizare»)
-- A database without that column FAILS on the last block (reported, left unmarked): sync it and run again.
-- Run it right before the new server files and the new K-BOT go out (see the plan), not weeks earlier.
--
-- WHAT IT DOES: from now on a supplier is identified ONLY by its CodFiscal (normalized: letters and digits, upper case,
-- a leading RO before a digit dropped). The column CodPartener holds that CodFiscal.
--   * FX_DDF_REV_SA, FX_DDF_REV_SB, FX_ORD_TBL: CodPartener := the normalized CodFiscal, IdPartener := NULL.
--     The CodFiscal is read from Parteneri through IdPartener where the row has one (rows that came from Access),
--     otherwise it is the CodPartener already there, normalized (rows written by K-BOT already hold the CodFiscal).
--   * VC_NIR: CodPartener := the normalized CodFiscal of its IdPartener. IdPartener is KEPT (Vercon still uses it).
--
-- WHAT IT DOES NOT TOUCH (the counts are in the checks at the end):
--   * a row whose IdPartener points at a partner WITHOUT a CodFiscal keeps its IdPartener and CodPartener as they are
--   * a row with no partner at all
--   * Parteneri itself and every other table
--
-- SAFE TO RUN AGAIN: a normalized CodFiscal stays the same when normalized again, and an IdPartener already NULL stays NULL.
-- Nothing is deleted. The old unit code of the partner is no longer stored on these rows (the partner is found by CodFiscal).
-- =====================================================================================

-- ---- FX_DDF_REV_SA ----------------------------------------------------------------
DROP TEMPORARY TABLE IF EXISTS p122_sa;

CREATE TEMPORARY TABLE p122_sa AS
SELECT x.id, IF(x.c REGEXP '^RO[0-9]', SUBSTRING(x.c, 3), x.c) AS cf
  FROM (SELECT R.IdSecA AS id,
               UPPER(REGEXP_REPLACE(COALESCE(IF(R.IdPartener IS NOT NULL, P.CodFiscal, R.CodPartener), ''), '[^A-Za-z0-9]', '')) AS c
          FROM FX_DDF_REV_SA R
          LEFT JOIN Parteneri P ON P.IdPartener = R.IdPartener) x
 WHERE x.c <> '';

UPDATE FX_DDF_REV_SA R JOIN p122_sa t ON t.id = R.IdSecA
   SET R.CodPartener = t.cf, R.IdPartener = NULL;

DROP TEMPORARY TABLE p122_sa;

-- ---- FX_DDF_REV_SB ----------------------------------------------------------------
DROP TEMPORARY TABLE IF EXISTS p122_sb;

CREATE TEMPORARY TABLE p122_sb AS
SELECT x.id, IF(x.c REGEXP '^RO[0-9]', SUBSTRING(x.c, 3), x.c) AS cf
  FROM (SELECT R.IdSecB AS id,
               UPPER(REGEXP_REPLACE(COALESCE(IF(R.IdPartener IS NOT NULL, P.CodFiscal, R.CodPartener), ''), '[^A-Za-z0-9]', '')) AS c
          FROM FX_DDF_REV_SB R
          LEFT JOIN Parteneri P ON P.IdPartener = R.IdPartener) x
 WHERE x.c <> '';

UPDATE FX_DDF_REV_SB R JOIN p122_sb t ON t.id = R.IdSecB
   SET R.CodPartener = t.cf, R.IdPartener = NULL;

DROP TEMPORARY TABLE p122_sb;

-- ---- FX_ORD_TBL -------------------------------------------------------------------
DROP TEMPORARY TABLE IF EXISTS p122_ord;

CREATE TEMPORARY TABLE p122_ord AS
SELECT x.id, IF(x.c REGEXP '^RO[0-9]', SUBSTRING(x.c, 3), x.c) AS cf
  FROM (SELECT R.IDORDTBLP AS id,
               UPPER(REGEXP_REPLACE(COALESCE(IF(R.IdPartener IS NOT NULL, P.CodFiscal, R.CodPartener), ''), '[^A-Za-z0-9]', '')) AS c
          FROM FX_ORD_TBL R
          LEFT JOIN Parteneri P ON P.IdPartener = R.IdPartener) x
 WHERE x.c <> '';

UPDATE FX_ORD_TBL R JOIN p122_ord t ON t.id = R.IDORDTBLP
   SET R.CodPartener = t.cf, R.IdPartener = NULL;

DROP TEMPORARY TABLE p122_ord;

-- ---- VC_NIR (needs the column from P122_01) ---------------------------------------
DROP TEMPORARY TABLE IF EXISTS p122_nir;

CREATE TEMPORARY TABLE p122_nir AS
SELECT x.id, IF(x.c REGEXP '^RO[0-9]', SUBSTRING(x.c, 3), x.c) AS cf
  FROM (SELECT N.IdIntrare AS id,
               UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal, ''), '[^A-Za-z0-9]', '')) AS c
          FROM VC_NIR N
          JOIN Parteneri P ON P.IdPartener = N.IdPartener) x
 WHERE x.c <> '';

UPDATE VC_NIR N JOIN p122_nir t ON t.id = N.IdIntrare
   SET N.CodPartener = t.cf;

DROP TEMPORARY TABLE p122_nir;

-- =====================================================================================
-- CHECKS, by hand (HeidiSQL), on each unit database - they are comments here because a one-time query returns no result set.
-- Run them BEFORE and AFTER.
--
-- Rows that still carry an IdPartener (their partner has no CodFiscal, they were not moved), per table:
--   SELECT 'SA' AS tabel, COUNT(*) AS ramase FROM FX_DDF_REV_SA WHERE IdPartener IS NOT NULL
--   UNION ALL SELECT 'SB', COUNT(*) FROM FX_DDF_REV_SB WHERE IdPartener IS NOT NULL
--   UNION ALL SELECT 'ORD_TBL', COUNT(*) FROM FX_ORD_TBL WHERE IdPartener IS NOT NULL
--   UNION ALL SELECT 'VC_NIR fara CodPartener', COUNT(*) FROM VC_NIR WHERE IdPartener IS NOT NULL AND CodPartener IS NULL;
--
-- Which partners those are (to give them a CodFiscal by hand, then run the query again):
--   SELECT DISTINCT P.IdUnitate, P.CodPartener, P.DenumirePartener, P.CodFiscal
--     FROM Parteneri P
--    WHERE P.IdPartener IN (SELECT IdPartener FROM FX_DDF_REV_SA WHERE IdPartener IS NOT NULL
--                           UNION SELECT IdPartener FROM FX_DDF_REV_SB WHERE IdPartener IS NOT NULL
--                           UNION SELECT IdPartener FROM FX_ORD_TBL WHERE IdPartener IS NOT NULL
--                           UNION SELECT IdPartener FROM VC_NIR WHERE IdPartener IS NOT NULL AND CodPartener IS NULL);
--
-- Rows with no IdPartener whose CodPartener is NOT the CodFiscal of any partner (an old unit code that lost its IdPartener?):
--   SELECT 'SA' AS tabel, R.CodPartener, COUNT(*) AS randuri
--     FROM FX_DDF_REV_SA R
--    WHERE R.IdPartener IS NULL AND COALESCE(R.CodPartener, '') <> ''
--      AND NOT EXISTS (SELECT 1 FROM Parteneri P
--                       WHERE UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal, ''), '[^A-Za-z0-9]', '')) IN (R.CodPartener, CONCAT('RO', R.CodPartener)))
--    GROUP BY R.CodPartener;
--   (the same for FX_DDF_REV_SB and FX_ORD_TBL)
--
-- How the suppliers are spread now (one CodFiscal on several Parteneri rows is normal, several CodPartener per supplier is not):
--   SELECT CodPartener, COUNT(*) FROM FX_DDF_REV_SA WHERE COALESCE(CodPartener, '') <> '' GROUP BY CodPartener ORDER BY 2 DESC LIMIT 50;
-- =====================================================================================
