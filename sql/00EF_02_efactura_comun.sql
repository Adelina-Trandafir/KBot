-- =====================================================================================
-- Slice 00EF-02 -- the ANAF e-Factura token, in AVACONT_COMUN. NOT RUN anywhere.
-- Safe to run again. InnoDB, utf8mb3 / utf8mb3_general_ci.
--
-- One row per unit (DC), with the tax code of the unit as it was when the unit authorised.
-- Why here and not in the unit database: the server's refresh job walks ONE table to renew
-- every unit's token; the unit's name and tax code (Unitati.DC, CF) are already here.
--
-- WHAT IS IN IT, AND WHAT NEVER LEAVES THE SERVER
--   * AccessTokenCrypt / RefreshTokenCrypt hold the two tokens ENCRYPTED by the Python server
--     (key in the server's environment, never in this database or in the repo). No route
--     returns them, no log line prints them.
--   * The client secret of the application registered at ANAF is NOT here at all: it lives in
--     the server's configuration.
--   * The PC only ever gets: «valid until», «warn from», the certificate label, the last
--     error text, and who authorised.
--
-- WHEN THE OPERATOR MUST ACT
--   The access token is renewed by the server alone (refresh needs no certificate). The one
--   date the operator has to know is when the REFRESH token stops working: from then on the
--   certificate step has to be done again (about once a year, operator 06.10.2026). That date
--   is RefreshExpiraLa. ANAF's answer to the token request carries `expires_in` (seconds) for
--   the access token; the refresh token's lifetime is NOT in that answer as far as the old
--   EF.EXE shows (it only reads access_token, refresh_token, expires_in), so RefreshExpiraLa
--   is a date the server COMPUTES (authorisation time + the lifetime written in the server
--   configuration) -- an assumption to confirm in 00EF-03, not a fact.
--   The app shows a warning from 7 days before RefreshExpiraLa.
-- =====================================================================================

CREATE TABLE IF NOT EXISTS `EF_Token` (
  `DC` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Unitati.DC',
  `CUI` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Tax code the token was issued for (the cif= of every ANAF call); digits only',
  `AccessTokenCrypt` text CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'ENCRYPTED. Never returned by any route',
  `RefreshTokenCrypt` text CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'ENCRYPTED. Never returned by any route',
  `AccessExpiraLa` datetime NULL DEFAULT NULL COMMENT 'UTC. From expires_in of the last token answer; the server renews before it',
  `RefreshExpiraLa` datetime NULL DEFAULT NULL COMMENT 'UTC. Computed by the server (see header): after it the certificate step is needed again',
  `DataAutorizare` datetime NULL DEFAULT NULL COMMENT 'UTC. Last certificate step',
  `DataReinnoire` datetime NULL DEFAULT NULL COMMENT 'UTC. Last successful refresh',
  `AutorizatDe` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'K-BOT login (e-mail) of who did the certificate step',
  `CertEticheta` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'CN of the certificate, for display (not a secret)',
  `CertAmprenta` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Thumbprint of the certificate, for display (not a secret)',
  `UltimaEroare` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Short text, no token and no secret in it',
  `UltimaEroareLa` datetime NULL DEFAULT NULL,
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`DC`) USING BTREE,
  INDEX `IX_EF_Token_CUI`(`CUI` ASC) USING BTREE,
  INDEX `IX_EF_Token_RefreshExpiraLa`(`RefreshExpiraLa` ASC) USING BTREE,
  CONSTRAINT `FK_EF_Token_Unitati` FOREIGN KEY (`DC`) REFERENCES `Unitati` (`DC`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- A one-time code (`state`) handed out by `token/start` and spent by `token/cod`, so a code
-- that arrives without a start, or twice, is refused. Short-lived: the server deletes rows
-- older than 15 minutes whenever it writes here.
CREATE TABLE IF NOT EXISTS `EF_TokenStart` (
  `State` char(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Random, single use',
  `DC` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `UN` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'K-BOT login that asked for it: the code must come from the same one',
  `DataAdaugare` datetime NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`State`) USING BTREE,
  INDEX `IX_EF_TokenStart_Data`(`DataAdaugare` ASC) USING BTREE,
  CONSTRAINT `FK_EF_TokenStart_Unitati` FOREIGN KEY (`DC`) REFERENCES `Unitati` (`DC`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ---------------------------------------------------------------------------------
-- Units of measure of the invoice lines (Access EF_UM: COD, EXP). COD is the UN/ECE code sent
-- as unitCode in the XML. Same list for every unit, so it is one table here (like BIC). NOT migrated by the
-- Migrator: the operator writes the rows on the server himself (the list is in
-- Surse/RawExport/tables/TABLE_VALUES/EF_UM.txt: 2113 rows, codes of 2-4 characters). The Migrator tab «E-Factura»
-- only READS this table, to warn about a line whose unit of measure is not in it.
-- ---------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `EF_UM` (
  `Cod` varchar(8) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Explicatie` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`Cod`) USING BTREE,
  INDEX `IX_EF_UM_Explicatie`(`Explicatie` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- Check afterwards:  SHOW CREATE TABLE EF_Token;  SHOW CREATE TABLE EF_UM;  SHOW CREATE TABLE EF_TokenStart;
-- The service account needs SELECT, INSERT, UPDATE, DELETE on both tables (check its rights).
