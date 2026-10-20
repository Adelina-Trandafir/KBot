-- Se executa MANUAL numai in AVACONT_SURSA, dupa AD_05.
-- Propagarea structurii catre bazele unitatilor se face de utilizator prin KBotAdmin.
-- ADE.Migrator nu executa acest DDL.
USE `AVACONT_SURSA`;

CREATE TABLE IF NOT EXISTS `AD_DateEronate` (
  `IDE` BIGINT NOT NULL AUTO_INCREMENT,
  `SubunitId` INT NOT NULL,
  `TabelSursa` VARCHAR(40) NOT NULL,
  `CheieSursa` INT NULL,
  `AniDetectati` VARCHAR(255) NULL,
  `Motiv` LONGTEXT NOT NULL,
  `DateSursa` LONGTEXT NOT NULL COMMENT 'JSON: toate coloanele randului original Access; ID-urile nu sunt remapate',
  `SourceHash` CHAR(64) NOT NULL,
  `SourceFile` VARCHAR(255) NOT NULL,
  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`IDE`),
  KEY `IX_AD_DateEronate_Subunit` (`SubunitId`, `TabelSursa`),
  KEY `IX_AD_DateEronate_Hash` (`SourceHash`),
  CONSTRAINT `FK_AD_DateEronate_Subunit` FOREIGN KEY (`SubunitId`)
    REFERENCES `AD_Subunits` (`SubunitId`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
