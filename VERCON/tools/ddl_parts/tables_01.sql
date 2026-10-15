CREATE TABLE IF NOT EXISTS `VC_Subunitati` (
  `SubunitId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(100) NOT NULL,
  `Active` TINYINT(1) NOT NULL DEFAULT 1,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Version` BIGINT NOT NULL DEFAULT 1,
  PRIMARY KEY (`SubunitId`),
  UNIQUE KEY `uq_VC_Subunit_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE IF NOT EXISTS `VC_Gestiuni` (
  `GestiuneId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `SubunitId` INT UNSIGNED NOT NULL,
  `Name` VARCHAR(100) NOT NULL,
  `Type` VARCHAR(10) NOT NULL COMMENT 'MATERIALE, ALIMENTE, OI or MF',
  `State` VARCHAR(24) NOT NULL DEFAULT 'IN_PREGATIRE' COMMENT 'IN_PREGATIRE, IMPORTATA_IN_REVIZIE, ACTIVA, ARHIVATA',
  `Zec` TINYINT UNSIGNED NOT NULL DEFAULT 4 COMMENT 'decimals shown and used in calculations (NR.ZEC of the gestiune)',
  `Scara` TINYINT UNSIGNED NOT NULL DEFAULT 4 COMMENT 'fixed storage precision 4; decimal values are not scaled',
  `Populated` TINYINT(1) NOT NULL DEFAULT 0 COMMENT 'set by the service at the first data; freezes Type and Scara',
  `Active` TINYINT(1) NOT NULL DEFAULT 1,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Version` BIGINT NOT NULL DEFAULT 1,
  PRIMARY KEY (`GestiuneId`),
  UNIQUE KEY `uq_VC_Gestiune_ctx` (`SubunitId`, `GestiuneId`),
  UNIQUE KEY `uq_VC_Gestiune_Name` (`SubunitId`, `Name`),
  CONSTRAINT `fk_VC_Gestiune_Subunit` FOREIGN KEY (`SubunitId`) REFERENCES `VC_Subunitati` (`SubunitId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE IF NOT EXISTS `VC_Exercitii` (
  `ExercitiuId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `SubunitId` INT UNSIGNED NOT NULL,
  `GestiuneId` INT UNSIGNED NOT NULL,
  `An` SMALLINT UNSIGNED NOT NULL,
  `Closed` TINYINT(1) NOT NULL DEFAULT 0,
  `ClosedAt` DATETIME NULL,
  `ClosedBy` VARCHAR(255) NULL,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Version` BIGINT NOT NULL DEFAULT 1,
  PRIMARY KEY (`ExercitiuId`),
  UNIQUE KEY `uq_VC_Exercitiu_ctx` (`SubunitId`, `GestiuneId`, `ExercitiuId`),
  UNIQUE KEY `uq_VC_Exercitiu_An` (`GestiuneId`, `An`),
  CONSTRAINT `fk_VC_Exercitiu_Gestiune` FOREIGN KEY (`SubunitId`, `GestiuneId`) REFERENCES `VC_Gestiuni` (`SubunitId`, `GestiuneId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- Exclusive access: the primary key IS the rule. Holder + session token + expiry; renewal and takeover are done by the
-- service with conditional UPDATEs (takeover only WHERE ExpiresAt < NOW()).
CREATE TABLE IF NOT EXISTS `VC_ContextLock` (
  `SubunitId` INT UNSIGNED NOT NULL,
  `GestiuneId` INT UNSIGNED NOT NULL,
  `HolderEmail` VARCHAR(255) NOT NULL,
  `SessionToken` CHAR(64) NOT NULL,
  `AcquiredAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `RenewedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ExpiresAt` DATETIME NOT NULL,
  PRIMARY KEY (`SubunitId`, `GestiuneId`),
  CONSTRAINT `fk_VC_Lock_Gestiune` FOREIGN KEY (`SubunitId`, `GestiuneId`) REFERENCES `VC_Gestiuni` (`SubunitId`, `GestiuneId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

