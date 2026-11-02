-- =====================================================================================
-- ONE-TIME REPAIR  corectie_AAB2EBFXAAE_receptii        (database 038_SCGP, ONE database only)
--
-- For HeidiSQL. Part 1 is read-only; Part 2 writes, in ONE transaction. NOT RUN anywhere yet.
--
-- WHAT WENT WRONG (angajament AAB2EBFXAAE, found 02.11.2026 from the FOREXE history in FX_Istoric):
--   A. Reception 58 (11.03.2026, "dobanda legala penalizatoare transa 4", true value AA2 3553) was
--      overwritten by a download made before the same-day matching fix (30.09.2026) with the values of
--      reception 59 (AAB 11543). Its chain (snapshot 73, AA2 3553) is right; the reception row is wrong.
--   B. Snapshots 75 and 76 (19213 = AAB 11543 + AA2 7670) belong to reception 61 ("... si contrib handica
--      neincadrati", 06.04.2026) but sit on reception 60 (04.04.2026), which has its own snapshot 74.
--      So AA2 7670 is counted twice in what the ordonantari read.
--
-- WHAT PART 2 DOES
--   1. remembers, per ordonantare line, the reception total the ordonantare reads NOW (before the move);
--   2. moves snapshots 75 and 76 from reception 60 to reception 61 (IDRR only, like the association window);
--   3. puts reception 58 back: header SumaAntet 3553, line AA2 = 3553, line AAB = 0;
--   4. recomputes DIFH/DIFHC and DIF/DIFC on the chains of 60 and 61 (same rule as step4d_calculeaza_dif);
--   5. recomputes the same per-line reception total AFTER the move and, on every FX_ORD_TBL line whose stored
--      TotalReceptii equals the "before" figure, adds the difference to TotalReceptii AND Ramas
--      (Ramas = TotalReceptii - PlatiAnt - Valoare, so it moves by the same amount). Lines whose stored figure
--      is not the "before" figure are NOT touched and are listed, to be looked at by hand.
--   So the ordonantari need no regeneration: only printing them again.
--
-- NOT CHANGED, ON PURPOSE: TipReceptie of reception 58 (stays 'EDIT'), CreditBugetar / ValoareN of its lines
-- (they carry the figures of reception 59's detail; the original ones are not in the database), the labels
-- Final / Partial (74 stays Final, 75 and 76 stay Partial, 77 stays Final: no label changes by the move).
--
-- NEEDS MariaDB >= 10.2 (window functions). If any statement of Part 2 fails: run  ROLLBACK;  and send the error.
-- =====================================================================================

SET @cod  = 'AAB2EBFXAAE';
SET @r_a  = 58;      -- reception put back to AA2 3553
SET @r_de = 60;      -- reception the snapshots sit on now
SET @r_la = 61;      -- reception they belong to
SET @h1   = 75;
SET @h2   = 76;

-- =====================================================================================
-- PART 1 -- LOOK (read-only). Save the grids (Export grid rows) before running Part 2: that is the backup.
-- =====================================================================================

-- 1.0 The state this script expects. ok must be 1 on every row, or Part 2 must NOT be run.
SELECT 'snapshot 75 on reception 60'  AS Check_, (SELECT IDRR FROM FX_Receptii_H WHERE IDRH = @h1 AND CodAngajament = @cod) = @r_de AS ok
UNION ALL SELECT 'snapshot 76 on reception 60',  (SELECT IDRR FROM FX_Receptii_H WHERE IDRH = @h2 AND CodAngajament = @cod) = @r_de
UNION ALL SELECT 'reception 58 reads 11543',     (SELECT ROUND(SumaAntet, 2) FROM FX_Receptii_R WHERE IDRR = @r_a AND CodAngajament = @cod) = 11543
UNION ALL SELECT 'snapshot 73 on reception 58',  (SELECT IDRR FROM FX_Receptii_H WHERE IDRH = 73 AND CodAngajament = @cod) = @r_a
UNION ALL SELECT 'snapshot 77 on reception 61',  (SELECT IDRR FROM FX_Receptii_H WHERE IDRH = 77 AND CodAngajament = @cod) = @r_la;

-- 1.1 The four receptions and their lines (reception 58 must show AAB 11543 / AA2 0 now).
SELECT R.IDRR, R.DataR, R.SumaAntet, R.TipReceptie, R.Descriere,
       L.IDRHR, L.CodIndicator, L.Valoare, L.ValoareN, L.CreditBugetar, L.TipIntern
  FROM FX_Receptii_R R LEFT JOIN FX_Receptii_RHR L ON L.IDRR = R.IDRR
 WHERE R.CodAngajament = @cod AND R.IDRR IN (58, 59, 60, 61)
 ORDER BY R.IDRR, L.CodIndicator;

-- 1.2 The snapshots of the four chains.
SELECT H.IDRH, H.IDRR, H.DataH, H.Total, H.TipReceptie, H.DIFH, H.DIFHC
  FROM FX_Receptii_H H
 WHERE H.CodAngajament = @cod AND H.IDRH BETWEEN 72 AND 77
 ORDER BY H.IDRR, H.DataH, H.IDRH;

-- 1.3 The lines of snapshots 74-77 with the DIF they have now.
SELECT R.IDR, R.IDRH, R.CodIndicator, R.Data, R.Valoare, R.DIF, R.DIFC
  FROM FX_Receptii R
 WHERE R.CodAngajament = @cod AND R.IDRH IN (74, 75, 76, 77)
 ORDER BY R.CodIndicator, R.Data, R.IDRH;

-- 1.4 Ordonantare lines from 05.04.2026 on: the figure stored, and the figure the ordonantare reads now
--     (the same sum as qFX_ORD_REC_ANT in ord_edit.py). Lines where Stored <> ReadNow will NOT be touched.
SELECT O.NrORD, O.DataORD, T.IDORDTBLP, T.CodIndicator, T.TotalReceptii AS Stored,
       ROUND(COALESCE((SELECT SUM(R.DIF)
                         FROM FX_Receptii R JOIN FX_Receptii_H H ON R.IDRH = H.IDRH
                        WHERE H.CodAngajament = @cod AND COALESCE(H.Sters, 0) = 0
                          AND R.CodIndicator = T.CodIndicator AND DATE(R.Data) <= DATE(O.DataORD)), 0), 2) AS ReadNow,
       T.PlatiAnt, T.Valoare, T.Ramas
  FROM FX_ORD_TBL T JOIN FX_ORD O ON O.IDORDP = T.IDORDP
 WHERE O.CodAngajament = @cod AND DATE(O.DataORD) >= '2026-04-05'
 ORDER BY O.DataORD, O.NrORD, T.CodIndicator;

-- =====================================================================================
-- PART 2 -- WRITE. Run only when 1.0 is all 1 and 1.1-1.4 look as described above.
-- =====================================================================================
START TRANSACTION;

-- 2.1 The figure each ordonantare line reads BEFORE the move.
DROP TEMPORARY TABLE IF EXISTS tmp_ord_before;
CREATE TEMPORARY TABLE tmp_ord_before AS
SELECT T.IDORDTBLP,
       ROUND(COALESCE((SELECT SUM(R.DIF)
                         FROM FX_Receptii R JOIN FX_Receptii_H H ON R.IDRH = H.IDRH
                        WHERE H.CodAngajament = @cod AND COALESCE(H.Sters, 0) = 0
                          AND R.CodIndicator = T.CodIndicator AND DATE(R.Data) <= DATE(O.DataORD)), 0), 2) AS Val
  FROM FX_ORD_TBL T JOIN FX_ORD O ON O.IDORDP = T.IDORDP
 WHERE O.CodAngajament = @cod;

-- 2.2 Move snapshots 75 and 76 to reception 61 (expected: 2 rows).
UPDATE FX_Receptii_H SET IDRR = @r_la
 WHERE CodAngajament = @cod AND IDRH IN (@h1, @h2) AND IDRR = @r_de;
SELECT ROW_COUNT() AS moved_expected_2;

-- 2.3 Put reception 58 back (expected 1, 1, 1).
UPDATE FX_Receptii_R SET SumaAntet = 3553
 WHERE IDRR = @r_a AND CodAngajament = @cod AND ROUND(SumaAntet, 2) = 11543;
SELECT ROW_COUNT() AS header_expected_1;
UPDATE FX_Receptii_RHR SET Valoare = 3553
 WHERE IDRR = @r_a AND CodAngajament = @cod AND CodIndicator = 'AA2' AND ROUND(COALESCE(Valoare, 0), 2) = 0;
SELECT ROW_COUNT() AS line_AA2_expected_1;
UPDATE FX_Receptii_RHR SET Valoare = 0
 WHERE IDRR = @r_a AND CodAngajament = @cod AND CodIndicator = 'AAB' AND ROUND(COALESCE(Valoare, 0), 2) = 11543;
SELECT ROW_COUNT() AS line_AAB_expected_1;

-- 2.4 DIFH / DIFHC on the headers of chains 60 and 61 (step4d_calculeaza_dif, header loop).
UPDATE FX_Receptii_H T
  JOIN (SELECT H.IDRH,
               ROUND(H.Total - COALESCE(LAG(H.Total) OVER w, 0), 2) AS d,
               ROUND(H.Total - FIRST_VALUE(H.Total) OVER w, 2)      AS dc
          FROM FX_Receptii_H H
         WHERE H.CodAngajament = @cod AND H.IDRR IN (@r_de, @r_la)
           AND (COALESCE(H.EsteStergere, 0) <> 0 OR EXISTS (SELECT 1 FROM FX_Receptii L WHERE L.IDRH = H.IDRH))
        WINDOW w AS (PARTITION BY H.IDRR ORDER BY H.DataH, H.IDRH)) X ON X.IDRH = T.IDRH
   SET T.DIFH = X.d, T.DIFHC = X.dc;

-- 2.5 DIF / DIFC on the lines of chains 60 and 61 (step4d_calculeaza_dif, line loop: per CodAI + IdClsf).
UPDATE FX_Receptii T
  JOIN (SELECT R.IDR,
               ROUND(COALESCE(R.Valoare, 0) - COALESCE(LAG(COALESCE(R.Valoare, 0)) OVER w, 0), 2) AS d,
               ROUND(COALESCE(R.Valoare, 0) - FIRST_VALUE(COALESCE(R.Valoare, 0)) OVER w, 2)      AS dc
          FROM FX_Receptii R JOIN FX_Receptii_H H ON R.IDRH = H.IDRH
         WHERE R.CodAngajament = @cod AND H.IDRR IN (@r_de, @r_la)
        WINDOW w AS (PARTITION BY H.IDRR, R.CodAI, COALESCE(R.IdClsf, 0) ORDER BY H.DataH, H.IDRH)) X ON X.IDR = T.IDR
   SET T.DIF = X.d, T.DIFC = X.dc;

-- 2.6 The figure each ordonantare line reads AFTER the move.
DROP TEMPORARY TABLE IF EXISTS tmp_ord_after;
CREATE TEMPORARY TABLE tmp_ord_after AS
SELECT T.IDORDTBLP,
       ROUND(COALESCE((SELECT SUM(R.DIF)
                         FROM FX_Receptii R JOIN FX_Receptii_H H ON R.IDRH = H.IDRH
                        WHERE H.CodAngajament = @cod AND COALESCE(H.Sters, 0) = 0
                          AND R.CodIndicator = T.CodIndicator AND DATE(R.Data) <= DATE(O.DataORD)), 0), 2) AS Val
  FROM FX_ORD_TBL T JOIN FX_ORD O ON O.IDORDP = T.IDORDP
 WHERE O.CodAngajament = @cod;

-- 2.7 Lines that change but whose stored figure is NOT the "before" figure: left alone, look at them by hand.
SELECT O.NrORD, O.DataORD, T.IDORDTBLP, T.CodIndicator, T.TotalReceptii AS Stored, B.Val AS ReadBefore, A.Val AS ReadAfter
  FROM FX_ORD_TBL T JOIN FX_ORD O ON O.IDORDP = T.IDORDP
  JOIN tmp_ord_before B ON B.IDORDTBLP = T.IDORDTBLP
  JOIN tmp_ord_after  A ON A.IDORDTBLP = T.IDORDTBLP
 WHERE ROUND(A.Val - B.Val, 2) <> 0 AND ROUND(T.TotalReceptii, 2) <> B.Val;

-- 2.8 Move the stored figures by the difference (TotalReceptii and Ramas together).
UPDATE FX_ORD_TBL T
  JOIN tmp_ord_before B ON B.IDORDTBLP = T.IDORDTBLP
  JOIN tmp_ord_after  A ON A.IDORDTBLP = T.IDORDTBLP
   SET T.TotalReceptii = ROUND(T.TotalReceptii + (A.Val - B.Val), 2),
       T.Ramas         = ROUND(T.Ramas + (A.Val - B.Val), 2)
 WHERE ROUND(A.Val - B.Val, 2) <> 0 AND ROUND(T.TotalReceptii, 2) = B.Val;
SELECT ROW_COUNT() AS ord_lines_changed;

-- 2.9 The result, before committing: chains close, ordonantare lines moved by the same amount.
SELECT 'chains' AS What, H.IDRR AS A, H.IDRH AS B, H.Total AS C, H.DIFH AS D
  FROM FX_Receptii_H H WHERE H.CodAngajament = @cod AND H.IDRH BETWEEN 72 AND 77
UNION ALL
SELECT 'ord line', O.NrORD, T.IDORDTBLP, T.TotalReceptii, T.Ramas
  FROM FX_ORD_TBL T JOIN FX_ORD O ON O.IDORDP = T.IDORDP
  JOIN tmp_ord_before B ON B.IDORDTBLP = T.IDORDTBLP JOIN tmp_ord_after A ON A.IDORDTBLP = T.IDORDTBLP
 WHERE ROUND(A.Val - B.Val, 2) <> 0
 ORDER BY What, A, B;

-- THE TRANSACTION IS STILL OPEN HERE. Look at the counts of 2.2 / 2.3 and at 2.9, then run ONE of these two
-- (select the single line and run it; do not leave the transaction open, it holds locks):
--   COMMIT;      -- all counts as expected and 2.9 looks right
--   ROLLBACK;    -- anything else: nothing was written
-- The two temporary tables disappear with the connection.
