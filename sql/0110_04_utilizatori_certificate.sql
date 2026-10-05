-- =====================================================================================
-- Slice 0110-04 -- portal: sign-in with a qualified digital certificate (nginx mTLS).
-- One row per certificate a user has enrolled (while signed in with password + code).
-- The certificate is recognised by the SHA-256 of its DER bytes (Amprenta), never by a field
-- of the person (no CNP). The operator revokes by setting Activ = 0 (or the user does, from
-- the portal). A renewed certificate is a NEW row: enrolling it switches the older ones off.
-- AVACONT_COMUN, utf8mb3 / utf8mb3_general_ci like every new table (house rule). No query joins
-- this table to Unitati_Utilizatori / Jurnal (those two are utf8mb4 in MariaDB_Schema); UN is
-- only compared with a parameter. Run BEFORE deploying routes/portal/certificat.py.
-- The service account needs SELECT/INSERT/UPDATE on the table (check after creating).
-- Safe to run twice.
-- =====================================================================================
USE `AVACONT_COMUN`;

CREATE TABLE IF NOT EXISTS `Utilizatori_Certificate` (
  `IdCertificat` int(11)      NOT NULL AUTO_INCREMENT,
  `UN`           varchar(80)  CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Amprenta`     char(64)     CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Subiect`      varchar(400) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Emitent`      varchar(400) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `ValidPana`    datetime     NULL DEFAULT NULL,
  `Activ`        tinyint(1)   NOT NULL DEFAULT 1,
  `DataInrolare` datetime     NOT NULL DEFAULT current_timestamp(),
  `DataDezactivare` datetime  NULL DEFAULT NULL,
  `UltimaFolosire` datetime   NULL DEFAULT NULL,
  PRIMARY KEY (`IdCertificat`) USING BTREE,
  UNIQUE INDEX `ux_Utilizatori_Certificate_Amprenta`(`Amprenta` ASC) USING BTREE,
  INDEX `ix_Utilizatori_Certificate_UN`(`UN` ASC, `Activ` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;
