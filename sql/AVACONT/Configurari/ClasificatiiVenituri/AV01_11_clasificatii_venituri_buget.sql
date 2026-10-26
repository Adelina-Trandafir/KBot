-- =====================================================================================
-- Slice AV01-11 -- RUN THIS ON `AVACONT_SURSA` ONLY (Navicat / mysql, as root), then «Sincronizare schema» on the unit database.
--
-- WHAT THIS DOES (the «Venituri» page of the «Clasificatii» window, docs/access-map/avacont/proiect-de-date-etapa-1.md section 4)
--   1. Clasificatii_Venituri_Buget       NEW. The budget of a revenue classification for a UNIT and a YEAR as VERSIONS (the same model as
--                                        Clasificatii_Buget): one row = the budget from DataInceput on, four quarters, TOTAL generated.
--   2. Clasificatii_Venituri_Rectificari ALTER. Trim1..Trim4 become numbers (they were varchar(255)), IdUnitate is added (the correction
--                                        belongs to the unit like the budget does), and one correction is unique per
--                                        classification + unit + date + document. The year of a correction is YEAR(Data), as on Clasificatii_Rectificari.
--   3. Clasificatii_Venituri             unique (Capitol, SubCapitol, Paragraf): the window adds a classification only from the nomenclator
--                                        DefaClsfV, one row per code.
--
-- WHY IdUnitate: Clasificatii_Venituri has none (it holds the revenue classifications of the whole directorate, extrase.py reads it without a
-- unit), but the money is per unit: the unit is the row of `Unitati` (sector+source + year) that the title bar of the main window names.
--
-- Clasificatii_Venituri ITSELF IS NOT CHANGED (its Trim1..Trim4 stay, unused): extrase.py and the Access migration read it as it is.
--
-- BEFORE YOU RUN: statement 2 changes the type of four columns. It is meant for an EMPTY table (the table was never filled: the window
-- did not exist; the Access RectificariV has 0 rows). Check first:   SELECT COUNT(*) FROM `AVACONT_SURSA`.`Clasificatii_Venituri_Rectificari`;
-- With rows in it the text of every quarter is converted to a number by MariaDB (a text that is not a number becomes 0 with a warning).
-- Statement 4 fails with a duplicate-key error if two rows already have the same Capitol+SubCapitol+Paragraf: remove the double first.
--
-- Safe to run twice except the foreign key of statement 3 (see there). utf8mb3_general_ci like every other table.
-- =====================================================================================

-- 1. The budget versions.
CREATE TABLE IF NOT EXISTS `AVACONT_SURSA`.`Clasificatii_Venituri_Buget` (
  `IdBuget`        int(11) NOT NULL AUTO_INCREMENT,
  `IdClsfV`        int(11) UNSIGNED NOT NULL,
  `IdUnitate`      int(11) NOT NULL,
  `An`             int(4) NOT NULL,
  `DataInceput`    date NOT NULL COMMENT 'The day this budget row starts to apply (one row = one version of the budget)',
  `Trim1`          double NULL DEFAULT NULL,
  `Trim2`          double NULL DEFAULT NULL,
  `Trim3`          double NULL DEFAULT NULL,
  `Trim4`          double NULL DEFAULT NULL,
  `TOTAL`          double GENERATED ALWAYS AS (coalesce(`Trim1`,0) + coalesce(`Trim2`,0) + coalesce(`Trim3`,0) + coalesce(`Trim4`,0)) PERSISTENT,
  `DataAdaugare`   datetime NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdBuget`) USING BTREE,
  UNIQUE INDEX `UK_CV_Buget_Clsf_Unit_An_Data`(`IdClsfV` ASC, `IdUnitate` ASC, `An` ASC, `DataInceput` ASC) USING BTREE,
  INDEX `Clasificatii_Venituri_Buget_ibfk_2`(`IdUnitate` ASC) USING BTREE,
  CONSTRAINT `Clasificatii_Venituri_Buget_ibfk_1` FOREIGN KEY (`IdClsfV`) REFERENCES `Clasificatii_Venituri` (`IdClsfV`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `Clasificatii_Venituri_Buget_ibfk_2` FOREIGN KEY (`IdUnitate`) REFERENCES `Unitati` (`IdUnitate`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- 2. The corrections: numbers instead of text, the unit, the unique key.
ALTER TABLE `AVACONT_SURSA`.`Clasificatii_Venituri_Rectificari`
  MODIFY COLUMN `Trim1` double NULL DEFAULT NULL,
  MODIFY COLUMN `Trim2` double NULL DEFAULT NULL,
  MODIFY COLUMN `Trim3` double NULL DEFAULT NULL,
  MODIFY COLUMN `Trim4` double NULL DEFAULT NULL,
  ADD COLUMN IF NOT EXISTS `IdUnitate` int(11) NULL DEFAULT NULL COMMENT 'The unit (row of Unitati) whose budget this correction changes' AFTER `IdClsfV`,
  ADD UNIQUE INDEX IF NOT EXISTS `UK_CV_Rectificari_Clsf_Unit_Data_Doc`(`IdClsfV` ASC, `IdUnitate` ASC, `Data` ASC, `Document` ASC) USING BTREE;

-- 2b. KEEP the old index of the foreign key. The unique index of statement 2 starts with IdClsfV, so MariaDB SILENTLY DROPS the older index
-- `Clasificatii_Venituri__Clasificatii_Venituri_Rectificari` (IdClsfV) from the template; the schema sync then plans a DROP INDEX on the unit databases,
-- and that DROP fails there (errno 1553: the foreign key of the unit database still needs it, the new unique index is not there yet - the sync drops before it adds).
-- Putting the index back makes the template equal to the unit databases on it, so the sync has nothing to drop. A redundant index is harmless.
ALTER TABLE `AVACONT_SURSA`.`Clasificatii_Venituri_Rectificari`
  ADD INDEX IF NOT EXISTS `Clasificatii_Venituri__Clasificatii_Venituri_Rectificari`(`IdClsfV` ASC) USING BTREE;

-- 3. The foreign key of the new column. ADD CONSTRAINT has no IF NOT EXISTS in every MariaDB version: run this statement ONCE; a second run
-- answers with a duplicate-constraint error, which is harmless.
ALTER TABLE `AVACONT_SURSA`.`Clasificatii_Venituri_Rectificari`
  ADD CONSTRAINT `Clasificatii_Venituri_Rectificari_ibfk_unit` FOREIGN KEY (`IdUnitate`) REFERENCES `Unitati` (`IdUnitate`) ON DELETE RESTRICT ON UPDATE RESTRICT;

-- 4. One row per code.
ALTER TABLE `AVACONT_SURSA`.`Clasificatii_Venituri`
  ADD UNIQUE INDEX IF NOT EXISTS `UK_Clasificatii_Venituri_CSP`(`Capitol` ASC, `SubCapitol` ASC, `Paragraf` ASC) USING BTREE;

-- Check (read only):
-- SHOW CREATE TABLE `AVACONT_SURSA`.`Clasificatii_Venituri_Buget`;
-- SHOW CREATE TABLE `AVACONT_SURSA`.`Clasificatii_Venituri_Rectificari`;
-- Then: AvacontPush, tab «Sincronizare schema», SAFE, on every unit database.
