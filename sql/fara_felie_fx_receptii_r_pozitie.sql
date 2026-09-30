-- =====================================================================================
-- Sliceless (operator, 30.09.2026): two or more receptions on the SAME DAY.
--
-- Step 4b used to find the existing reception of a FOREXE row by angajament + date only and
-- take the first one. Two receptions of the same day therefore collapsed into ONE
-- FX_Receptii_R row: the second overwrote the first's value and lines, the second reception
-- never got its own row, and the snapshots of both ended up on one chain.
--
--   NrCrtForexe  the row's 1-based position in the FOREXE receptions list at the last read.
--                Kept for the record and for the journal; NOT used to match (it shifts when a
--                reception with an earlier date is added or deleted on the site).
--   RangZi       1-based rank among the receptions of the SAME DATE, in FOREXE list order.
--                This is the tie breaker step 4b matches on (date + rank).
--
-- NULL on every reception written before this script: the first download after it stamps
-- them (step 4b pairs the legacy row by value first, then by order).
--
-- Run ONCE on EVERY unit database (000_DEMO first) and on AVACONT_SURSA (the template).
-- Safe to run twice: IF NOT EXISTS. Must run BEFORE the new server code is deployed.
-- =====================================================================================

ALTER TABLE `FX_Receptii_R`
  ADD COLUMN IF NOT EXISTS `NrCrtForexe` INT NULL DEFAULT NULL
  COMMENT 'Pozitia (1..n) in lista de receptii din FOREXE la ultima citire. Informativ.',
  ADD COLUMN IF NOT EXISTS `RangZi` INT NULL DEFAULT NULL
  COMMENT 'Rangul (1..n) intre receptiile din aceeasi zi, in ordinea din FOREXE. Departajeaza receptiile din aceeasi zi.';
