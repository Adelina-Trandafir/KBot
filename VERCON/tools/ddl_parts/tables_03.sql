CREATE TABLE IF NOT EXISTS `VC_Imports` (
  `ImportId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `SubunitId` INT UNSIGNED NOT NULL,
  `ConfirmedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ConfirmedBy` VARCHAR(255) NOT NULL,
  `ToolVersion` VARCHAR(40) NOT NULL,
  `PlanHash` CHAR(64) NOT NULL COMMENT 'hash of the association plan the operator confirmed',
  `Summary` MEDIUMTEXT NULL COMMENT 'counts per table and conversion, JSON',
  PRIMARY KEY (`ImportId`),
  UNIQUE KEY `uq_VC_Import_Subunit` (`SubunitId`),
  CONSTRAINT `fk_VC_Import_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `VC_Subunitati` (`SubunitId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE IF NOT EXISTS `VC_ImportSources` (
  `SourceId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `ImportId` INT UNSIGNED NOT NULL,
  `SubunitId` INT UNSIGNED NOT NULL,
  `GestiuneId` INT UNSIGNED NOT NULL,
  `SourceFile` VARCHAR(255) NOT NULL COMMENT 'file name only, never a full path',
  `SourceHash` CHAR(64) NOT NULL,
  `SourceYear` SMALLINT UNSIGNED NULL,
  `SizeBytes` BIGINT UNSIGNED NOT NULL,
  `ReadAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`SourceId`),
  UNIQUE KEY `uq_VC_ImportSource_hash` (`ImportId`, `SourceHash`),
  KEY `ix_VC_ImportSource_gestiune` (`SubunitId`, `GestiuneId`),
  CONSTRAINT `fk_VC_ImportSource_Import` FOREIGN KEY (`ImportId`) REFERENCES `VC_Imports` (`ImportId`) ON DELETE RESTRICT,
  CONSTRAINT `fk_VC_ImportSource_Gestiune` FOREIGN KEY (`SubunitId`, `GestiuneId`) REFERENCES `VC_Gestiuni` (`SubunitId`, `GestiuneId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE IF NOT EXISTS `VC_IdMap` (
  `SourceId` INT UNSIGNED NOT NULL,
  `TableName` VARCHAR(40) NOT NULL,
  `Kind` VARCHAR(10) NOT NULL DEFAULT 'ROW' COMMENT 'ROW (line), GROUP (OI document group) or ASSET (permanent identity across years)',
  `SourceKey` VARCHAR(100) NOT NULL COMMENT 'signed source key, canonical text; composite keys serialised in column order',
  `TargetId` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`SourceId`, `TableName`, `Kind`, `SourceKey`),
  KEY `ix_VC_IdMap_target` (`TableName`, `TargetId`),
  CONSTRAINT `fk_VC_IdMap_Source` FOREIGN KEY (`SourceId`) REFERENCES `VC_ImportSources` (`SourceId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE IF NOT EXISTS `VC_MigrationIssues` (
  `IssueId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `ImportId` INT UNSIGNED NOT NULL,
  `SourceId` INT UNSIGNED NULL,
  `TableName` VARCHAR(40) NOT NULL,
  `SourceKey` VARCHAR(100) NULL,
  `FieldName` VARCHAR(40) NULL,
  `RuleCode` VARCHAR(20) NOT NULL COMMENT 'N01, N02, ... or a conversion code',
  `Severity` VARCHAR(12) NOT NULL COMMENT 'BLOCAJ, AVERTIZARE or CONVERSIE',
  `OriginalValue` VARCHAR(255) NULL,
  `TargetValue` VARCHAR(255) NULL,
  `Justification` VARCHAR(500) NULL,
  `Decision` VARCHAR(20) NULL,
  `DecidedBy` VARCHAR(255) NULL,
  `DecidedAt` DATETIME NULL,
  PRIMARY KEY (`IssueId`),
  KEY `ix_VC_Issue_import` (`ImportId`, `Severity`),
  CONSTRAINT `fk_VC_Issue_Import` FOREIGN KEY (`ImportId`) REFERENCES `VC_Imports` (`ImportId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- Idempotency of web commands: a RequestKey is applied once and its answer is replayed.
CREATE TABLE IF NOT EXISTS `VC_Operations` (
  `SubunitId` INT UNSIGNED NOT NULL,
  `GestiuneId` INT UNSIGNED NOT NULL,
  `RequestKey` VARCHAR(64) NOT NULL,
  `Fingerprint` CHAR(64) NOT NULL COMMENT 'hash of the request body: the same key with another body is refused',
  `Result` MEDIUMTEXT NULL,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`SubunitId`, `GestiuneId`, `RequestKey`),
  CONSTRAINT `fk_VC_Operation_Gestiune` FOREIGN KEY (`SubunitId`, `GestiuneId`) REFERENCES `VC_Gestiuni` (`SubunitId`, `GestiuneId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE IF NOT EXISTS `VC_Audit` (
  `AuditId` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `SubunitId` INT UNSIGNED NOT NULL,
  `GestiuneId` INT UNSIGNED NOT NULL,
  `ExercitiuId` INT UNSIGNED NULL,
  `UserEmail` VARCHAR(255) NOT NULL,
  `Operation` VARCHAR(40) NOT NULL,
  `TableName` VARCHAR(40) NOT NULL,
  `RowKey` VARCHAR(100) NULL,
  `BeforeValue` MEDIUMTEXT NULL,
  `AfterValue` MEDIUMTEXT NULL,
  `RequestKey` VARCHAR(64) NULL,
  `RequestId` VARCHAR(64) NULL COMMENT 'the X-Request-Id that ties page, log and this row',
  `At` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`AuditId`),
  KEY `ix_VC_Audit_ctx` (`SubunitId`, `GestiuneId`, `At`),
  CONSTRAINT `fk_VC_Audit_Gestiune` FOREIGN KEY (`SubunitId`, `GestiuneId`) REFERENCES `VC_Gestiuni` (`SubunitId`, `GestiuneId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- Annual closing: Materiale and Alimente only (the trigger on VC_Exercitii refuses OI/MF).
CREATE TABLE IF NOT EXISTS `VC_AnnualClosings` (
  `ClosingId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `SubunitId` INT UNSIGNED NOT NULL,
  `GestiuneId` INT UNSIGNED NOT NULL,
  `FromExercitiuId` INT UNSIGNED NOT NULL,
  `ToExercitiuId` INT UNSIGNED NOT NULL,
  `ClosedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ClosedBy` VARCHAR(255) NOT NULL,
  `RequestKey` VARCHAR(64) NOT NULL,
  `BalanceHash` CHAR(64) NOT NULL COMMENT 'hash of the carried-over balances: reconciliation after the close',
  PRIMARY KEY (`ClosingId`),
  UNIQUE KEY `uq_VC_Closing_from` (`SubunitId`, `GestiuneId`, `FromExercitiuId`),
  CONSTRAINT `fk_VC_Closing_from` FOREIGN KEY (`SubunitId`, `GestiuneId`, `FromExercitiuId`) REFERENCES `VC_Exercitii` (`SubunitId`, `GestiuneId`, `ExercitiuId`) ON DELETE RESTRICT,
  CONSTRAINT `fk_VC_Closing_to` FOREIGN KEY (`SubunitId`, `GestiuneId`, `ToExercitiuId`) REFERENCES `VC_Exercitii` (`SubunitId`, `GestiuneId`, `ExercitiuId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- Setari/NR of the Access base, typed, per gestiune. Not copied: the type flags (MF, OBINV: they became VC_Gestiuni.Type)
-- and NR.ZEC (became VC_Gestiuni.Zec). IntValue is scaled by VC_Gestiuni.Scara only for keys that hold a value (SI_LA).
CREATE TABLE IF NOT EXISTS `VC_Settings` (
  `SubunitId` INT UNSIGNED NOT NULL,
  `GestiuneId` INT UNSIGNED NOT NULL,
  `SettingKey` VARCHAR(40) NOT NULL,
  `IntValue` INT NOT NULL,
  `Version` BIGINT NOT NULL DEFAULT 1,
  PRIMARY KEY (`SubunitId`, `GestiuneId`, `SettingKey`),
  CONSTRAINT `fk_VC_Setting_Gestiune` FOREIGN KEY (`SubunitId`, `GestiuneId`) REFERENCES `VC_Gestiuni` (`SubunitId`, `GestiuneId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- Counters: one per gestiune + exercise + document kind (NIR, FACTURA, BC, ...). The service increments with a
-- locking read in the same transaction as the document; a manual number is accepted only if free.
-- No UNIQUE on the document tables' number columns: imported history may repeat numbers (rule decided at VC06).
CREATE TABLE IF NOT EXISTS `VC_Numbering` (
  `SubunitId` INT UNSIGNED NOT NULL,
  `GestiuneId` INT UNSIGNED NOT NULL,
  `ExercitiuId` INT UNSIGNED NOT NULL,
  `Kind` VARCHAR(20) NOT NULL,
  `LastNumber` INT NOT NULL DEFAULT 0,
  `Version` BIGINT NOT NULL DEFAULT 1,
  PRIMARY KEY (`SubunitId`, `GestiuneId`, `ExercitiuId`, `Kind`),
  CONSTRAINT `fk_VC_Numbering_Exercitiu` FOREIGN KEY (`SubunitId`, `GestiuneId`, `ExercitiuId`) REFERENCES `VC_Exercitii` (`SubunitId`, `GestiuneId`, `ExercitiuId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
