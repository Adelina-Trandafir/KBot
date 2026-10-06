-- =====================================================================================
-- Slice 0111 -- RUN THIS ON `AVACONT_SURSA` ONLY (HeidiSQL / mysql, as root).
--
-- AVACONT_SURSA is the template: the schema sync copies its STRUCTURE to every unit database
-- (see 0108_01_sursa.sql). The DATA part is the one-time query `0111_02_interogare_unica.sql`.
--
-- WHAT THIS SLICE CHANGES (operator, 06.10.2026)
--   FOREXE's own history sometimes writes a WRONG total on a reception header: the history row
--   «Receptie: ..., valoare: 0, (activ:true)» of AAB3MEF2MG2 (reception of 12.06.2026) says 0 while
--   the line under it says 1635. K-BOT copies the header total into FX_Receptii_H.Total, so the
--   chain of that reception does not close and DIFH / DIF (what the ordonantare sums) start from
--   a false figure. FX_Istoric is the evidence of what FOREXE said and is NEVER touched.
--
--   The operator may now correct the figure from the association window. The correction is made
--   on the WORKING columns (`FX_Receptii_H.Total`, `FX_Receptii.Valoare`), which every reader
--   already uses; the ORIGINAL columns (`TotalOrig`, `ValoareOrig`, both already in the schema)
--   keep what FOREXE said. A row is «corrected» when its working value differs from its original.
--   `TotalOrig` was never written by K-BOT; it is filled at birth from now on, and for the rows that
--   exist by the one-time query.
--
--   THE ONLY STRUCTURE CHANGE: who / when / why, three columns on the header. One save = one
--   header + its lines, so the header carries the audit of the whole correction.
--
-- ORDER (the whole slice):
--   1. this file, on AVACONT_SURSA;
--   2. AvacontPush, tab «Sincronizare schema»: SAFE, on every unit database;
--   3. AvacontPush: push the new Python files, restart the service (the birth of a header now
--      writes TotalOrig);
--   4. AvacontPush, tab «Interogari unice»: the text of `0111_02_interogare_unica.sql`, name
--      `0111_total_orig_si_valoare_orig`, «Vezi», then «Executa»;
--   5. the client (K-BOT).
--
-- Safe to run twice (IF NOT EXISTS). Every INSERT into FX_Receptii_H names its columns
-- (checked in PYTHON/routes/forexe), so new nullable columns break none of them.
-- =====================================================================================

ALTER TABLE `AVACONT_SURSA`.`FX_Receptii_H`
  ADD COLUMN IF NOT EXISTS `CorectatDe` varchar(255) NULL DEFAULT NULL
    COMMENT 'Slice 0111: who corrected Total (or its lines) by hand; NULL = never corrected' AFTER `TotalOrig`,
  ADD COLUMN IF NOT EXISTS `CorectatLa` datetime NULL DEFAULT NULL
    COMMENT 'Slice 0111: when Total (or its lines) were last corrected by hand' AFTER `CorectatDe`,
  ADD COLUMN IF NOT EXISTS `CorectatMotiv` varchar(500) NULL DEFAULT NULL
    COMMENT 'Slice 0111: why (required on every correction)' AFTER `CorectatLa`;

-- Check:
-- SHOW CREATE TABLE `AVACONT_SURSA`.`FX_Receptii_H`;
