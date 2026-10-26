-- =====================================================================================
-- Slice AV01-09 -- RUN THIS ON `AVACONT_SURSA` ONLY (Navicat / mysql, as root), then «Sincronizare schema» on the unit database.
--
-- WHAT THIS CREATES
--   TZ_Jurnale   the accounting journals of the unit database (the Access table `Jurnale`: CodJurnal + DenumireJurnal).
--                Every unit keeps its own list (operator, 25.10.2026, no common nomenclator) and, like the accounts, every journal belongs to a
--                SECTOR + SOURCE (SursaSector, 01A ...): the window shows the journals of the one chosen in the title bar; on «Toate» it shows all
--                of them and the sector+source is typed on the journal (operator, 26.10.2026).
--
-- RULES
--   * The code is unique PER SECTOR+SOURCE (case does not matter: utf8mb3_general_ci), written in capitals by the window: 1 to 10 of A-Z, 0-9, _.
--   * Every later table that points at a journal (notes, templates) must reference IdJurnal with ON DELETE RESTRICT: a journal that is
--     used cannot be deleted (operator, 23.10.2026); the server turns the refusal into a message. Nothing points at it yet.
--
-- NOT HERE: foreign keys FROM other tables (they come with those tables).
-- Safe to run twice (IF NOT EXISTS). utf8mb3_general_ci like every other table.
--
-- IF THE FIRST VERSION OF THIS SCRIPT (without SursaSector, unique key on Cod alone) WAS ALREADY RUN: the ALTER at the end adds the column, swaps
-- the unique key and adds the foreign key. It works only while TZ_Jurnale is EMPTY (the new column has no default, the foreign key refuses an empty
-- value). With rows in it, drop the table (nothing points at it yet) and run the script again.
-- =====================================================================================

CREATE TABLE IF NOT EXISTS `AVACONT_SURSA`.`TZ_Jurnale` (
  `IdJurnal`       int(11) NOT NULL AUTO_INCREMENT,
  `SursaSector`    varchar(3) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Sector + source of the journal (01A, 02E ...): follows the title bar choice, typed only on «Toate»',
  `Cod`            varchar(10) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Journal code, capitals (Access CodJurnal): J0D, JI, L ...',
  `Denumire`       varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '' COMMENT 'Access DenumireJurnal',
  `DataAdaugare`   datetime NOT NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdJurnal`) USING BTREE,
  UNIQUE INDEX `UK_TZ_Jurnale_SS_Cod`(`SursaSector` ASC, `Cod` ASC) USING BTREE,
  CONSTRAINT `TZ_Jurnale__DefaSS` FOREIGN KEY (`SursaSector`) REFERENCES `AVACONT_COMUN`.`DefaSursaSector` (`SursaSector`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- Only for a table made by the first version (see above). The column and the unique key are guarded; ADD CONSTRAINT has no IF NOT EXISTS in every MariaDB
-- version, so on a table that ALREADY has the foreign key this statement answers with a duplicate-constraint error: then run the ALTER again without the ADD CONSTRAINT clause (end the previous clause with the semicolon).
ALTER TABLE `AVACONT_SURSA`.`TZ_Jurnale`
  ADD COLUMN IF NOT EXISTS `SursaSector` varchar(3) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Sector + source of the journal (01A, 02E ...): follows the title bar choice, typed only on «Toate»' AFTER `IdJurnal`,
  DROP INDEX IF EXISTS `UK_TZ_Jurnale_Cod`,
  ADD UNIQUE INDEX IF NOT EXISTS `UK_TZ_Jurnale_SS_Cod`(`SursaSector` ASC, `Cod` ASC) USING BTREE,
  ADD CONSTRAINT `TZ_Jurnale__DefaSS` FOREIGN KEY (`SursaSector`) REFERENCES `AVACONT_COMUN`.`DefaSursaSector` (`SursaSector`) ON DELETE RESTRICT ON UPDATE RESTRICT;

-- Check (read only):
-- SHOW CREATE TABLE `AVACONT_SURSA`.`TZ_Jurnale`;
-- Then: AvacontPush, tab «Sincronizare schema», SAFE, on every unit database.
