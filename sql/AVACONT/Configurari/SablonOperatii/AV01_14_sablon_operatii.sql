-- =====================================================================================
-- Slice AV01-14 -- RUN THIS ON `AVACONT_SURSA` ONLY (Navicat / mysql, as root), then «Sincronizare schema» on the unit database.
--
-- WHAT THIS CREATES  (the Access `DefaNoteA` + `DefaNote`, button OCI, form `Operatii2020`; named SablonOperatii by the operator, 27.10.2026)
--   TZ_SablonOperatii        the HEADER of an operation template: journal, type, kind, explanation, department, payment text, classification key.
--   TZ_SablonOperatiiLinie   the LINES of a template: debit account + credit account (base accounts only) and who the partner is on each side.
--
-- RULES (operator, 27.10.2026)
--   * Tip = 'M'  an operation WITH a classification and a partner on ONE side (debit or credit), fixed from the start or chosen when the template
--                is used; Cheie (the classification key, e.g. 65.01.XX.XX.57.02.02) is then required.
--     Tip = 'A'  any other operation; it may have no partner and no classification (Cheie empty).
--     The Access types C and M both become 'M'; A stays 'A'.
--   * Fel = 'E' evidence of an expense, 'P' payment, 'A' other operations. FelOP (the text added to the payment order) only makes sense for 'P'.
--   * A line holds the BASE account (AVACONT_COMUN.DefaCont.Simbol); the full account (sector + source + classification) is composed when the
--     template is used (stage 2). The 'XXX' mark of Access becomes PartXMod = 'OPERATIE' (the partner of the operation); a fixed partner is
--     PartXMod = 'FIX' + IdPartX; no partner is 'NIMIC'.
--   * PA / Ordonantare / Chelt need a classification (Tip 'M'); PA only with Fel 'E', Ordonantare only with Fel 'P'. The SERVER checks this on save.
--   * The sector+source of a template is the one of its journal (TZ_Jurnale.SursaSector): it is not repeated here.
--   * A template cannot be deleted once an operation points at it (the foreign key from the operations, stage 2, will be RESTRICT); until then
--     it can be deleted freely. The lines go with their template (ON DELETE CASCADE).
--
-- NOT HERE: the operations that use a template (stage 2); the salary templates (TZ_SablonSalarii*, slice AV01-15).
-- Safe to run twice (IF NOT EXISTS). utf8mb3_general_ci like every other table.
-- =====================================================================================

CREATE TABLE IF NOT EXISTS `AVACONT_SURSA`.`TZ_SablonOperatii` (
  `IdSablon`       int(11) NOT NULL AUTO_INCREMENT,
  `IdJurnal`       int(11) NOT NULL COMMENT 'Journal of the operation (Access DefaNoteA.Jurnal); gives the sector+source of the template',
  `Tip`            char(1) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'M = classification + partner on one side; A = other operations',
  `Fel`            char(1) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'E = evidence, P = payment, A = other operations',
  `Explicatie`     varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Text of the operation',
  `Compartiment`   varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Department (free text); goes onto the operation',
  `FelOP`          varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Text added to the payment order; only for Fel = P',
  `Cheie`          varchar(30) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Classification key with XX.XX for the subchapter, e.g. 65.01.XX.XX.57.02.02; required for Tip M',
  `DataAdaugare`   datetime NOT NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdSablon`) USING BTREE,
  INDEX `IX_TZ_SablonOperatii_Jurnal`(`IdJurnal` ASC) USING BTREE,
  INDEX `IX_TZ_SablonOperatii_Tip`(`Tip` ASC, `Fel` ASC) USING BTREE,
  CONSTRAINT `TZ_SablonOperatii__Jurnal` FOREIGN KEY (`IdJurnal`) REFERENCES `TZ_Jurnale` (`IdJurnal`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `CK_TZ_SablonOperatii_Tip` CHECK (`Tip` in ('M','A')),
  CONSTRAINT `CK_TZ_SablonOperatii_Fel` CHECK (`Fel` in ('E','P','A'))
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

CREATE TABLE IF NOT EXISTS `AVACONT_SURSA`.`TZ_SablonOperatiiLinie` (
  `IdLinie`         int(11) NOT NULL AUTO_INCREMENT,
  `IdSablon`        int(11) NOT NULL,
  `Forma`           varchar(5) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Subchapter of the classification (04.02); empty = valid for every form. Unique per template',
  `ContDebit`       int(11) NOT NULL COMMENT 'Base account, debit = AVACONT_COMUN.DefaCont.Simbol',
  `ContCredit`      int(11) NOT NULL COMMENT 'Base account, credit = AVACONT_COMUN.DefaCont.Simbol',
  `PartDebitMod`    enum('NIMIC','OPERATIE','FIX') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'NIMIC' COMMENT 'Partner on the debit side: none / the one of the operation (Access XXX) / a fixed one',
  `IdPartDebit`     int(11) NULL DEFAULT NULL COMMENT 'The fixed partner; required exactly when PartDebitMod = FIX',
  `PartCreditMod`   enum('NIMIC','OPERATIE','FIX') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'NIMIC',
  `IdPartCredit`    int(11) NULL DEFAULT NULL,
  `PA`              tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Proposal + commitment (evidence only); needs a classification',
  `Ordonantare`     tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Payment order line (payment only); needs a classification',
  `Chelt`           tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Effective expense; needs a classification',
  `DataAdaugare`    datetime NOT NULL DEFAULT current_timestamp(),
  `DataModificare`  datetime NULL DEFAULT NULL ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdLinie`) USING BTREE,
  UNIQUE INDEX `UK_TZ_SablonOperatiiLinie_Forma`(`IdSablon` ASC, `Forma` ASC) USING BTREE,
  INDEX `IX_TZ_SablonOperatiiLinie_Debit`(`ContDebit` ASC) USING BTREE,
  INDEX `IX_TZ_SablonOperatiiLinie_Credit`(`ContCredit` ASC) USING BTREE,
  INDEX `IX_TZ_SablonOperatiiLinie_PartD`(`IdPartDebit` ASC) USING BTREE,
  INDEX `IX_TZ_SablonOperatiiLinie_PartC`(`IdPartCredit` ASC) USING BTREE,
  CONSTRAINT `TZ_SablonOperatiiLinie__Sablon` FOREIGN KEY (`IdSablon`) REFERENCES `TZ_SablonOperatii` (`IdSablon`) ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT `TZ_SablonOperatiiLinie__ContD` FOREIGN KEY (`ContDebit`) REFERENCES `AVACONT_COMUN`.`DefaCont` (`Simbol`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `TZ_SablonOperatiiLinie__ContC` FOREIGN KEY (`ContCredit`) REFERENCES `AVACONT_COMUN`.`DefaCont` (`Simbol`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `TZ_SablonOperatiiLinie__PartD` FOREIGN KEY (`IdPartDebit`) REFERENCES `Parteneri` (`IdPartener`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `TZ_SablonOperatiiLinie__PartC` FOREIGN KEY (`IdPartCredit`) REFERENCES `Parteneri` (`IdPartener`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `CK_TZ_SablonOperatiiLinie_PartD` CHECK ((`PartDebitMod` = 'FIX') = (`IdPartDebit` is not null)),
  CONSTRAINT `CK_TZ_SablonOperatiiLinie_PartC` CHECK ((`PartCreditMod` = 'FIX') = (`IdPartCredit` is not null))
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- Check (read only):
-- SHOW CREATE TABLE `AVACONT_SURSA`.`TZ_SablonOperatii`;
-- SHOW CREATE TABLE `AVACONT_SURSA`.`TZ_SablonOperatiiLinie`;
-- Then: AvacontPush, tab «Sincronizare schema», SAFE, on every unit database.
