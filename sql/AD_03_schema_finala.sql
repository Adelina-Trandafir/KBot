-- SLICE-ADE0-07. Final schema of the kindergarten module (prefix AD_), for AVACONT_SURSA.
-- Charset matches the server (utf8mb3). No USE, no DROP, no data seeds except the two technical rows.
--
-- This file REPLACES sql/AD_01_schema.sql: run it on a database where no AD_ table exists yet
-- (ADE0-06: nothing was ever created on the server). If AD_01 was already run, only the three
-- ALTER statements at the end are needed; the old AD_Grupe.Educator column is dropped by hand
-- AFTER the educator history has been migrated (see the end of the file).
-- AVACONT_COMUN.Unitati_Chitante stays in sql/AD_02_chitante.sql (other database).
-- Not validated on MariaDB.
--
-- Decisions behind the changes versus AD_01: ADECHIT/DECIZII_DESCHISE.md M03 and M06.

-- ---------------------------------------------------------------- groups

CREATE TABLE IF NOT EXISTS `AD_Grupe` (
  `IDG` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `Grupa` VARCHAR(50) NULL,
  `Tip` VARCHAR(10) NOT NULL DEFAULT 'NORMALA',        -- NORMALA | PLECATI (the special "departed children" group)
  `InchisaDinAn` INT NULL DEFAULT NULL,                -- NULL = active; else the school year (start year) from which it is no longer offered
  `Version` BIGINT NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE IF NOT EXISTS `AD_Grupe_Educator` (
  `IDGE` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `IDG` INT NOT NULL,
  `Educator` VARCHAR(50) NULL,
  `DeLa` DATE NULL,
  `PanaLa` DATE NULL,                                  -- NULL = still current
  `Version` BIGINT NOT NULL DEFAULT 1,
  INDEX `ix_IDG` (`IDG`),
  CONSTRAINT `fk_Grupe_Educator_IDG` FOREIGN KEY (`IDG`) REFERENCES `AD_Grupe` (`IDG`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- ---------------------------------------------------------------- fee sets and months

CREATE TABLE IF NOT EXISTS `AD_ValoriTaxe` (
  `IDV` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `TaxaZilnica` DOUBLE NULL DEFAULT 0,
  `Activ` BOOLEAN NULL DEFAULT 0,
  `Expl` VARCHAR(255) NULL,
  `Version` BIGINT NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE IF NOT EXISTS `AD_LunaD` (
  `IDL` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `IDV` INT NULL DEFAULT NULL,
  `Luna` INT NULL DEFAULT 0,
  `Anul` INT NULL DEFAULT 0,
  `LunaT` VARCHAR(255) NULL,
  `LA` VARCHAR(255) NULL,
  `Inchisa` BOOLEAN NULL DEFAULT 0,
  `ZileLuna` INT NULL,
  `Version` BIGINT NOT NULL DEFAULT 1,
  INDEX `ix_IDV` (`IDV`),
  CONSTRAINT `fk_LunaD_IDV` FOREIGN KEY (`IDV`) REFERENCES `AD_ValoriTaxe` (`IDV`) ON DELETE RESTRICT ON UPDATE CASCADE,
  UNIQUE KEY `uq_Luna_Anul` (`Luna`,`Anul`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- ---------------------------------------------------------------- children

CREATE TABLE IF NOT EXISTS `AD_Platitori` (
  `IDP` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `IDG` INT NULL DEFAULT NULL,
  `Nume` VARCHAR(255) NULL,
  `CNP` VARCHAR(255) NULL,
  `Plecat` BOOLEAN NULL DEFAULT 0,
  `SI` INT NULL DEFAULT 0,
  `DataIntrare` DATETIME NULL,
  `DataIesire` DATETIME NULL,
  `Version` BIGINT NOT NULL DEFAULT 1,
  INDEX `ix_IDG` (`IDG`),
  CONSTRAINT `fk_Platitori_IDG` FOREIGN KEY (`IDG`) REFERENCES `AD_Grupe` (`IDG`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE IF NOT EXISTS `AD_Platitori_sub` (
  `IDS` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `IDP` INT NULL DEFAULT NULL,
  `Nume` VARCHAR(50) NULL,
  `Adresa` VARCHAR(255) NULL,
  `CUI` VARCHAR(255) NULL,
  `Cont` VARCHAR(255) NULL,
  `Banca` VARCHAR(255) NULL,
  `EMail` VARCHAR(255) NULL,
  `Telefon` VARCHAR(50) NULL,                          -- new: not in Access, stays NULL at migration
  `TrimiteMail` BOOLEAN NULL DEFAULT 0,
  `Activ` BOOLEAN NULL DEFAULT 0,
  `CNP_Platitor` VARCHAR(255) NULL,
  `Version` BIGINT NOT NULL DEFAULT 1,
  INDEX `ix_IDP` (`IDP`),
  CONSTRAINT `fk_Platitori_sub_IDP` FOREIGN KEY (`IDP`) REFERENCES `AD_Platitori` (`IDP`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- Append-only log of what happened to a child; written in the same transaction as the change.
CREATE TABLE IF NOT EXISTS `AD_Platitori_Istoric` (
  `IDI` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `IDP` INT NOT NULL,
  `Data` DATETIME NOT NULL,
  `Tip` VARCHAR(20) NOT NULL,                          -- INTRARE | MUTARE | PLECARE | REVENIRE | IESIRE_DEFINITIVA | COMPENSARE
  `IDG_Vechi` INT NULL DEFAULT NULL,
  `IDG_Nou` INT NULL DEFAULT NULL,
  `Dedus` BOOLEAN NOT NULL DEFAULT 0,                  -- 1 = deduced at migration from consecutive months (month only, no exact day)
  `Utilizator` VARCHAR(80) NULL,
  `Nota` VARCHAR(255) NULL,
  INDEX `ix_IDP_Data` (`IDP`,`Data`),
  INDEX `ix_IDG_Vechi` (`IDG_Vechi`),
  INDEX `ix_IDG_Nou` (`IDG_Nou`),
  CONSTRAINT `fk_Istoric_IDP` FOREIGN KEY (`IDP`) REFERENCES `AD_Platitori` (`IDP`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_Istoric_IDG_Vechi` FOREIGN KEY (`IDG_Vechi`) REFERENCES `AD_Grupe` (`IDG`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_Istoric_IDG_Nou` FOREIGN KEY (`IDG_Nou`) REFERENCES `AD_Grupe` (`IDG`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- ---------------------------------------------------------------- monthly attendance and saved statement

CREATE TABLE IF NOT EXISTS `AD_Prezenta` (
  `IDZ` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `IDP` INT NULL DEFAULT NULL,
  `IDL` INT NULL DEFAULT NULL,
  `IDV` INT NULL DEFAULT NULL,
  `IDG` INT NULL DEFAULT NULL,                         -- the group the child is in when the month is closed
  `ZilePrezenta` INT NULL DEFAULT 0,
  `ValoareContract` DOUBLE NULL DEFAULT 0,
  `ValoareTotala` INT NULL DEFAULT 0,
  `Version` BIGINT NOT NULL DEFAULT 1,
  INDEX `ix_IDP` (`IDP`),
  INDEX `ix_IDL` (`IDL`),
  INDEX `ix_IDG` (`IDG`),
  INDEX `ix_IDV` (`IDV`),
  CONSTRAINT `fk_Prezenta_IDP` FOREIGN KEY (`IDP`) REFERENCES `AD_Platitori` (`IDP`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_Prezenta_IDL` FOREIGN KEY (`IDL`) REFERENCES `AD_LunaD` (`IDL`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_Prezenta_IDV` FOREIGN KEY (`IDV`) REFERENCES `AD_ValoriTaxe` (`IDV`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_Prezenta_IDG` FOREIGN KEY (`IDG`) REFERENCES `AD_Grupe` (`IDG`) ON DELETE RESTRICT ON UPDATE CASCADE,
  UNIQUE KEY `uq_IDP_IDL` (`IDP`,`IDL`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE IF NOT EXISTS `AD_SS_Buget` (
  `ID` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `IDG` INT NULL DEFAULT NULL,
  `IDP` INT NULL DEFAULT NULL,
  `IDL` INT NULL DEFAULT NULL,
  `IDZ` INT NULL DEFAULT NULL,
  `Luna` VARCHAR(255) NULL,
  `Anul` INT NULL,
  `Nume` VARCHAR(255) NULL,
  `CNP` VARCHAR(255) NULL,
  `ZilePrezenta` INT NULL,
  `SID` INT NULL DEFAULT 0,
  `SIC` INT NULL DEFAULT 0,
  `ValoareContract` DOUBLE NULL,
  `ValoareTotala` INT NULL,
  `Plata` DOUBLE NULL,
  `Restanta` DOUBLE NULL,
  `Compensare` DOUBLE NULL,
  `Anticipat` DOUBLE NULL,
  `Plati` INT NULL DEFAULT 0,
  `Retur` INT NULL DEFAULT 0,
  `SFD` INT NULL DEFAULT 0,
  `SFC` INT NULL DEFAULT 0,
  `Detalii` LONGTEXT NULL,
  `Plecat` BOOLEAN NULL DEFAULT 0,
  `Educator` VARCHAR(255) NULL,                        -- text as it was in the saved month
  `Grupa` VARCHAR(255) NULL,                           -- text as it was in the saved month
  `Version` BIGINT NOT NULL DEFAULT 1,
  INDEX `ix_IDP` (`IDP`),
  INDEX `ix_IDL` (`IDL`),
  INDEX `ix_IDZ` (`IDZ`),
  INDEX `ix_IDG` (`IDG`),
  CONSTRAINT `fk_SS_Buget_IDG` FOREIGN KEY (`IDG`) REFERENCES `AD_Grupe` (`IDG`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_SS_Buget_IDP` FOREIGN KEY (`IDP`) REFERENCES `AD_Platitori` (`IDP`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_SS_Buget_IDL` FOREIGN KEY (`IDL`) REFERENCES `AD_LunaD` (`IDL`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_SS_Buget_IDZ` FOREIGN KEY (`IDZ`) REFERENCES `AD_Prezenta` (`IDZ`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- ---------------------------------------------------------------- payments, receipts, other documents, refunds

CREATE TABLE IF NOT EXISTS `AD_Plati` (
  `IDPL` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `IDP` INT NULL DEFAULT NULL,
  `IDZ` INT NULL DEFAULT NULL,
  `IDS` INT NULL DEFAULT NULL,
  `IDL` INT NULL DEFAULT NULL,
  `Data` DATETIME NULL,
  `Plata` INT NULL DEFAULT 0,
  `TIP` INT NULL,                                      -- 2 = receipt, 1 = other document (M01: fiscal receipts are gone)
  `Anulata` BOOLEAN NULL DEFAULT 0,
  `Valid` BOOLEAN NULL DEFAULT 0,
  `Motivul` VARCHAR(255) NULL,
  `OriginMonth` INT NULL,
  `OriginYear` INT NULL,
  `Version` BIGINT NOT NULL DEFAULT 1,
  INDEX `ix_IDP` (`IDP`),
  INDEX `ix_IDL` (`IDL`),
  INDEX `ix_IDZ` (`IDZ`),
  INDEX `ix_IDS` (`IDS`),
  CONSTRAINT `fk_Plati_IDP` FOREIGN KEY (`IDP`) REFERENCES `AD_Platitori` (`IDP`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_Plati_IDZ` FOREIGN KEY (`IDZ`) REFERENCES `AD_Prezenta` (`IDZ`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_Plati_IDS` FOREIGN KEY (`IDS`) REFERENCES `AD_Platitori_sub` (`IDS`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_Plati_IDL` FOREIGN KEY (`IDL`) REFERENCES `AD_LunaD` (`IDL`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE IF NOT EXISTS `AD_Chitante` (
  `IDC` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `IDPL` INT NULL DEFAULT NULL,
  `Data` DATETIME NULL,
  `Serie` VARCHAR(50) NULL,
  `Numar` INT NULL DEFAULT 0,
  `Explicatie` VARCHAR(255) NULL,
  `Anulata` BOOLEAN NULL,
  `IDL` INT NULL DEFAULT NULL,
  `Version` BIGINT NOT NULL DEFAULT 1,
  INDEX `ix_IDL` (`IDL`),
  INDEX `ix_IDPL` (`IDPL`),
  CONSTRAINT `fk_Chitante_IDPL` FOREIGN KEY (`IDPL`) REFERENCES `AD_Plati` (`IDPL`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_Chitante_IDL` FOREIGN KEY (`IDL`) REFERENCES `AD_LunaD` (`IDL`) ON DELETE SET NULL ON UPDATE CASCADE,
  UNIQUE KEY `uq_Serie_Numar` (`Serie`,`Numar`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE IF NOT EXISTS `AD_AlteDoc` (
  `IDA` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `IDPL` INT NULL DEFAULT NULL,
  `NrDoc` VARCHAR(255) NULL,
  `FelDoc` VARCHAR(255) NULL,
  `DataDoc` DATETIME NULL,
  `Anulata` BOOLEAN NULL DEFAULT 0,
  `IDL` INT NULL DEFAULT NULL,
  `Explicatie` VARCHAR(255) NULL,
  `Version` BIGINT NOT NULL DEFAULT 1,
  INDEX `ix_IDL` (`IDL`),
  INDEX `ix_IDPL` (`IDPL`),
  CONSTRAINT `fk_AlteDoc_IDPL` FOREIGN KEY (`IDPL`) REFERENCES `AD_Plati` (`IDPL`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_AlteDoc_IDL` FOREIGN KEY (`IDL`) REFERENCES `AD_LunaD` (`IDL`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE IF NOT EXISTS `AD_Retur` (
  `IDR` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `IDP` INT NULL DEFAULT NULL,
  `IDZ` INT NULL DEFAULT NULL,
  `IDL` INT NULL DEFAULT NULL,
  `IDS` INT NULL DEFAULT NULL,
  `Data` DATETIME NULL,
  `Explicatie` VARCHAR(255) NULL,
  `Anulat` BOOLEAN NULL DEFAULT 0,
  `MotivAnulare` VARCHAR(255) NULL,                    -- mandatory when a refund is cancelled (M03)
  `NrDoc` VARCHAR(255) NULL,
  `Suma` DOUBLE NULL,
  `OriginMonth` INT NULL,
  `OriginYear` INT NULL,
  `Version` BIGINT NOT NULL DEFAULT 1,
  INDEX `ix_IDP` (`IDP`),
  INDEX `ix_IDL` (`IDL`),
  INDEX `ix_IDZ` (`IDZ`),
  INDEX `ix_IDS` (`IDS`),
  CONSTRAINT `fk_Retur_IDP` FOREIGN KEY (`IDP`) REFERENCES `AD_Platitori` (`IDP`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_Retur_IDZ` FOREIGN KEY (`IDZ`) REFERENCES `AD_Prezenta` (`IDZ`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_Retur_IDS` FOREIGN KEY (`IDS`) REFERENCES `AD_Platitori_sub` (`IDS`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_Retur_IDL` FOREIGN KEY (`IDL`) REFERENCES `AD_LunaD` (`IDL`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- Council decision: a departed child with debit is matched with a departed child with credit.
-- IDP_Debit <> IDP_Credit is enforced by the service: MariaDB refuses a CHECK on columns that have a
-- foreign key with ON UPDATE CASCADE (error 1901).
CREATE TABLE IF NOT EXISTS `AD_Compensare` (
  `IDCMP` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `Data` DATETIME NOT NULL,
  `IDP_Debit` INT NOT NULL,
  `IDP_Credit` INT NOT NULL,
  `Suma` DOUBLE NOT NULL,
  `Nota` VARCHAR(255) NULL,
  `Utilizator` VARCHAR(80) NULL,
  `Version` BIGINT NOT NULL DEFAULT 1,
  INDEX `ix_IDP_Debit` (`IDP_Debit`),
  INDEX `ix_IDP_Credit` (`IDP_Credit`),
  CONSTRAINT `fk_Compensare_Debit` FOREIGN KEY (`IDP_Debit`) REFERENCES `AD_Platitori` (`IDP`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_Compensare_Credit` FOREIGN KEY (`IDP_Credit`) REFERENCES `AD_Platitori` (`IDP`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- ---------------------------------------------------------------- technical

CREATE TABLE IF NOT EXISTS AD_Lock (ID INT PRIMARY KEY) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

INSERT IGNORE INTO AD_Lock (ID) VALUES (1);


CREATE TABLE IF NOT EXISTS AD_Operations (RequestKey VARCHAR(100) PRIMARY KEY, Fingerprint CHAR(64) NOT NULL, Result LONGTEXT NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE IF NOT EXISTS AD_Settings (SettingKey VARCHAR(64) PRIMARY KEY, SettingValue VARCHAR(255) NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

INSERT IGNORE INTO AD_Settings (SettingKey, SettingValue) VALUES ('BlockReopenWithMovements', '1');

CREATE TABLE IF NOT EXISTS AD_Imports (SourceHash CHAR(64) PRIMARY KEY, SourceFile VARCHAR(255) NOT NULL, Manifest LONGTEXT NOT NULL, Result LONGTEXT NOT NULL, ImportedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- ---------------------------------------------------------------- only if AD_01 was already run

-- ALTER TABLE `AD_Grupe` ADD COLUMN IF NOT EXISTS `Tip` VARCHAR(10) NOT NULL DEFAULT 'NORMALA';
-- ALTER TABLE `AD_Grupe` ADD COLUMN IF NOT EXISTS `InchisaDinAn` INT NULL DEFAULT NULL;
-- ALTER TABLE `AD_Platitori_sub` ADD COLUMN IF NOT EXISTS `Telefon` VARCHAR(50) NULL AFTER `EMail`;
-- ALTER TABLE `AD_Retur` ADD COLUMN IF NOT EXISTS `MotivAnulare` VARCHAR(255) NULL AFTER `Anulat`;
-- After the educator history is migrated into AD_Grupe_Educator and checked by the operator:
-- ALTER TABLE `AD_Grupe` DROP COLUMN `Educator`;
