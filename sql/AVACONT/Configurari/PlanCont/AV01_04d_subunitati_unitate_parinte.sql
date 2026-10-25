-- =====================================================================================
-- Slice AV01-05 -- RUN THIS ON `AVACONT_SURSA` ONLY (Navicat / mysql, as root), then «Sincronizare schema» on the unit database.
--
-- WHAT THIS ADDS to TZ_Subunitati (two nullable columns, nothing existing changes):
--   IdUnitateAccess      the IdUnitate of the old Access unit (cale.accdb > cai) this subunit was ported from. UNIQUE: one
--                        Access unit = one subunit. The migrator uses it to put the chart of accounts of each unit into
--                        its own subunit, and to know which units are already ported. NULL for a subunit made by hand.
--   IdSubunitateParinte  the parent subunit (Access cai.IdUnitateParinte). A subunit with a parent has its own data but reads
--                        the CLASSIFICATIONS and the BUDGET of the parent. NULL = it has its own (the usual case).
--                        Only stored here; the windows that read classifications / budget use it in a later slice.
--
-- RULES: deleting a parent that still has children is BLOCKED (RESTRICT, like everywhere in the TZ_ family).
-- Safe to run twice (IF NOT EXISTS). utf8mb3_general_ci like every other table (nothing textual here).
-- =====================================================================================

ALTER TABLE `AVACONT_SURSA`.`TZ_Subunitati`
  ADD COLUMN IF NOT EXISTS `IdUnitateAccess`     int(11) NULL DEFAULT NULL COMMENT 'IdUnitate of the Access unit (cale.accdb > cai) this subunit was ported from' AFTER `Activ`,
  ADD COLUMN IF NOT EXISTS `IdSubunitateParinte` int(11) NULL DEFAULT NULL COMMENT 'Parent subunit: classifications and budget are read from it (Access cai.IdUnitateParinte)' AFTER `IdUnitateAccess`,
  ADD UNIQUE INDEX IF NOT EXISTS `UK_TZ_Subunitati_UnitateAccess`(`IdUnitateAccess` ASC) USING BTREE,
  ADD INDEX IF NOT EXISTS `IX_TZ_Subunitati_Parinte`(`IdSubunitateParinte` ASC) USING BTREE,
  ADD CONSTRAINT IF NOT EXISTS `TZ_Subunitati__Parinte` FOREIGN KEY (`IdSubunitateParinte`) REFERENCES `TZ_Subunitati` (`IdSubunitate`) ON DELETE RESTRICT ON UPDATE RESTRICT;

-- Check (read only):
-- SHOW CREATE TABLE `AVACONT_SURSA`.`TZ_Subunitati`;
-- Then: AvacontPush, tab «Sincronizare schema», SAFE, on every unit database (000_DEMO ...).
