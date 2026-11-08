-- =====================================================================================
-- Slice 0076-02 -- RUN THIS ON `AVACONT_SURSA` ONLY (HeidiSQL / mysql, as root).
--
-- AVACONT_SURSA is the template: the schema sync copies its STRUCTURE to every unit database
-- (see 0108_01_sursa.sql). No data part.
--
-- WHAT THIS SLICE CHANGES (operator, 06.11.2026)
--   A reception deleted in the FOREXE page of K-BOT is marked deleted at once (FX_Receptii_R.Sters = 1),
--   before the history row «Stergere receptie» reaches K-BOT. Two columns say that the deletion was
--   SEEN in FOREXE (so it is not a mark the operator put from the association window):
--     StersObservatLa  when K-BOT learned of the deletion; NOT NULL = «deleted, proven by FOREXE».
--                      The association windows cannot take such a Sters back.
--     StersObservatDe  the K-BOT user who was working in the browser then.
--
-- ORDER:
--   1. this file, on AVACONT_SURSA;
--   2. AvacontPush, tab «Sincronizare schema»: SAFE, on every unit database;
--   3. AvacontPush: push the new Python files, restart the service;
--   4. the client (K-BOT).
--
-- Safe to run twice (IF NOT EXISTS). Every INSERT into FX_Receptii_R names its columns, so new
-- nullable columns break none of them. utf8mb3 like the rest of the schema (no CHARACTER SET given:
-- the table default applies).
-- =====================================================================================

ALTER TABLE `AVACONT_SURSA`.`FX_Receptii_R`
  ADD COLUMN IF NOT EXISTS `StersObservatLa` datetime NULL DEFAULT NULL
    COMMENT 'Slice 0076-02: when K-BOT saw the deletion in the FOREXE page; NULL = Sters (if any) is not from FOREXE',
  ADD COLUMN IF NOT EXISTS `StersObservatDe` varchar(100) NULL DEFAULT NULL
    COMMENT 'Slice 0076-02: the K-BOT user who was in the browser when the deletion was seen';

-- Check:
-- SHOW CREATE TABLE `AVACONT_SURSA`.`FX_Receptii_R`;
