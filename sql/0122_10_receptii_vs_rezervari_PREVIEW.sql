-- =====================================================================================
-- READ-ONLY CHECK  0122_10_receptii_vs_rezervari_PREVIEW        (slice 0122-10)
--
-- Lists, for the database it is run on, every day on which the receptions of an angajament do not add up to
-- its reservations. WRITES NOTHING (one SELECT). For HeidiSQL, on ONE database at a time.
-- It is NOT a one-time query for KBotAdmin: that tool runs and records a statement, it does not show a result.
-- Across all the databases the main tree of K-BOT shows the same thing for each unit (tooltip + red row).
--
-- THE RULE (the same as PYTHON/routes/forexe/receptii_cumulat.py -- keep them in step):
--   the sum of FX_Receptii.DIF (versions not deleted) up to a reservation day, that day included, must equal
--   FX_Rezervari.R_Definitiva at the end of that day, per indicator.
--   Only days that have a reception in that day are looked at, only days with a single end of the day's
--   reservation chain, and only indicators with at least TWO days that match exactly (so a contract
--   reserved whole in advance is not reported).
--
-- WHY. A reception version put on the wrong header gives no error: the final total is right, but DIF is
-- computed against another chain, and an ordonantare (which adds DIF up to the payment date) sees wrong
-- receptions for the days in between. Case AAB2FH3R3FF: version 245 (12.10.2026) was on the reception of
-- 10.06.2026 instead of the one of 12.10.2026.
--
-- RESULT: one row per day that does not add up -- CodAngajament, CodIndicator, Zi, Rezervat (R_Definitiva),
-- Receptionat (sum of DIF), Diferenta. No rows = everything adds up.
-- NOT RUN anywhere yet (written, not executed).
-- =====================================================================================
WITH rez AS (
    SELECT CodAngajament, CodIndicator, IDRZ, DATE(DataRezervare) AS Zi,
           ROUND(COALESCE(R_Anterioara, 0), 2) AS Ant, ROUND(R_Definitiva, 2) AS Def
      FROM FX_Rezervari
     WHERE CodIndicator IS NOT NULL AND DataRezervare IS NOT NULL AND R_Definitiva IS NOT NULL
),
capat AS (
    -- the end of a day's reservation chain: the one row whose Def no other row of the day starts from
    SELECT r.CodAngajament, r.CodIndicator, r.Zi, MIN(r.Def) AS Def, COUNT(*) AS N
      FROM rez r
     WHERE NOT EXISTS (SELECT 1 FROM rez o
                        WHERE o.CodAngajament = r.CodAngajament AND o.CodIndicator = r.CodIndicator
                          AND o.Zi = r.Zi AND o.IDRZ <> r.IDRZ AND ABS(o.Ant - r.Def) < 0.01)
     GROUP BY r.CodAngajament, r.CodIndicator, r.Zi
    HAVING COUNT(*) = 1
),
rec AS (
    SELECT R.CodAngajament, R.CodIndicator, DATE(R.Data) AS Zi, ROUND(SUM(COALESCE(R.DIF, 0)), 2) AS Dif
      FROM FX_Receptii R
      JOIN FX_Receptii_H H ON H.IDRH = R.IDRH
     WHERE COALESCE(H.Sters, 0) = 0 AND R.CodIndicator IS NOT NULL AND R.Data IS NOT NULL
     GROUP BY R.CodAngajament, R.CodIndicator, DATE(R.Data)
),
verif AS (
    SELECT c.CodAngajament, c.CodIndicator, c.Zi, c.Def AS Rezervat,
           (SELECT ROUND(SUM(r2.Dif), 2) FROM rec r2
             WHERE r2.CodAngajament = c.CodAngajament AND r2.CodIndicator = c.CodIndicator
               AND r2.Zi <= c.Zi) AS Receptionat
      FROM capat c
      JOIN rec r ON r.CodAngajament = c.CodAngajament AND r.CodIndicator = c.CodIndicator AND r.Zi = c.Zi
)
SELECT v.CodAngajament, v.CodIndicator, v.Zi, v.Rezervat, v.Receptionat,
       ROUND(v.Rezervat - v.Receptionat, 2) AS Diferenta
  FROM verif v
 WHERE ABS(v.Rezervat - v.Receptionat) >= 0.01
   AND (SELECT COUNT(*) FROM verif p
         WHERE p.CodAngajament = v.CodAngajament AND p.CodIndicator = v.CodIndicator
           AND ABS(p.Rezervat - p.Receptionat) < 0.01) >= 2
 ORDER BY v.CodAngajament, v.CodIndicator, v.Zi;
