-- KBotAdmin -> Interogari unice -> Din fisier.
-- NAME: VC_20261027_BC_NUMEROTARE_PE_CONT
-- SLICE-VC06-19. For the database(s) whose consumption bons (BC) were saved with one continuous numbering for all
-- accounts: tick only that database in the target list (AVACONT_SURSA is always included first by the runner; it holds
-- no bons, so nothing changes there).
--
-- 1. CHOOSE WHAT IT APPLIES TO: edit the three SET lines below (the other lines stay as they are).
--      @bcn_start     the first day renumbered. Bons dated BEFORE it are not touched; the last number of each account is its seed.
--      @bcn_subunit   the exact name of the subunit (VC_Subunitati.Name).
--      @bcn_gestiuni  the exact names of the MATERIALE gestiuni of that subunit, each between | signs:
--                     '|Magazie materiale|Atelier|'   (a name never contains |).
--    Left empty, the query refuses to run (error PARAMETRI_NECOMPLETATI_BCN) and nothing is marked as run.
-- 2. The ledger identifies a query by its text: after the SET lines change, the NAME above must change too
--    (e.g. VC_20261027_BC_NUMEROTARE_PE_CONT_018_GRRS), also for a second run with other values.
--
-- What it does, per selected gestiune and per exercise:
--   * SEED, separately for every account: the number of the last bon of THAT account before @bcn_start = the largest
--     number among the bons of the account on the latest day that has bons of it before the date (not returns, not
--     cancelled). It does not have to be the day before: it can be much earlier. No such bon (an account without
--     bons before the date, or a new exercise): seed 0.
--   * the bons from @bcn_start on (not returns, not cancelled) are renumbered separately for every account
--     (VC_Iesiri_H.IdMasina): seed of the account + 1, + 2, ... in the order of the bon date, then the old number, then the id.
--   * the counters VC_Numbering ('BC:<IdMasina>') of EVERY account of the gestiune are set to the last number
--     of that account, or to its seed when it has no bon from @bcn_start on, so the next bon follows;
--   * the old and the new number of every changed bon go to VC_Audit (UserEmail 'interogare_unica');
--   * the setting NUM_BC_PE_CONT is turned on (the gestiune numbers per account from now on, like the page option).
-- The new number is written as BC <number>: the prefix, ONE space, the number (BC 13). Only rows that hold a consumption
-- number are read and renumbered, in any older spelling (BC 13, BC13, 13); rows of other kinds (BP ..., BX ...) are never touched.
-- Not touched: bons before @bcn_start, returns (BX...), cancelled bons (they keep their old number), values, lines,
-- links to food lists (those are by id), the old counter 'BC', other gestiuni and other gestiune types.
-- If a bon of an account before @bcn_start has a number above its seed, a new number can equal it inside one account: the page then
-- reports "Numarul ... este deja folosit" on that bon only.
-- Printed or exported bons keep the numbers they were printed with: run it before the numbers are given to anyone.
-- The runner owns the transaction and the ledger: no BEGIN/COMMIT here, no permanent DDL.
-- Needs MariaDB 10.2 or newer (ROW_NUMBER).

SET @bcn_start = NULL;
SET @bcn_subunit = '';
SET @bcn_gestiuni = '';

DROP TEMPORARY TABLE IF EXISTS VC_BCN_Guard;
CREATE TEMPORARY TABLE VC_BCN_Guard (Reason VARCHAR(60) NOT NULL, PRIMARY KEY (Reason));
INSERT INTO VC_BCN_Guard (Reason)
SELECT 'PARAMETRI_NECOMPLETATI_BCN' FROM DUAL
WHERE @bcn_start IS NULL OR COALESCE(@bcn_subunit, '') = '' OR COALESCE(@bcn_gestiuni, '') = ''
UNION ALL
SELECT 'PARAMETRI_NECOMPLETATI_BCN' FROM DUAL
WHERE @bcn_start IS NULL OR COALESCE(@bcn_subunit, '') = '' OR COALESCE(@bcn_gestiuni, '') = '';

DROP TEMPORARY TABLE IF EXISTS VC_BCN_Gest;
CREATE TEMPORARY TABLE VC_BCN_Gest AS
SELECT g.SubunitId, g.GestiuneId
FROM VC_Gestiuni g
JOIN VC_Subunitati s ON s.SubunitId = g.SubunitId
WHERE g.`Type` = 'MATERIALE' AND s.Name = @bcn_subunit
  AND INSTR(@bcn_gestiuni, CONCAT('|', g.Name, '|')) > 0;

DROP TEMPORARY TABLE IF EXISTS VC_BCN_Seed;
CREATE TEMPORARY TABLE VC_BCN_Seed AS
SELECT e.SubunitId, e.GestiuneId, e.ExercitiuId, m.IdMasina,
       COALESCE((SELECT MAX(CAST(REGEXP_REPLACE(h.NumarBC, '[^0-9]', '') AS UNSIGNED))
                 FROM VC_Iesiri_H h
                 WHERE h.SubunitId = e.SubunitId AND h.GestiuneId = e.GestiuneId AND h.ExercitiuId = e.ExercitiuId
                   AND h.IdMasina = m.IdMasina AND COALESCE(h.Retur, 0) = 0 AND h.Anulat = 0 AND h.NumarBC REGEXP '^ *(BC)? *[0-9]+ *$'
                   AND DATE(h.DataBC) = (SELECT MAX(DATE(x.DataBC))
                                         FROM VC_Iesiri_H x
                                         WHERE x.SubunitId = e.SubunitId AND x.GestiuneId = e.GestiuneId
                                           AND x.ExercitiuId = e.ExercitiuId AND x.IdMasina = m.IdMasina
                                           AND COALESCE(x.Retur, 0) = 0 AND x.Anulat = 0 AND x.NumarBC REGEXP '^ *(BC)? *[0-9]+ *$'
                                           AND DATE(x.DataBC) < CAST(@bcn_start AS DATE))), 0) AS Seed
FROM VC_Exercitii e
JOIN VC_BCN_Gest g ON g.SubunitId = e.SubunitId AND g.GestiuneId = e.GestiuneId
JOIN VC_Masini m ON m.SubunitId = e.SubunitId AND m.GestiuneId = e.GestiuneId;

DROP TEMPORARY TABLE IF EXISTS VC_BCN_Plan;
CREATE TEMPORARY TABLE VC_BCN_Plan AS
SELECT h.IdBonConsum, h.SubunitId, h.GestiuneId, h.ExercitiuId, h.IdMasina, h.NumarBC AS OldNumber,
       d.Seed + ROW_NUMBER() OVER (PARTITION BY h.SubunitId, h.GestiuneId, h.ExercitiuId, h.IdMasina
                                   ORDER BY DATE(h.DataBC), CAST(REGEXP_REPLACE(h.NumarBC, '[^0-9]', '') AS UNSIGNED), h.IdBonConsum) AS NewNumber
FROM VC_Iesiri_H h
JOIN VC_BCN_Seed d ON d.SubunitId = h.SubunitId AND d.GestiuneId = h.GestiuneId AND d.ExercitiuId = h.ExercitiuId
                  AND d.IdMasina = h.IdMasina
WHERE COALESCE(h.Retur, 0) = 0 AND h.Anulat = 0 AND h.NumarBC REGEXP '^ *(BC)? *[0-9]+ *$' AND h.IdMasina IS NOT NULL
  AND DATE(h.DataBC) >= CAST(@bcn_start AS DATE);

INSERT INTO VC_Audit (SubunitId, GestiuneId, ExercitiuId, UserEmail, Operation, TableName, RowKey, BeforeValue, AfterValue)
SELECT p.SubunitId, p.GestiuneId, p.ExercitiuId, 'interogare_unica', 'bc.renumber_per_account', 'VC_Iesiri_H', p.IdBonConsum,
       CONCAT('{"NumarBC": "', COALESCE(p.OldNumber, ''), '", "IdMasina": ', p.IdMasina, '}'),
       CONCAT('{"NumarBC": "BC ', p.NewNumber, '", "IdMasina": ', p.IdMasina, '}')
FROM VC_BCN_Plan p
WHERE NOT (p.OldNumber <=> CONCAT('BC ', p.NewNumber));

UPDATE VC_Iesiri_H h
JOIN VC_BCN_Plan p ON p.IdBonConsum = h.IdBonConsum
SET h.NumarBC = CONCAT('BC ', p.NewNumber), h.Version = h.Version + 1
WHERE NOT (h.NumarBC <=> CONCAT('BC ', p.NewNumber));

INSERT INTO VC_Numbering (SubunitId, GestiuneId, ExercitiuId, Kind, LastNumber)
SELECT d.SubunitId, d.GestiuneId, d.ExercitiuId, CONCAT('BC:', d.IdMasina),
       GREATEST(d.Seed, COALESCE((SELECT MAX(p.NewNumber)
                                  FROM VC_BCN_Plan p
                                  WHERE p.SubunitId = d.SubunitId AND p.GestiuneId = d.GestiuneId
                                    AND p.ExercitiuId = d.ExercitiuId AND p.IdMasina = d.IdMasina), 0))
FROM VC_BCN_Seed d
ON DUPLICATE KEY UPDATE LastNumber = VALUES(LastNumber), Version = Version + 1;

INSERT INTO VC_Settings (SubunitId, GestiuneId, SettingKey, IntValue)
SELECT g.SubunitId, g.GestiuneId, 'NUM_BC_PE_CONT', 1
FROM VC_BCN_Gest g
ON DUPLICATE KEY UPDATE IntValue = 1, Version = Version + 1;

DROP TEMPORARY TABLE IF EXISTS VC_BCN_Plan;
DROP TEMPORARY TABLE IF EXISTS VC_BCN_Seed;
DROP TEMPORARY TABLE IF EXISTS VC_BCN_Gest;
DROP TEMPORARY TABLE IF EXISTS VC_BCN_Guard;
