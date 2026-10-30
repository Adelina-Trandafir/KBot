-- KBotAdmin -> Interogari unice -> Din fisier.
-- NAME: VC_20261027_BC_REPARA_FIR_NUMERE
-- SLICE-VC06-19. Repairs the thread of the consumption bon numbers (BC) of ONE gestiune, in the order the bons were entered.
-- Tick only the database(s) you want; AVACONT_SURSA is always included first by the runner (it has no such gestiune: nothing changes).
--
-- 1. WRITE THE TWO IDS (the only lines to edit) - the ids of the database you tick:
--      @bcr_subunit    IdSubunitate = VC_Subunitati.SubunitId
--      @bcr_gestiune   IdGestiune   = VC_Gestiuni.GestiuneId
--    Left empty, the query refuses to run (error PARAMETRI_NECOMPLETATI_BCR) and nothing is marked as run.
-- 2. The ledger identifies a query by its text: after the ids change, the NAME above must change too
--    (e.g. VC_20261027_BC_REPARA_FIR_NUMERE_018_G2), also for a second run with other ids.
-- Look at what it would change BEFORE running: sql/VC_20261027_BC_REPARA_FIR_NUMERE_PREVIEW.sql (a plain SELECT, changes nothing).
--
-- What it does, for every exercise of that gestiune:
--   * the consumption bons (not returns; any spelling BC 13, BC13, 13; cancelled bons too: a cancelled bon keeps its place)
--     are taken in the order of IdBonConsum, separately for every account when the gestiune numbers per account
--     (VC_Settings NUM_BC_PE_CONT = 1), together for the whole gestiune otherwise;
--   * the FIRST bon of a series keeps its number; every next one gets the previous number + 1 (so 81, 82, 83, 84, 1, 2, 12, 13
--     becomes 81, 82, 83, 84, 85, 86, 87, 88). Where the thread is not broken nothing changes;
--   * every number is written as BC <number>: the prefix, ONE space, the number (BC 13);
--   * the counters VC_Numbering ('BC:<IdMasina>', or 'BC' without per-account numbering) are set to the last number of the series,
--     so the next bon follows;
--   * the old and the new number of every changed bon go to VC_Audit (UserEmail 'interogare_unica').
-- Not touched: returns (BX ...), rows of other kinds (BP ...), values, lines, dates, links to food lists (those are by id),
-- the setting NUM_BC_PE_CONT, other gestiuni.
-- If the first bon of a series is itself out of place, or older bons have gaps that are meant to stay, those gaps are closed too:
-- check the PREVIEW first. Printed or exported bons keep the numbers they were printed with.
-- The runner owns the transaction and the ledger: no BEGIN/COMMIT here, no permanent DDL. Needs MariaDB 10.2 or newer.

SET @bcr_subunit = NULL;
SET @bcr_gestiune = NULL;

DROP TEMPORARY TABLE IF EXISTS VC_BCR_Guard;
CREATE TEMPORARY TABLE VC_BCR_Guard (Reason VARCHAR(60) NOT NULL, PRIMARY KEY (Reason));
INSERT INTO VC_BCR_Guard (Reason)
SELECT 'PARAMETRI_NECOMPLETATI_BCR' FROM DUAL WHERE @bcr_subunit IS NULL OR @bcr_gestiune IS NULL
UNION ALL
SELECT 'PARAMETRI_NECOMPLETATI_BCR' FROM DUAL WHERE @bcr_subunit IS NULL OR @bcr_gestiune IS NULL;

DROP TEMPORARY TABLE IF EXISTS VC_BCR_Base;
CREATE TEMPORARY TABLE VC_BCR_Base AS
SELECT h.IdBonConsum, h.SubunitId, h.GestiuneId, h.ExercitiuId, h.IdMasina, h.NumarBC AS OldText,
       CASE WHEN COALESCE(s.IntValue, 0) = 1 THEN COALESCE(h.IdMasina, 0) ELSE 0 END AS Scope,
       CAST(REGEXP_REPLACE(h.NumarBC, '[^0-9]', '') AS UNSIGNED) AS OldNumber
FROM VC_Iesiri_H h
LEFT JOIN VC_Settings s ON s.SubunitId = h.SubunitId AND s.GestiuneId = h.GestiuneId AND s.SettingKey = 'NUM_BC_PE_CONT'
WHERE h.SubunitId = @bcr_subunit AND h.GestiuneId = @bcr_gestiune
  AND COALESCE(h.Retur, 0) = 0 AND h.NumarBC REGEXP '^ *(BC)? *[0-9]+ *$';

DROP TEMPORARY TABLE IF EXISTS VC_BCR_Plan;
CREATE TEMPORARY TABLE VC_BCR_Plan AS
SELECT b.IdBonConsum, b.SubunitId, b.GestiuneId, b.ExercitiuId, b.IdMasina, b.Scope, b.OldText,
       FIRST_VALUE(b.OldNumber) OVER w + ROW_NUMBER() OVER w - 1 AS NewNumber
FROM VC_BCR_Base b
WINDOW w AS (PARTITION BY b.SubunitId, b.GestiuneId, b.ExercitiuId, b.Scope ORDER BY b.IdBonConsum);

INSERT INTO VC_Audit (SubunitId, GestiuneId, ExercitiuId, UserEmail, Operation, TableName, RowKey, BeforeValue, AfterValue)
SELECT p.SubunitId, p.GestiuneId, p.ExercitiuId, 'interogare_unica', 'bc.repair_number_thread', 'VC_Iesiri_H', p.IdBonConsum,
       CONCAT('{"NumarBC": "', COALESCE(p.OldText, ''), '"}'),
       CONCAT('{"NumarBC": "BC ', p.NewNumber, '"}')
FROM VC_BCR_Plan p
WHERE NOT (p.OldText <=> CONCAT('BC ', p.NewNumber));

UPDATE VC_Iesiri_H h
JOIN VC_BCR_Plan p ON p.IdBonConsum = h.IdBonConsum
SET h.NumarBC = CONCAT('BC ', p.NewNumber), h.Version = h.Version + 1
WHERE NOT (h.NumarBC <=> CONCAT('BC ', p.NewNumber));

INSERT INTO VC_Numbering (SubunitId, GestiuneId, ExercitiuId, Kind, LastNumber)
SELECT p.SubunitId, p.GestiuneId, p.ExercitiuId, IF(p.Scope = 0, 'BC', CONCAT('BC:', p.Scope)), MAX(p.NewNumber)
FROM VC_BCR_Plan p
GROUP BY p.SubunitId, p.GestiuneId, p.ExercitiuId, p.Scope
ON DUPLICATE KEY UPDATE LastNumber = VALUES(LastNumber), Version = Version + 1;

DROP TEMPORARY TABLE IF EXISTS VC_BCR_Plan;
DROP TEMPORARY TABLE IF EXISTS VC_BCR_Base;
DROP TEMPORARY TABLE IF EXISTS VC_BCR_Guard;
