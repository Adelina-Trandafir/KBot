-- =====================================================================================
-- Slice AV01-04 (second pass, 25.10.2026) -- RUN THIS ON `AVACONT_SURSA` ONLY (Navicat / mysql, as root).
--
-- WHY: the sector + source of an account is no longer read from its subunit only. It follows the choice in the
-- title bar (SLICE-0097-06): a concrete sector/source = the account takes it; «Toate» = the operator types it on
-- the account. So the account carries its own SursaSector (the symbol already contains it).
--
-- WHAT: adds TZ_PlanCont.SursaSector (NOT NULL, foreign key to AVACONT_COMUN.DefaSursaSector). Existing rows (if any)
-- get the sector/source of their subunit. Safe to run twice. AV01_04_conturi_sursa.sql already has the column in its
-- CREATE TABLE, so a database created from it needs nothing from this file.
-- Then: AvacontPush, tab «Sincronizare schema», on every unit database.
-- =====================================================================================

ALTER TABLE `AVACONT_SURSA`.`TZ_PlanCont`
  ADD COLUMN IF NOT EXISTS `SursaSector` varchar(3) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL
    COMMENT 'Sector + source of THIS account (e.g. 01A): follows the title bar choice, editable only on «Toate». Subunit value = the default'
    AFTER `IdSubunitate`;

UPDATE `AVACONT_SURSA`.`TZ_PlanCont` P
  JOIN `AVACONT_SURSA`.`TZ_Subunitati` S ON S.`IdSubunitate` = P.`IdSubunitate`
   SET P.`SursaSector` = S.`SursaSector`
 WHERE P.`SursaSector` IS NULL;

ALTER TABLE `AVACONT_SURSA`.`TZ_PlanCont`
  MODIFY COLUMN `SursaSector` varchar(3) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL
    COMMENT 'Sector + source of THIS account (e.g. 01A): follows the title bar choice, editable only on «Toate». Subunit value = the default',
  ADD INDEX IF NOT EXISTS `IX_TZ_PlanCont_SS`(`SursaSector` ASC) USING BTREE,
  ADD CONSTRAINT `TZ_PlanCont__DefaSS` FOREIGN KEY IF NOT EXISTS (`SursaSector`) REFERENCES `AVACONT_COMUN`.`DefaSursaSector` (`SursaSector`) ON DELETE RESTRICT ON UPDATE RESTRICT;

-- Check (read only):
-- SHOW CREATE TABLE `AVACONT_SURSA`.`TZ_PlanCont`;
