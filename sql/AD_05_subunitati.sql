-- SLICE-ADE10-01: subunits inside one DC (plan: ADECHIT/plan_subunitati.md). Run ONCE per ADE unit database,
-- after AD_03 and AD_04, with a fresh backup and with ADE writes suspended (no operator, no ADE.Migrator).
-- Not run from this chat; DDL is not transactional, so a failure half-way means restoring the backup.
-- 1. Edit @ade_subunit_name below: it names the initial subunit that receives the existing data of this DC.
--    An empty DC gets no subunit here; ADE.Migrator creates one per imported MDB.
-- 2. After this script, the new application and the new ADE.Migrator are mandatory: the old ones are not compatible.
-- Receipt series/number of the initial subunit are copied from AVACONT_COMUN.Unitati_Chitante (DC = this database).
-- The common table is left untouched and no longer read by ADE.

SET @ade_subunit_name = 'Evidenta principala';

DELIMITER $$
DROP PROCEDURE IF EXISTS ad05_guard$$
CREATE PROCEDURE ad05_guard()
BEGIN
  IF EXISTS (SELECT 1 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'AD_Grupe' AND COLUMN_NAME = 'SubunitId') THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'AD_05 a fost deja aplicat pe aceasta baza.';
  END IF;
  IF (SELECT COUNT(*) FROM AD_Imports) > 1 THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'AD_Imports are mai multe loturi: datele provin din mai multe surse. Opriti conversia automata si cereti reconcilierea dupa provenienta.';
  END IF;
END$$
CALL ad05_guard()$$
DROP PROCEDURE ad05_guard$$
DELIMITER ;

-- ---- catalog, receipt configuration, id map --------------------------------------------------------------
CREATE TABLE `AD_Subunits` (
  `SubunitId` INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(100) NOT NULL,
  `Active` TINYINT(1) NOT NULL DEFAULT 1,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Version` BIGINT NOT NULL DEFAULT 1,
  PRIMARY KEY (`SubunitId`),
  UNIQUE KEY `uq_Subunit_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE `AD_ReceiptConfig` (
  `SubunitId` INT NOT NULL,
  `Serie` VARCHAR(50) NOT NULL,
  `Numar` INT NOT NULL,
  `Explicatie` VARCHAR(255) NOT NULL,
  `Version` BIGINT NOT NULL DEFAULT 1,
  PRIMARY KEY (`SubunitId`),
  CONSTRAINT `fk_ReceiptConfig_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE `AD_IdMap` (
  `SubunitId` INT NOT NULL,
  `ImportHash` CHAR(64) NOT NULL,
  `TableName` VARCHAR(40) NOT NULL,
  `SourceId` INT NOT NULL,
  `TargetId` INT NOT NULL,
  PRIMARY KEY (`SubunitId`, `TableName`, `SourceId`),
  KEY `ix_IdMap_Hash` (`ImportHash`),
  CONSTRAINT `fk_IdMap_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- Per-user restriction: a subunit WITHOUT rows here is open to every user with ADE rights in the DC; a subunit with rows is open only to the listed e-mails.
CREATE TABLE `AD_SubunitAccess` (
  `SubunitId` INT NOT NULL,
  `Email` VARCHAR(190) NOT NULL,
  PRIMARY KEY (`SubunitId`, `Email`),
  CONSTRAINT `fk_SubunitAccess_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- ---- initial subunit for a DC that already holds data -----------------------------------------------------
INSERT INTO AD_Subunits (Name)
SELECT @ade_subunit_name FROM DUAL
WHERE EXISTS (SELECT 1 FROM AD_Grupe) OR EXISTS (SELECT 1 FROM AD_ValoriTaxe) OR EXISTS (SELECT 1 FROM AD_LunaD)
   OR EXISTS (SELECT 1 FROM AD_Platitori) OR EXISTS (SELECT 1 FROM AD_Imports) OR EXISTS (SELECT 1 FROM AD_Operations);
SET @ade_sub = (SELECT MIN(SubunitId) FROM AD_Subunits);
INSERT INTO AD_ReceiptConfig (SubunitId, Serie, Numar, Explicatie)
SELECT @ade_sub, Serie, Numar, Explicatie FROM AVACONT_COMUN.Unitati_Chitante WHERE DC = DATABASE() AND @ade_sub IS NOT NULL;
-- One mutation lock row per subunit: its id is the subunit id.
INSERT IGNORE INTO AD_Lock (ID) SELECT SubunitId FROM AD_Subunits;
-- Settings of an empty DC were only the seeded default (BlockReopenWithMovements); the code falls back to it.
DELETE FROM AD_Settings WHERE @ade_sub IS NULL;

-- ---- SubunitId on every ADE table -------------------------------------------------------------------------
ALTER TABLE `AD_Grupe` ADD COLUMN `SubunitId` INT NULL;
UPDATE `AD_Grupe` SET `SubunitId` = @ade_sub;
ALTER TABLE `AD_Grupe` MODIFY `SubunitId` INT NOT NULL;
ALTER TABLE `AD_ValoriTaxe` ADD COLUMN `SubunitId` INT NULL;
UPDATE `AD_ValoriTaxe` SET `SubunitId` = @ade_sub;
ALTER TABLE `AD_ValoriTaxe` MODIFY `SubunitId` INT NOT NULL;
ALTER TABLE `AD_LunaD` ADD COLUMN `SubunitId` INT NULL;
UPDATE `AD_LunaD` SET `SubunitId` = @ade_sub;
ALTER TABLE `AD_LunaD` MODIFY `SubunitId` INT NOT NULL;
ALTER TABLE `AD_Platitori` ADD COLUMN `SubunitId` INT NULL;
UPDATE `AD_Platitori` SET `SubunitId` = @ade_sub;
ALTER TABLE `AD_Platitori` MODIFY `SubunitId` INT NOT NULL;
ALTER TABLE `AD_Platitori_sub` ADD COLUMN `SubunitId` INT NULL;
UPDATE `AD_Platitori_sub` SET `SubunitId` = @ade_sub;
ALTER TABLE `AD_Platitori_sub` MODIFY `SubunitId` INT NOT NULL;
ALTER TABLE `AD_Prezenta` ADD COLUMN `SubunitId` INT NULL;
UPDATE `AD_Prezenta` SET `SubunitId` = @ade_sub;
ALTER TABLE `AD_Prezenta` MODIFY `SubunitId` INT NOT NULL;
ALTER TABLE `AD_Plati` ADD COLUMN `SubunitId` INT NULL;
UPDATE `AD_Plati` SET `SubunitId` = @ade_sub;
ALTER TABLE `AD_Plati` MODIFY `SubunitId` INT NOT NULL;
ALTER TABLE `AD_Chitante` ADD COLUMN `SubunitId` INT NULL;
UPDATE `AD_Chitante` SET `SubunitId` = @ade_sub;
ALTER TABLE `AD_Chitante` MODIFY `SubunitId` INT NOT NULL;
ALTER TABLE `AD_AlteDoc` ADD COLUMN `SubunitId` INT NULL;
UPDATE `AD_AlteDoc` SET `SubunitId` = @ade_sub;
ALTER TABLE `AD_AlteDoc` MODIFY `SubunitId` INT NOT NULL;
ALTER TABLE `AD_Retur` ADD COLUMN `SubunitId` INT NULL;
UPDATE `AD_Retur` SET `SubunitId` = @ade_sub;
ALTER TABLE `AD_Retur` MODIFY `SubunitId` INT NOT NULL;
ALTER TABLE `AD_SS_Buget` ADD COLUMN `SubunitId` INT NULL;
UPDATE `AD_SS_Buget` SET `SubunitId` = @ade_sub;
ALTER TABLE `AD_SS_Buget` MODIFY `SubunitId` INT NOT NULL;
ALTER TABLE `AD_Grupe_Educator` ADD COLUMN `SubunitId` INT NULL;
UPDATE `AD_Grupe_Educator` SET `SubunitId` = @ade_sub;
ALTER TABLE `AD_Grupe_Educator` MODIFY `SubunitId` INT NOT NULL;
ALTER TABLE `AD_Platitori_Istoric` ADD COLUMN `SubunitId` INT NULL;
UPDATE `AD_Platitori_Istoric` SET `SubunitId` = @ade_sub;
ALTER TABLE `AD_Platitori_Istoric` MODIFY `SubunitId` INT NOT NULL;
ALTER TABLE `AD_Compensare` ADD COLUMN `SubunitId` INT NULL;
UPDATE `AD_Compensare` SET `SubunitId` = @ade_sub;
ALTER TABLE `AD_Compensare` MODIFY `SubunitId` INT NOT NULL;
ALTER TABLE `AD_Settings` ADD COLUMN `SubunitId` INT NULL;
UPDATE `AD_Settings` SET `SubunitId` = @ade_sub;
ALTER TABLE `AD_Settings` MODIFY `SubunitId` INT NOT NULL;
ALTER TABLE `AD_Operations` ADD COLUMN `SubunitId` INT NULL;
UPDATE `AD_Operations` SET `SubunitId` = @ade_sub;
ALTER TABLE `AD_Operations` MODIFY `SubunitId` INT NOT NULL;
ALTER TABLE `AD_Imports` ADD COLUMN `SubunitId` INT NULL;
UPDATE `AD_Imports` SET `SubunitId` = @ade_sub;
ALTER TABLE `AD_Imports` MODIFY `SubunitId` INT NOT NULL;

-- Local month order: the Access rule "IDL + 1 >= last attendance month" is evaluated on this value, not on the
-- surrogate IDL, which the server allocates (plan section 5). Existing rows keep Ordine = IDL.
ALTER TABLE `AD_LunaD` ADD COLUMN `Ordine` INT NULL AFTER `IDL`;
UPDATE `AD_LunaD` SET `Ordine` = `IDL`;

-- ---- keys that were DC-wide become subunit-wide ----------------------------------------------------------
ALTER TABLE `AD_LunaD` DROP INDEX `uq_Luna_Anul`, ADD UNIQUE KEY `uq_Sub_Luna_Anul` (`SubunitId`, `Luna`, `Anul`);
ALTER TABLE `AD_Chitante` DROP INDEX `uq_Serie_Numar`, ADD UNIQUE KEY `uq_Sub_Serie_Numar` (`SubunitId`, `Serie`, `Numar`);
ALTER TABLE `AD_Settings` DROP PRIMARY KEY, ADD PRIMARY KEY (`SubunitId`, `SettingKey`);
ALTER TABLE `AD_Operations` DROP PRIMARY KEY, ADD PRIMARY KEY (`SubunitId`, `RequestKey`);
-- AD_Imports keeps SourceHash as its key: the same MDB cannot be imported twice in one DC.

-- ---- composite keys: a relation can only join rows of the same subunit ---------------------------------------
ALTER TABLE `AD_Grupe` ADD UNIQUE KEY `uq_Sub_IDG` (`SubunitId`, `IDG`);
ALTER TABLE `AD_ValoriTaxe` ADD UNIQUE KEY `uq_Sub_IDV` (`SubunitId`, `IDV`);
ALTER TABLE `AD_Platitori` ADD UNIQUE KEY `uq_Sub_IDP` (`SubunitId`, `IDP`);
ALTER TABLE `AD_Platitori_sub` ADD UNIQUE KEY `uq_Sub_IDS` (`SubunitId`, `IDS`);
ALTER TABLE `AD_LunaD` ADD UNIQUE KEY `uq_Sub_IDL` (`SubunitId`, `IDL`);
ALTER TABLE `AD_Prezenta` ADD UNIQUE KEY `uq_Sub_IDZ` (`SubunitId`, `IDZ`);
ALTER TABLE `AD_Plati` ADD UNIQUE KEY `uq_Sub_IDPL` (`SubunitId`, `IDPL`);

ALTER TABLE `AD_AlteDoc` DROP FOREIGN KEY `fk_AlteDoc_IDL`;
ALTER TABLE `AD_AlteDoc` DROP FOREIGN KEY `fk_AlteDoc_IDPL`;
ALTER TABLE `AD_Chitante` DROP FOREIGN KEY `fk_Chitante_IDL`;
ALTER TABLE `AD_Chitante` DROP FOREIGN KEY `fk_Chitante_IDPL`;
ALTER TABLE `AD_Compensare` DROP FOREIGN KEY `fk_Compensare_Credit`;
ALTER TABLE `AD_Compensare` DROP FOREIGN KEY `fk_Compensare_Debit`;
ALTER TABLE `AD_Grupe_Educator` DROP FOREIGN KEY `fk_Grupe_Educator_IDG`;
ALTER TABLE `AD_LunaD` DROP FOREIGN KEY `fk_LunaD_IDV`;
ALTER TABLE `AD_Plati` DROP FOREIGN KEY `fk_Plati_IDL`;
ALTER TABLE `AD_Plati` DROP FOREIGN KEY `fk_Plati_IDP`;
ALTER TABLE `AD_Plati` DROP FOREIGN KEY `fk_Plati_IDS`;
ALTER TABLE `AD_Plati` DROP FOREIGN KEY `fk_Plati_IDZ`;
ALTER TABLE `AD_Platitori` DROP FOREIGN KEY `fk_Platitori_IDG`;
ALTER TABLE `AD_Platitori_Istoric` DROP FOREIGN KEY `fk_Istoric_IDG_Nou`;
ALTER TABLE `AD_Platitori_Istoric` DROP FOREIGN KEY `fk_Istoric_IDG_Vechi`;
ALTER TABLE `AD_Platitori_Istoric` DROP FOREIGN KEY `fk_Istoric_IDP`;
ALTER TABLE `AD_Platitori_sub` DROP FOREIGN KEY `fk_Platitori_sub_IDP`;
ALTER TABLE `AD_Prezenta` DROP FOREIGN KEY `fk_Prezenta_IDG`;
ALTER TABLE `AD_Prezenta` DROP FOREIGN KEY `fk_Prezenta_IDL`;
ALTER TABLE `AD_Prezenta` DROP FOREIGN KEY `fk_Prezenta_IDP`;
ALTER TABLE `AD_Prezenta` DROP FOREIGN KEY `fk_Prezenta_IDV`;
ALTER TABLE `AD_Retur` DROP FOREIGN KEY `fk_Retur_IDL`;
ALTER TABLE `AD_Retur` DROP FOREIGN KEY `fk_Retur_IDP`;
ALTER TABLE `AD_Retur` DROP FOREIGN KEY `fk_Retur_IDS`;
ALTER TABLE `AD_Retur` DROP FOREIGN KEY `fk_Retur_IDZ`;
ALTER TABLE `AD_SS_Buget` DROP FOREIGN KEY `fk_SS_Buget_IDG`;
ALTER TABLE `AD_SS_Buget` DROP FOREIGN KEY `fk_SS_Buget_IDL`;
ALTER TABLE `AD_SS_Buget` DROP FOREIGN KEY `fk_SS_Buget_IDP`;
ALTER TABLE `AD_SS_Buget` DROP FOREIGN KEY `fk_SS_Buget_IDZ`;

ALTER TABLE `AD_AlteDoc` ADD CONSTRAINT `fk_AlteDoc_IDL` FOREIGN KEY (`SubunitId`, `IDL`) REFERENCES `AD_LunaD` (`SubunitId`, `IDL`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_AlteDoc` ADD CONSTRAINT `fk_AlteDoc_IDPL` FOREIGN KEY (`SubunitId`, `IDPL`) REFERENCES `AD_Plati` (`SubunitId`, `IDPL`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Chitante` ADD CONSTRAINT `fk_Chitante_IDL` FOREIGN KEY (`SubunitId`, `IDL`) REFERENCES `AD_LunaD` (`SubunitId`, `IDL`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Chitante` ADD CONSTRAINT `fk_Chitante_IDPL` FOREIGN KEY (`SubunitId`, `IDPL`) REFERENCES `AD_Plati` (`SubunitId`, `IDPL`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Compensare` ADD CONSTRAINT `fk_Compensare_Credit` FOREIGN KEY (`SubunitId`, `IDP_Credit`) REFERENCES `AD_Platitori` (`SubunitId`, `IDP`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Compensare` ADD CONSTRAINT `fk_Compensare_Debit` FOREIGN KEY (`SubunitId`, `IDP_Debit`) REFERENCES `AD_Platitori` (`SubunitId`, `IDP`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Grupe_Educator` ADD CONSTRAINT `fk_Grupe_Educator_IDG` FOREIGN KEY (`SubunitId`, `IDG`) REFERENCES `AD_Grupe` (`SubunitId`, `IDG`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_LunaD` ADD CONSTRAINT `fk_LunaD_IDV` FOREIGN KEY (`SubunitId`, `IDV`) REFERENCES `AD_ValoriTaxe` (`SubunitId`, `IDV`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Plati` ADD CONSTRAINT `fk_Plati_IDL` FOREIGN KEY (`SubunitId`, `IDL`) REFERENCES `AD_LunaD` (`SubunitId`, `IDL`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Plati` ADD CONSTRAINT `fk_Plati_IDP` FOREIGN KEY (`SubunitId`, `IDP`) REFERENCES `AD_Platitori` (`SubunitId`, `IDP`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Plati` ADD CONSTRAINT `fk_Plati_IDS` FOREIGN KEY (`SubunitId`, `IDS`) REFERENCES `AD_Platitori_sub` (`SubunitId`, `IDS`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Plati` ADD CONSTRAINT `fk_Plati_IDZ` FOREIGN KEY (`SubunitId`, `IDZ`) REFERENCES `AD_Prezenta` (`SubunitId`, `IDZ`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Platitori` ADD CONSTRAINT `fk_Platitori_IDG` FOREIGN KEY (`SubunitId`, `IDG`) REFERENCES `AD_Grupe` (`SubunitId`, `IDG`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Platitori_Istoric` ADD CONSTRAINT `fk_Istoric_IDG_Nou` FOREIGN KEY (`SubunitId`, `IDG_Nou`) REFERENCES `AD_Grupe` (`SubunitId`, `IDG`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Platitori_Istoric` ADD CONSTRAINT `fk_Istoric_IDG_Vechi` FOREIGN KEY (`SubunitId`, `IDG_Vechi`) REFERENCES `AD_Grupe` (`SubunitId`, `IDG`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Platitori_Istoric` ADD CONSTRAINT `fk_Istoric_IDP` FOREIGN KEY (`SubunitId`, `IDP`) REFERENCES `AD_Platitori` (`SubunitId`, `IDP`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Platitori_sub` ADD CONSTRAINT `fk_Platitori_sub_IDP` FOREIGN KEY (`SubunitId`, `IDP`) REFERENCES `AD_Platitori` (`SubunitId`, `IDP`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Prezenta` ADD CONSTRAINT `fk_Prezenta_IDG` FOREIGN KEY (`SubunitId`, `IDG`) REFERENCES `AD_Grupe` (`SubunitId`, `IDG`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Prezenta` ADD CONSTRAINT `fk_Prezenta_IDL` FOREIGN KEY (`SubunitId`, `IDL`) REFERENCES `AD_LunaD` (`SubunitId`, `IDL`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Prezenta` ADD CONSTRAINT `fk_Prezenta_IDP` FOREIGN KEY (`SubunitId`, `IDP`) REFERENCES `AD_Platitori` (`SubunitId`, `IDP`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Prezenta` ADD CONSTRAINT `fk_Prezenta_IDV` FOREIGN KEY (`SubunitId`, `IDV`) REFERENCES `AD_ValoriTaxe` (`SubunitId`, `IDV`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Retur` ADD CONSTRAINT `fk_Retur_IDL` FOREIGN KEY (`SubunitId`, `IDL`) REFERENCES `AD_LunaD` (`SubunitId`, `IDL`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Retur` ADD CONSTRAINT `fk_Retur_IDP` FOREIGN KEY (`SubunitId`, `IDP`) REFERENCES `AD_Platitori` (`SubunitId`, `IDP`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Retur` ADD CONSTRAINT `fk_Retur_IDS` FOREIGN KEY (`SubunitId`, `IDS`) REFERENCES `AD_Platitori_sub` (`SubunitId`, `IDS`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Retur` ADD CONSTRAINT `fk_Retur_IDZ` FOREIGN KEY (`SubunitId`, `IDZ`) REFERENCES `AD_Prezenta` (`SubunitId`, `IDZ`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_SS_Buget` ADD CONSTRAINT `fk_SS_Buget_IDG` FOREIGN KEY (`SubunitId`, `IDG`) REFERENCES `AD_Grupe` (`SubunitId`, `IDG`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_SS_Buget` ADD CONSTRAINT `fk_SS_Buget_IDL` FOREIGN KEY (`SubunitId`, `IDL`) REFERENCES `AD_LunaD` (`SubunitId`, `IDL`) ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE `AD_SS_Buget` ADD CONSTRAINT `fk_SS_Buget_IDP` FOREIGN KEY (`SubunitId`, `IDP`) REFERENCES `AD_Platitori` (`SubunitId`, `IDP`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_SS_Buget` ADD CONSTRAINT `fk_SS_Buget_IDZ` FOREIGN KEY (`SubunitId`, `IDZ`) REFERENCES `AD_Prezenta` (`SubunitId`, `IDZ`) ON DELETE CASCADE ON UPDATE CASCADE;

-- Every ADE table belongs to an existing subunit.
ALTER TABLE `AD_Grupe` ADD CONSTRAINT `fk_Grupe_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_ValoriTaxe` ADD CONSTRAINT `fk_ValoriTaxe_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_LunaD` ADD CONSTRAINT `fk_LunaD_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Platitori` ADD CONSTRAINT `fk_Platitori_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Platitori_sub` ADD CONSTRAINT `fk_Platitori_sub_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Prezenta` ADD CONSTRAINT `fk_Prezenta_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Plati` ADD CONSTRAINT `fk_Plati_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Chitante` ADD CONSTRAINT `fk_Chitante_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_AlteDoc` ADD CONSTRAINT `fk_AlteDoc_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Retur` ADD CONSTRAINT `fk_Retur_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_SS_Buget` ADD CONSTRAINT `fk_SS_Buget_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Grupe_Educator` ADD CONSTRAINT `fk_Grupe_Educator_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Platitori_Istoric` ADD CONSTRAINT `fk_Platitori_Istoric_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Compensare` ADD CONSTRAINT `fk_Compensare_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Settings` ADD CONSTRAINT `fk_Settings_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Operations` ADD CONSTRAINT `fk_Operations_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE `AD_Imports` ADD CONSTRAINT `fk_Imports_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE;

-- Checks to read after the run (expected: every count equals the count before the script).
-- SELECT SubunitId, Name FROM AD_Subunits;
-- SELECT 'Grupe', COUNT(*) FROM AD_Grupe UNION ALL SELECT 'LunaD', COUNT(*) FROM AD_LunaD UNION ALL SELECT 'Platitori', COUNT(*) FROM AD_Platitori
--   UNION ALL SELECT 'Prezenta', COUNT(*) FROM AD_Prezenta UNION ALL SELECT 'Plati', COUNT(*) FROM AD_Plati UNION ALL SELECT 'Chitante', COUNT(*) FROM AD_Chitante;
-- SELECT * FROM AD_ReceiptConfig;
