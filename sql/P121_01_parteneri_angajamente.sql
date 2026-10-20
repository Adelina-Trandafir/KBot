-- 0121-01: supplier <-> angajament links, keyed by the normalized CodFiscal (NOT by the per-unit IdPartener / CodPartener).
-- Plan: docs/worklog/SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md.
-- Run by hand ONLY on AVACONT_SURSA, then propagate the structure with the schema sync (KBotAdmin) to the unit databases
-- (same procedure as KE_01_primite_receptii.sql). Safe to run twice (IF NOT EXISTS). Nothing is dropped, nothing is altered:
-- `Parteneri` and `Parteneri_Coduri` stay exactly as they are.
--
-- Why by CodFiscal: one DC database holds several units, and `Parteneri` is per unit (unique IdUnitate + CodPartener). A table keyed
-- by the CUI is DC-wide by construction, so a supplier does not have to exist in every unit an angajament touches.
--
-- CodFiscal is written NORMALIZED (the DDF rule, PYTHON primite_ubl.normalize_cui) and read normalized; the table never holds the raw
-- text of Parteneri.CodFiscal. A partner without a CodFiscal has no rows here.
--
-- No foreign key on CodAng / CodAI: the pick can be made before the angajament is synchronised, an angajament deleted on the site must
-- not silently take the supplier's links with it, and the Migrator report has to be able to SHOW a CodAI missing in FX_Indicatori.
-- The only foreign key is indicator -> its link (cascade).

-- 1. The link supplier <-> angajament. Several angajamente of one supplier on the same classification are allowed: the key is the pair
--    (CodFiscal, CodAng), not (partner, classification) as in Parteneri_Coduri.
CREATE TABLE IF NOT EXISTS `Parteneri_Angajamente` (
  `IdLegAng` int(11) NOT NULL AUTO_INCREMENT,
  `CodFiscal` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT '0121-01: normalized CUI of the supplier (primite_ubl.normalize_cui)',
  `CodAng` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT '0121-01: FX_Angajamente.CodAngajament, no foreign key',
  `DataAdugare` datetime NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`IdLegAng`) USING BTREE,
  UNIQUE INDEX `UQ_Parteneri_Angajamente_CodFiscal_CodAng`(`CodFiscal` ASC, `CodAng` ASC) USING BTREE,
  INDEX `IX_Parteneri_Angajamente_CodAng`(`CodAng` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- 2. The indicators of such a link. CodAI (CodAng-CodInd) is the key of FX_Indicatori and already gives IdClsf, so no per-unit IdClsf is
--    kept here. ContBancar is the supplier's bank account for that indicator, as in Parteneri_Coduri today; NULL = none.
CREATE TABLE IF NOT EXISTS `Parteneri_Angajamente_Indicatori` (
  `IdLegInd` int(11) NOT NULL AUTO_INCREMENT,
  `IdLegAng` int(11) NOT NULL,
  `CodAI` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT '0121-01: FX_Indicatori.CodAI (CodAng-CodInd), no foreign key',
  `CodInd` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `ContBancar` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DataAdugare` datetime NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`IdLegInd`) USING BTREE,
  UNIQUE INDEX `UQ_Parteneri_Angajamente_Indicatori_Leg_CodAI`(`IdLegAng` ASC, `CodAI` ASC) USING BTREE,
  INDEX `IX_Parteneri_Angajamente_Indicatori_CodAI`(`CodAI` ASC) USING BTREE,
  CONSTRAINT `Parteneri_Angajamente_Indicatori_ibfk_1` FOREIGN KEY (`IdLegAng`) REFERENCES `Parteneri_Angajamente` (`IdLegAng`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;
