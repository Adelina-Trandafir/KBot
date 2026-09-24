-- =====================================================================================
-- Slice 0079 -- the signature log (FX_PDF_SEMNATURI): one row per signature ADDED to a
-- DDF / ORD PDF through K-BOT -- who signed (per the certificate), when (per the signature),
-- which account uploaded it, from which address and computer.
--
-- APPLY ON EVERY UNIT DATABASE (and on AVACONT_SURSA, the template for new units), like 0078.
--
-- Shapes checked against MariaDB_Schema/000_DEMO.sql (dump of 22.09.2026):
-- FX_DDF_REV.IDREV and FX_ORD.IDORDP are int(11) NOT NULL AUTO_INCREMENT primary keys.
--
-- ORDER IS FREE: routes/forexe/pdf.py probes for this table on every upload; until this file
-- has run on a database, uploads work exactly as in 0078 and the records are skipped (logged).
--
-- WHERE EACH COLUMN COMES FROM:
--   from the PDF (read by K-BOT): Camp, Rol, Semnatar, DataSemnaturii
--   from the server:              DataInregistrarii (NOW()), Sha256Pdf, UN (bearer session),
--                                 IpPublic (request address, behind ProxyFix)
--   from the client computer:     IpLocal, NumeCalculator, UtilizatorWindows, SistemOperare,
--                                 VersiuneKbot
-- Only signatures that are NEW in the uploaded file are sent (the ones already there when the
-- file was opened belong to somebody else), and the route skips a row it already has (same
-- document, field and signing time), so a repeated upload never doubles a signature.
-- DataSemnaturii / DataInregistrarii are in the server's local time, like every NOW() column.
-- =====================================================================================

CREATE TABLE IF NOT EXISTS `FX_PDF_SEMNATURI` (
  `IdSemnatura`       int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  -- Exactly one of the two documents (CK_FX_PDF_SEMNATURI_DOC); both cascade with it.
  `IDREV`             int(11)      NULL DEFAULT NULL,
  `IDORDP`            int(11)      NULL DEFAULT NULL,
  `Camp`              varchar(255) NOT NULL,
  `Rol`               varchar(16)  NULL DEFAULT NULL,
  `Semnatar`          varchar(255) NULL DEFAULT NULL,
  `DataSemnaturii`    datetime     NULL DEFAULT NULL,
  `DataInregistrarii` datetime     NOT NULL,
  `Sha256Pdf`         char(64)     NOT NULL,
  `UN`                varchar(128) NOT NULL,
  `IpPublic`          varchar(45)  NULL DEFAULT NULL,
  `IpLocal`           varchar(255) NULL DEFAULT NULL,
  `NumeCalculator`    varchar(128) NULL DEFAULT NULL,
  `UtilizatorWindows` varchar(128) NULL DEFAULT NULL,
  `SistemOperare`     varchar(128) NULL DEFAULT NULL,
  `VersiuneKbot`      varchar(32)  NULL DEFAULT NULL,
  PRIMARY KEY (`IdSemnatura`) USING BTREE,
  INDEX `IX_FX_PDF_SEMNATURI_IDREV` (`IDREV`, `Camp`) USING BTREE,
  INDEX `IX_FX_PDF_SEMNATURI_IDORDP` (`IDORDP`, `Camp`) USING BTREE,
  INDEX `IX_FX_PDF_SEMNATURI_UN` (`UN`, `DataInregistrarii`) USING BTREE,
  CONSTRAINT `FK_FX_PDF_SEMNATURI_REV` FOREIGN KEY (`IDREV`)
    REFERENCES `FX_DDF_REV` (`IDREV`) ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT `FK_FX_PDF_SEMNATURI_ORD` FOREIGN KEY (`IDORDP`)
    REFERENCES `FX_ORD` (`IDORDP`) ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT `CK_FX_PDF_SEMNATURI_DOC` CHECK ((`IDREV` IS NULL) <> (`IDORDP` IS NULL))
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci ROW_FORMAT = Dynamic;
