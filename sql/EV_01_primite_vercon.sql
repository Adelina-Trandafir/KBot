-- EV01 (slice 00EF-45): received e-invoices -> VERCON. Run by hand ONLY on AVACONT_SURSA, then propagate the structure with the schema
-- sync to the unit databases (same procedure as 00EF_17_primite_asocieri.sql / KE_01_primite_receptii.sql). Plan: docs/EV_PLAN.md.
-- Needs KE_01_primite_receptii.sql (EF_PrimiteLinii.CotaTVA) to have been run: the VAT of a line comes from there.
--
-- The accountant SENDS a received invoice to VERCON; the Administrator TAKES it over in "Adaugare NIR" and the row closes with the
-- NIR (or NIRs) he created. The invoice itself stays where it is (EF_Primite): nothing is copied.
--
-- 1. EF_PrimiteVercon: the sending. At most ONE active row (TRIMISA / PRELUATA) per invoice -- the server checks it inside the
--    transaction (a partial unique index does not exist in MariaDB, and a generated column would complicate the schema sync).
--    Stare: TRIMISA (waiting for the Administrator), PRELUATA (a NIR exists), RETURNATA (the Administrator sent it back, with a reason),
--    RETRASA (the accountant took it back before it was taken). RETURNATA / RETRASA rows stay as history.
--    A PRELUATA row whose NIRs are all cancelled counts as TRIMISA again (decided at READING, see EF_PrimiteVerconNir).
CREATE TABLE IF NOT EXISTS `EF_PrimiteVercon` (
  `IdTrimitere` int(11) NOT NULL AUTO_INCREMENT,
  `IdPrimita` int(11) NOT NULL,
  `Stare` varchar(16) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'TRIMISA' COMMENT 'TRIMISA / PRELUATA / RETURNATA / RETRASA',
  `Observatie` varchar(500) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'note of the accountant for the Administrator',
  `UnTrimis` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'who sent it',
  `DataTrimis` datetime NULL DEFAULT current_timestamp(),
  `MotivReturnare` varchar(500) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `UnReturnat` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DataReturnat` datetime NULL DEFAULT NULL,
  `UnPreluat` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'who took it over',
  `DataPreluat` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`IdTrimitere`) USING BTREE,
  INDEX `IX_EF_PrimiteVercon_IdPrimita`(`IdPrimita` ASC, `Stare` ASC) USING BTREE,
  CONSTRAINT `FK_EF_PrimiteVercon_Primita` FOREIGN KEY (`IdPrimita`) REFERENCES `EF_Primite` (`IdPrimita`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- 2. EF_PrimiteVerconLinii: ALL the lines of the invoice (decision D10: nothing is left out), each with the account the accountant
--    PROPOSES (VC_Masini.IdMasina, from any subunit). NULL = the Administrator chooses the account. A proposal, not an order (D9).
--    No foreign key on IdMasina: an account is deactivated, never a reason to block the nomenclature sync (same decision as
--    EF_PrimiteLinii.IdClsf).
CREATE TABLE IF NOT EXISTS `EF_PrimiteVerconLinii` (
  `IdTrimitere` int(11) NOT NULL,
  `IdLinie` int(11) NOT NULL,
  `IdMasina` int(10) UNSIGNED NULL DEFAULT NULL COMMENT 'VC_Masini.IdMasina proposed by the accountant; NULL = the Administrator chooses',
  PRIMARY KEY (`IdTrimitere`, `IdLinie`) USING BTREE,
  INDEX `IX_EF_PrimiteVerconLinii_IdLinie`(`IdLinie` ASC) USING BTREE,
  CONSTRAINT `FK_EF_PrimiteVerconLinii_Trimitere` FOREIGN KEY (`IdTrimitere`) REFERENCES `EF_PrimiteVercon` (`IdTrimitere`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_EF_PrimiteVerconLinii_Linie` FOREIGN KEY (`IdLinie`) REFERENCES `EF_PrimiteLinii` (`IdLinie`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- 3. EF_PrimiteVerconNir: the NIRs the Administrator created from a sending (0..n; several only when the accounts belong to
--    different gestiuni). Written by VERCON in the same transaction that saves the NIR (slice EV05).
--    NO foreign key to VC_NIR on purpose: the migrator empties and refills the VC_* tables (VC_Golire.sql), and a RESTRICT key from
--    here would block it. The link is read with a join on (SubunitId, GestiuneId, ExercitiuId, IdIntrare); a NIR that no longer
--    exists, or is cancelled (VC_NIR.Anulat = 1), does not count.
CREATE TABLE IF NOT EXISTS `EF_PrimiteVerconNir` (
  `IdTrimitere` int(11) NOT NULL,
  `SubunitId` int(10) UNSIGNED NOT NULL,
  `GestiuneId` int(10) UNSIGNED NOT NULL,
  `ExercitiuId` int(10) UNSIGNED NOT NULL,
  `IdIntrare` int(10) UNSIGNED NOT NULL,
  PRIMARY KEY (`IdTrimitere`, `SubunitId`, `GestiuneId`, `ExercitiuId`, `IdIntrare`) USING BTREE,
  INDEX `IX_EF_PrimiteVerconNir_Nir`(`SubunitId` ASC, `GestiuneId` ASC, `ExercitiuId` ASC, `IdIntrare` ASC) USING BTREE,
  CONSTRAINT `FK_EF_PrimiteVerconNir_Trimitere` FOREIGN KEY (`IdTrimitere`) REFERENCES `EF_PrimiteVercon` (`IdTrimitere`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;
