-- ADECHIT, SLICE-AD13-01: execute MANUALLY in EACH unit database that uses ADECHIT, after AD_11_extrase_asocieri.sql.
-- The application never executes this script or creates these tables automatically.
-- Requires the K-BOT statement tables (FX_Extrase_F / FX_Extrase_H / FX_Extrase). No changes in AVACONT_COMUN.
--
-- Server side, once: the mailbox password is stored ENCRYPTED. The service needs `cryptography` and the key
-- ADE_MAIL_KEY (Fernet key, 32 random bytes url-safe base64) in its environment file, next to EF_TOKEN_KEY.
-- When ADE_MAIL_KEY is absent EF_TOKEN_KEY is used. config.py is not touched.

-- The mailbox of the unit (one row, ID = 1). The password is a Fernet token, never plain text.
CREATE TABLE IF NOT EXISTS `AD_EXTRASE_EMAIL` (
  `ID` TINYINT NOT NULL,
  `HOST` VARCHAR(253) NOT NULL,
  `UTILIZATOR` VARCHAR(255) NOT NULL,
  `PAROLA` VARCHAR(1024) NOT NULL,
  `DOSAR` VARCHAR(255) NOT NULL DEFAULT 'INBOX',
  `EXPEDITOR` VARCHAR(255) NULL,
  `NUME_ATASAMENT` VARCHAR(100) NOT NULL DEFAULT 'ExtrasEP',
  `DE_LA_DATA` DATE NOT NULL,
  `UIDVALIDITY` BIGINT NULL,
  `ULTIMUL_UID` BIGINT NULL,
  `ULTIMA_VERIFICARE` DATETIME NULL,
  `ULTIMUL_REZULTAT` VARCHAR(500) NULL,
  `MODIFICAT_DE` VARCHAR(255) NOT NULL,
  `MODIFICAT_LA` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

-- Every statement file that came in, by hand or from the mailbox, with what happened to it.
-- SHA256 is the hash of the PDF: the same file is never read twice. IDEXF = FX_Extrase_F.IDEXF when imported.
CREATE TABLE IF NOT EXISTS `AD_EXTRASE_INCARCARI` (
  `ID` INT NOT NULL AUTO_INCREMENT,
  `SURSA` VARCHAR(10) NOT NULL,
  `FISIER` VARCHAR(255) NOT NULL,
  `SHA256` CHAR(64) NOT NULL,
  `STARE` VARCHAR(20) NOT NULL,
  `MESAJ` VARCHAR(500) NULL,
  `RANDURI` INT NOT NULL DEFAULT 0,
  `IDEXF` INT NULL,
  `EMAIL_UID` BIGINT NULL,
  `EMAIL_DE` VARCHAR(255) NULL,
  `EMAIL_SUBIECT` VARCHAR(255) NULL,
  `EMAIL_DATA` DATETIME NULL,
  `INCARCAT_DE` VARCHAR(255) NOT NULL,
  `INCARCAT_LA` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ID`),
  KEY `IX_AD_EXTRASE_INCARCARI_SHA` (`SHA256`),
  KEY `IX_AD_EXTRASE_INCARCARI_LA` (`INCARCAT_LA`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
