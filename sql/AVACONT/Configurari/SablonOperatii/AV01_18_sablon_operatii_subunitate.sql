-- =====================================================================================
-- Slice AV01-18 -- RUN THIS ON `AVACONT_SURSA` ONLY (Navicat / mysql, as root), then «Sincronizare schema» on the unit database.
-- Needs AV01_14 (TZ_SablonOperatii) and AV01_04 (TZ_Subunitati) already run.
--
-- WHAT THIS ADDS to TZ_SablonOperatii  (operator, 02.11.2026: the subunit is the application's choice; until the port is finished the templates stay as in Access)
--   IdSubunitate   the subunit the template belongs to (TZ_Subunitati). In Access every unit had its own DefaNoteA: nothing was shared between units, so a template is
--                  kept per subunit and nothing is common for now. The column stays NULL-able only so the rows that exist now survive the ALTER (the server and the
--                  migrator always fill it; the window shows the templates of the subunit in force). The sector+source of the subunit must be the one of the journal:
--                  the SERVER checks it on save.
--
-- The foreign key is RESTRICT: a subunit with templates cannot be deleted.
-- Safe to run twice (ADD COLUMN / ADD INDEX IF NOT EXISTS, MariaDB 10.0.2+; the key is dropped and added again).
-- =====================================================================================

ALTER TABLE `AVACONT_SURSA`.`TZ_SablonOperatii`
  ADD COLUMN IF NOT EXISTS `IdSubunitate` int(11) NULL DEFAULT NULL
      COMMENT 'Subunit of the template (TZ_Subunitati); its sector+source is the one of the journal' AFTER `IdJurnal`,
  ADD INDEX IF NOT EXISTS `IX_TZ_SablonOperatii_Subunitate` (`IdSubunitate` ASC) USING BTREE;

ALTER TABLE `AVACONT_SURSA`.`TZ_SablonOperatii`
  DROP FOREIGN KEY IF EXISTS `TZ_SablonOperatii__Subunitate`;
ALTER TABLE `AVACONT_SURSA`.`TZ_SablonOperatii`
  ADD CONSTRAINT `TZ_SablonOperatii__Subunitate` FOREIGN KEY (`IdSubunitate`) REFERENCES `AVACONT_SURSA`.`TZ_Subunitati` (`IdSubunitate`) ON DELETE RESTRICT ON UPDATE RESTRICT;

-- Check (read only):
-- SHOW CREATE TABLE `AVACONT_SURSA`.`TZ_SablonOperatii`;
-- Then: AvacontPush, tab «Sincronizare schema», SAFE, on every unit database.
