-- =====================================================================================
-- Slice 0108 -- LAST STEP. RUN ON `AVACONT_SURSA` ONLY (HeidiSQL / mysql, as root), then on every
-- unit database (a SAFE schema sync never drops a column, so run the same two statements per unit,
-- or use the «Interogări unice» tool with the text below).
--
-- Drops the two columns nothing reads any more:
--   * FX_Indicatori.Credit_Bugetar -- replaced by FX_Indicatori_Buget (one row per classification);
--   * FX_Indicatori.DTL            -- replaced by DTQ, which is now ON UPDATE (operator, 03.10.2026).
--
-- ONLY AFTER the server files of this slice, the K-BOT client and the Migrator of this slice are
-- installed EVERYWHERE: until then the old code still writes / reads Credit_Bugetar and fails with
-- «Unknown column» if it is gone. Safe to run twice (IF EXISTS).
-- =====================================================================================

ALTER TABLE `FX_Indicatori`
  DROP COLUMN IF EXISTS `Credit_Bugetar`,
  DROP COLUMN IF EXISTS `DTL`;
