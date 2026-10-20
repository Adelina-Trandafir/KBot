-- =====================================================================================
-- ONE-TIME QUERY  0121_07_parteneri_coduri_in_angajamente        (slice 0121-07)
--
-- NOT run by hand. Paste this text into KBotAdmin, page «Interogari unice», name it
-- `0121_07_parteneri_coduri_in_angajamente`, tick the databases, press the preview and then run. The server
-- runs it once on AVACONT_SURSA and once on every ticked unit database, each in its own database (so the
-- table names below are NOT qualified), and writes it in each database's `Interogari_Unice`.
--
-- BEFORE it, in this order:
--   1. sql/P121_01_parteneri_angajamente.sql on AVACONT_SURSA (the two new tables);
--   2. the schema sync of the structure to the unit databases (KBotAdmin, «Sincronizare»);
--   3. the new Python files on the server (partner_links.py, parteneri_edit.py, ...), so that nobody adds
--      codes to the OLD table while this runs.
-- A database that does not have the two new tables FAILS (reported, left unmarked): sync it and run again.
--
-- WHAT IT DOES, on each database: copies the OLD «Coduri angajament» (Parteneri_Coduri, per unit and partner)
-- into the new tables, which are keyed by the supplier's normalized CodFiscal (slice 0121):
--   * Parteneri_Angajamente              one row per (CodFiscal, CodAng);
--   * Parteneri_Angajamente_Indicatori   one row per (link, CodAI = CodAng-CodInd), with ContBancar.
-- CodFiscal is the partner's, normalized like the DDF does it: letters and digits only, upper case, a leading
-- «RO» before a digit dropped (routes/efactura/primite_ubl.normalize_cui, EfCui.Normalize in the Migrator).
--
-- WHAT IT DOES NOT MOVE (the rows stay in Parteneri_Coduri, nothing is deleted; the counts are at the end):
--   * a row whose partner has no CodFiscal (nothing is left after normalization);
--   * a row with no CodAng (an angajament is the key of the link);
--   * the old IdClsf is not copied: CodAI already gives the classification (FX_Indicatori.IdClsf).
-- A row with a CodAng but no CodInd makes the link only, with no indicator.
--
-- SAFE TO RUN AGAIN, and safe over data the operator already entered in the new tables: links are inserted
-- only when missing; an indicator that exists keeps its CodInd / ContBancar and is filled only where empty.
-- Parteneri_Coduri is not read by K-BOT any more after slice 0121-03; it is dropped later, at the operator's word.
-- =====================================================================================

DROP TEMPORARY TABLE IF EXISTS p121_coduri;

CREATE TEMPORARY TABLE p121_coduri AS
SELECT n.cf, n.ang, n.ind, n.cont
  FROM (SELECT IF(u.c REGEXP '^RO[0-9]', SUBSTRING(u.c, 3), u.c) AS cf, u.ang, u.ind, u.cont
          FROM (SELECT UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal, ''), '[^A-Za-z0-9]', '')) AS c,
                       TRIM(COALESCE(K.CodAng, '')) AS ang,
                       TRIM(COALESCE(K.CodInd, '')) AS ind,
                       NULLIF(TRIM(COALESCE(K.ContBancar, '')), '') AS cont
                  FROM Parteneri_Coduri K
                  JOIN Parteneri P ON P.IdPartener = K.IdPartener) u) n
 WHERE n.cf <> '' AND n.ang <> '';

INSERT INTO Parteneri_Angajamente (CodFiscal, CodAng)
SELECT DISTINCT cf, ang FROM p121_coduri
ON DUPLICATE KEY UPDATE IdLegAng = IdLegAng;

INSERT INTO Parteneri_Angajamente_Indicatori (IdLegAng, CodAI, CodInd, ContBancar)
SELECT L.IdLegAng, CONCAT(c.ang, '-', c.ind), c.ind, c.cont
  FROM p121_coduri c
  JOIN Parteneri_Angajamente L ON L.CodFiscal = c.cf AND L.CodAng = c.ang
 WHERE c.ind <> ''
ON DUPLICATE KEY UPDATE
  CodInd = IF(CodInd IS NULL OR CodInd = '', VALUES(CodInd), CodInd),
  ContBancar = IF(ContBancar IS NULL OR ContBancar = '', VALUES(ContBancar), ContBancar);

DROP TEMPORARY TABLE p121_coduri;

-- =====================================================================================
-- CHECKS, by hand (HeidiSQL), on each unit database - they are comments here because a one-time query
-- returns no result set. Run them BEFORE and AFTER, and keep what they list for the rows that were not moved.
--
-- Rows of the old table, and how many cannot be moved:
--   SELECT COUNT(*) AS in_total,
--          SUM(TRIM(COALESCE(P.CodFiscal, '')) = '' OR UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal, ''), '[^A-Za-z0-9]', '')) = '') AS fara_cod_fiscal,
--          SUM(TRIM(COALESCE(K.CodAng, '')) = '') AS fara_codang,
--          SUM(TRIM(COALESCE(K.CodAng, '')) <> '' AND TRIM(COALESCE(K.CodInd, '')) = '') AS fara_codind
--     FROM Parteneri_Coduri K JOIN Parteneri P ON P.IdPartener = K.IdPartener;
--
-- The rows that stay behind, to see them:
--   SELECT P.IdUnitate, P.CodPartener, P.DenumirePartener, P.CodFiscal, K.CodAng, K.CodInd, K.ContBancar
--     FROM Parteneri_Coduri K JOIN Parteneri P ON P.IdPartener = K.IdPartener
--    WHERE TRIM(COALESCE(K.CodAng, '')) = ''
--       OR UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal, ''), '[^A-Za-z0-9]', '')) = ''
--    ORDER BY P.IdUnitate, P.CodPartener;
--
-- What the new tables hold:
--   SELECT (SELECT COUNT(*) FROM Parteneri_Angajamente) AS legaturi,
--          (SELECT COUNT(*) FROM Parteneri_Angajamente_Indicatori) AS indicatori;
-- =====================================================================================
