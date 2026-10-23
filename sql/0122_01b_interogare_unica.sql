-- =====================================================================================
-- ONE-TIME QUERY  0122_01b_completari_codfiscal        (slice 0122-01, completion)
--
-- NOT run by hand. Paste this text into KBotAdmin, page «Interogari unice», name it `0122_01b_completari_codfiscal`,
-- tick the databases, press the preview and then run. Same way as 0122_01_codfiscal_in_codpartener (which stays as it is).
--
-- WHY: after 0122_01 the checks found two things left behind:
--   1. rows of FX_DDF_REV_SA, FX_DDF_REV_SB and FX_ORD_TBL with NO IdPartener whose CodPartener is still the old code of the
--      partner in its unit (for example 025), not a CodFiscal. 0122_01 could only normalize them, not recognise them.
--   2. rows of VC_NIR whose CodPartener was never filled (the column was probably missing when 0122_01 reached that block).
--
-- WHAT IT DOES:
--   * SA, SB, ORD_TBL: a row with no IdPartener whose CodPartener is NOT the CodFiscal of any partner, but IS the CodPartener of a
--     partner of the SAME unit (IdUnitate of the row) that has a CodFiscal: CodPartener := that normalized CodFiscal.
--   * VC_NIR: CodPartener := normalized CodFiscal of the partner of its IdPartener. When the partner has none, the CodFiscalSursa of
--     the NIR (the Access supplier's tax code, written at migration) is used. IdPartener is KEPT (Vercon still uses it).
-- Normalization as everywhere: letters and digits, upper case, a leading RO before a digit dropped.
--
-- WHAT IT DOES NOT TOUCH: a row whose code matches no partner of its unit, and every row already holding a known CodFiscal.
-- Nothing is deleted. SAFE TO RUN AGAIN: a row once changed holds a CodFiscal and is no longer selected.
-- Needs the column VC_NIR.CodPartener (sql/P122_01_vc_nir_codpartener.sql). A database without it FAILS on the last block.
-- =====================================================================================

-- ---- FX_DDF_REV_SA: old unit code, no IdPartener ----------------------------------
DROP TEMPORARY TABLE IF EXISTS p122b_sa;

CREATE TEMPORARY TABLE p122b_sa AS
SELECT x.id, IF(x.c REGEXP '^RO[0-9]', SUBSTRING(x.c, 3), x.c) AS cf
  FROM (SELECT R.IdSecA AS id,
               UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal, ''), '[^A-Za-z0-9]', '')) AS c
          FROM FX_DDF_REV_SA R
          JOIN Parteneri P ON P.IdUnitate = R.IdUnitate AND P.CodPartener = R.CodPartener
         WHERE R.IdPartener IS NULL AND COALESCE(R.CodPartener, '') <> ''
           AND NOT EXISTS (SELECT 1 FROM Parteneri Q
                            WHERE UPPER(REGEXP_REPLACE(COALESCE(Q.CodFiscal, ''), '[^A-Za-z0-9]', '')) IN (R.CodPartener, CONCAT('RO', R.CodPartener)))) x
 WHERE x.c <> '';

UPDATE FX_DDF_REV_SA R JOIN p122b_sa t ON t.id = R.IdSecA
   SET R.CodPartener = t.cf;

DROP TEMPORARY TABLE p122b_sa;

-- ---- FX_DDF_REV_SB ----------------------------------------------------------------
DROP TEMPORARY TABLE IF EXISTS p122b_sb;

CREATE TEMPORARY TABLE p122b_sb AS
SELECT x.id, IF(x.c REGEXP '^RO[0-9]', SUBSTRING(x.c, 3), x.c) AS cf
  FROM (SELECT R.IdSecB AS id,
               UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal, ''), '[^A-Za-z0-9]', '')) AS c
          FROM FX_DDF_REV_SB R
          JOIN Parteneri P ON P.IdUnitate = R.IdUnitate AND P.CodPartener = R.CodPartener
         WHERE R.IdPartener IS NULL AND COALESCE(R.CodPartener, '') <> ''
           AND NOT EXISTS (SELECT 1 FROM Parteneri Q
                            WHERE UPPER(REGEXP_REPLACE(COALESCE(Q.CodFiscal, ''), '[^A-Za-z0-9]', '')) IN (R.CodPartener, CONCAT('RO', R.CodPartener)))) x
 WHERE x.c <> '';

UPDATE FX_DDF_REV_SB R JOIN p122b_sb t ON t.id = R.IdSecB
   SET R.CodPartener = t.cf;

DROP TEMPORARY TABLE p122b_sb;

-- ---- FX_ORD_TBL -------------------------------------------------------------------
DROP TEMPORARY TABLE IF EXISTS p122b_ord;

CREATE TEMPORARY TABLE p122b_ord AS
SELECT x.id, IF(x.c REGEXP '^RO[0-9]', SUBSTRING(x.c, 3), x.c) AS cf
  FROM (SELECT R.IDORDTBLP AS id,
               UPPER(REGEXP_REPLACE(COALESCE(P.CodFiscal, ''), '[^A-Za-z0-9]', '')) AS c
          FROM FX_ORD_TBL R
          JOIN Parteneri P ON P.IdUnitate = R.IdUnitate AND P.CodPartener = R.CodPartener
         WHERE R.IdPartener IS NULL AND COALESCE(R.CodPartener, '') <> ''
           AND NOT EXISTS (SELECT 1 FROM Parteneri Q
                            WHERE UPPER(REGEXP_REPLACE(COALESCE(Q.CodFiscal, ''), '[^A-Za-z0-9]', '')) IN (R.CodPartener, CONCAT('RO', R.CodPartener)))) x
 WHERE x.c <> '';

UPDATE FX_ORD_TBL R JOIN p122b_ord t ON t.id = R.IDORDTBLP
   SET R.CodPartener = t.cf;

DROP TEMPORARY TABLE p122b_ord;

-- ---- VC_NIR (needs the column from P122_01) ---------------------------------------
DROP TEMPORARY TABLE IF EXISTS p122b_nir;

CREATE TEMPORARY TABLE p122b_nir AS
SELECT x.id, IF(x.c REGEXP '^RO[0-9]', SUBSTRING(x.c, 3), x.c) AS cf
  FROM (SELECT N.IdIntrare AS id,
               UPPER(REGEXP_REPLACE(COALESCE(IF(TRIM(COALESCE(P.CodFiscal, '')) <> '', P.CodFiscal, N.CodFiscalSursa), ''), '[^A-Za-z0-9]', '')) AS c
          FROM VC_NIR N
          JOIN Parteneri P ON P.IdPartener = N.IdPartener
         WHERE N.CodPartener IS NULL) x
 WHERE x.c <> '';

UPDATE VC_NIR N JOIN p122b_nir t ON t.id = N.IdIntrare
   SET N.CodPartener = t.cf;

DROP TEMPORARY TABLE p122b_nir;
