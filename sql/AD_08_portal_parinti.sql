-- SLICE-AD11-01. Run per unit after AD_05, AD_06, AD_07 with ADE writes suspended.
-- Existing email/CNP conflicts stop conversion; correct them before rerunning.
ALTER TABLE AD_Platitori_sub ADD COLUMN IF NOT EXISTS CodAccesPortal VARCHAR(64) NULL;
CREATE TABLE IF NOT EXISTS AD_PortalParents (
  CNP VARCHAR(13) CHARACTER SET ascii COLLATE ascii_bin NOT NULL PRIMARY KEY,
  Email VARCHAR(255) NULL,
  CodAccesPortal VARCHAR(64) CHARACTER SET ascii COLLATE ascii_bin NULL,
  UNIQUE KEY uq_Portal_Email (Email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
CREATE TABLE IF NOT EXISTS AD_PortalChallenges (
  ChallengeId VARCHAR(64) CHARACTER SET ascii COLLATE ascii_bin NOT NULL PRIMARY KEY,
  CNP VARCHAR(13) NOT NULL,
  Email VARCHAR(255) NOT NULL,
  AccessHash CHAR(64) NOT NULL,
  CodeHash CHAR(64) NOT NULL,
  ExpiresAt BIGINT NOT NULL,
  Attempts INT NOT NULL DEFAULT 0,
  CreatedAt BIGINT NOT NULL,
  KEY ix_Challenge_CNP (CNP, CreatedAt)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
INSERT IGNORE INTO AD_Lock (ID) VALUES (0);
DELIMITER $$
DROP PROCEDURE IF EXISTS ad08_parents$$
CREATE PROCEDURE ad08_parents()
BEGIN
  IF EXISTS (SELECT 1 FROM AD_Platitori_sub WHERE TRIM(COALESCE(EMail,''))<>''
    GROUP BY LOWER(TRIM(EMail)) HAVING COUNT(DISTINCT COALESCE(NULLIF(TRIM(CNP_Platitor),''),'MISSING'))>1)
  THEN SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='Email folosit de CNP-uri diferite. Corectati parintii inainte de AD_08.'; END IF;
  IF EXISTS (SELECT 1 FROM AD_Platitori_sub WHERE CHAR_LENGTH(TRIM(CNP_Platitor))=13 AND TRIM(COALESCE(EMail,''))<>''
    GROUP BY TRIM(CNP_Platitor) HAVING COUNT(DISTINCT LOWER(TRIM(EMail)))>1)
  THEN SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT='Acelasi CNP are emailuri diferite. Corectati parintii inainte de AD_08.'; END IF;
  INSERT INTO AD_PortalParents (CNP,Email)
    SELECT TRIM(CNP_Platitor),MAX(NULLIF(LOWER(TRIM(EMail)),'')) FROM AD_Platitori_sub
    WHERE TRIM(CNP_Platitor) REGEXP '^[0-9]{13}$' GROUP BY TRIM(CNP_Platitor)
    ON DUPLICATE KEY UPDATE Email=VALUES(Email);
  UPDATE AD_PortalParents i SET CodAccesPortal=LOWER(HEX(RANDOM_BYTES(16)))
    WHERE i.CodAccesPortal IS NULL AND EXISTS (
      SELECT 1 FROM AD_Platitori_sub p JOIN AD_Platitori c ON c.IDP=p.IDP AND c.SubunitId=p.SubunitId
      WHERE TRIM(p.CNP_Platitor)=i.CNP AND COALESCE(c.Plecat,0)=0);
  UPDATE AD_Platitori_sub p JOIN AD_PortalParents i ON TRIM(p.CNP_Platitor)=i.CNP
    SET p.CodAccesPortal=i.CodAccesPortal;
END$$
CALL ad08_parents()$$
DROP PROCEDURE ad08_parents$$
DELIMITER ;
