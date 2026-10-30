-- =====================================================================================
-- Slice AV01-15 -- RUN THIS ON `AVACONT_SURSA` ONLY (Navicat / mysql, as root), then «Sincronizare schema» on the unit database.
--
-- WHAT THIS CREATES  (the Access `DefaSalarii` + `DefaSalariiVA` + `DefaSalariiPC` + `DefaSalariiPN`, button OCS, form `OCS`; named SablonSalarii by the operator)
--   TZ_SablonSalarii        an ARTICLE of the salary template: one economic classification of the pay roll (10.01.01 ...) of one SUBUNIT, with its ONE
--                           «commitment» operation (debit = personnel expense, credit = debt to the personnel).
--   TZ_SablonSalariiLinie   the lines under an article, in three kinds:
--                             RETINERE  (Access VA)  an amount kept from the salary (taxes, contributions ...); its DEBIT is the CREDIT of the article;
--                             VIRAMENT  (Access PC)  the payment of what was kept, to the third party; it may point at the RETINERE it pays (IdParinte);
--                             NUMERAR   (Access PN)  salaries paid in cash (withdrawal from the treasury + the payment).
--
-- RULES (operator, 23.10.2026 and 27.10.2026)
--   * Articles belong to a SUBUNIT (the subunit decides the chapter: 65.01 = 01A ...). No year column: the template does not depend on the year.
--   * Accounts are BASE accounts (AVACONT_COMUN.DefaCont.Simbol); the full account is composed when the pay roll is applied (stage 3).
--   * Partner of a side: PartXMod = 'NIMIC' (none) / 'OPERATIE' (the partner of the operation) / 'FIX' + IdPartX (a fixed partner), like the operation templates.
--   * An article is NOT saved without at least one VIRAMENT (the SERVER checks it on save, with a clear message).
--   * EsteCM marks the lines of medical leave (account 423); at most one per article and kind (checked by the server).
--   * The debit of a RETINERE is the credit of its article: the server writes it, the window shows it.
--   * Deleting an article that a saved month uses will be blocked by the foreign key of those operations (stage 3, RESTRICT). Until then it is free.
--
-- NOT HERE: the operations a month of pay roll produces (stage 3). The Access columns SRT / Sef / Proiect / GRUPA / ContB and the flag Ascuns are not brought over
-- (hidden rows are not migrated).
-- Safe to run twice (IF NOT EXISTS). utf8mb3_general_ci like every other table.
-- =====================================================================================

CREATE TABLE IF NOT EXISTS `AVACONT_SURSA`.`TZ_SablonSalarii` (
  `IdArticol`      int(11) NOT NULL AUTO_INCREMENT,
  `IdSubunitate`   int(11) NOT NULL COMMENT 'The subunit of the article (it decides the chapter)',
  `Clasificatie`   varchar(8) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Article + paragraph of the pay roll, NN.NN.NN (Access DefaSalarii.Clsf)',
  `Explicatie`     varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Text of the commitment operation',
  `ContDebit`      int(11) NOT NULL COMMENT 'Base account, debit = AVACONT_COMUN.DefaCont.Simbol',
  `ContCredit`     int(11) NOT NULL COMMENT 'Base account, credit',
  `PartDebitMod`   enum('NIMIC','OPERATIE','FIX') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'NIMIC',
  `IdPartDebit`    int(11) NULL DEFAULT NULL COMMENT 'The fixed partner; required exactly when PartDebitMod = FIX',
  `PartCreditMod`  enum('NIMIC','OPERATIE','FIX') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'NIMIC',
  `IdPartCredit`   int(11) NULL DEFAULT NULL,
  `DataAdaugare`   datetime NOT NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdArticol`) USING BTREE,
  INDEX `IX_TZ_SablonSalarii_Subunitate`(`IdSubunitate` ASC, `Clasificatie` ASC) USING BTREE,
  INDEX `IX_TZ_SablonSalarii_ContD`(`ContDebit` ASC) USING BTREE,
  INDEX `IX_TZ_SablonSalarii_ContC`(`ContCredit` ASC) USING BTREE,
  INDEX `IX_TZ_SablonSalarii_PartD`(`IdPartDebit` ASC) USING BTREE,
  INDEX `IX_TZ_SablonSalarii_PartC`(`IdPartCredit` ASC) USING BTREE,
  CONSTRAINT `TZ_SablonSalarii__Subunitate` FOREIGN KEY (`IdSubunitate`) REFERENCES `TZ_Subunitati` (`IdSubunitate`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `TZ_SablonSalarii__ContD` FOREIGN KEY (`ContDebit`) REFERENCES `AVACONT_COMUN`.`DefaCont` (`Simbol`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `TZ_SablonSalarii__ContC` FOREIGN KEY (`ContCredit`) REFERENCES `AVACONT_COMUN`.`DefaCont` (`Simbol`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `TZ_SablonSalarii__PartD` FOREIGN KEY (`IdPartDebit`) REFERENCES `Parteneri` (`IdPartener`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `TZ_SablonSalarii__PartC` FOREIGN KEY (`IdPartCredit`) REFERENCES `Parteneri` (`IdPartener`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `CK_TZ_SablonSalarii_PartD` CHECK ((`PartDebitMod` = 'FIX') = (`IdPartDebit` is not null)),
  CONSTRAINT `CK_TZ_SablonSalarii_PartC` CHECK ((`PartCreditMod` = 'FIX') = (`IdPartCredit` is not null))
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

CREATE TABLE IF NOT EXISTS `AVACONT_SURSA`.`TZ_SablonSalariiLinie` (
  `IdLinie`        int(11) NOT NULL AUTO_INCREMENT,
  `IdArticol`      int(11) NOT NULL,
  `Tip`            enum('RETINERE','VIRAMENT','NUMERAR') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'RETINERE = Access VA, VIRAMENT = PC, NUMERAR = PN',
  `IdParinte`      int(11) NULL DEFAULT NULL COMMENT 'For a VIRAMENT: the RETINERE it pays (Access PC.IDVAA); empty when it pays the article itself',
  `Ordine`         smallint(6) NOT NULL DEFAULT 0 COMMENT 'Position inside the article and kind',
  `Explicatie`     varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `ContDebit`      int(11) NOT NULL,
  `ContCredit`     int(11) NOT NULL,
  `PartDebitMod`   enum('NIMIC','OPERATIE','FIX') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'NIMIC',
  `IdPartDebit`    int(11) NULL DEFAULT NULL,
  `PartCreditMod`  enum('NIMIC','OPERATIE','FIX') CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'NIMIC',
  `IdPartCredit`   int(11) NULL DEFAULT NULL,
  `EsteCM`         tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Medical leave (account 423); only RETINERE and VIRAMENT; at most one per article and kind',
  `DataAdaugare`   datetime NOT NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE current_timestamp(),
  PRIMARY KEY (`IdLinie`) USING BTREE,
  INDEX `IX_TZ_SablonSalariiLinie_Articol`(`IdArticol` ASC, `Tip` ASC, `Ordine` ASC) USING BTREE,
  INDEX `IX_TZ_SablonSalariiLinie_Parinte`(`IdParinte` ASC) USING BTREE,
  INDEX `IX_TZ_SablonSalariiLinie_ContD`(`ContDebit` ASC) USING BTREE,
  INDEX `IX_TZ_SablonSalariiLinie_ContC`(`ContCredit` ASC) USING BTREE,
  INDEX `IX_TZ_SablonSalariiLinie_PartD`(`IdPartDebit` ASC) USING BTREE,
  INDEX `IX_TZ_SablonSalariiLinie_PartC`(`IdPartCredit` ASC) USING BTREE,
  CONSTRAINT `TZ_SablonSalariiLinie__Articol` FOREIGN KEY (`IdArticol`) REFERENCES `TZ_SablonSalarii` (`IdArticol`) ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT `TZ_SablonSalariiLinie__Parinte` FOREIGN KEY (`IdParinte`) REFERENCES `TZ_SablonSalariiLinie` (`IdLinie`) ON DELETE SET NULL ON UPDATE RESTRICT,
  CONSTRAINT `TZ_SablonSalariiLinie__ContD` FOREIGN KEY (`ContDebit`) REFERENCES `AVACONT_COMUN`.`DefaCont` (`Simbol`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `TZ_SablonSalariiLinie__ContC` FOREIGN KEY (`ContCredit`) REFERENCES `AVACONT_COMUN`.`DefaCont` (`Simbol`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `TZ_SablonSalariiLinie__PartD` FOREIGN KEY (`IdPartDebit`) REFERENCES `Parteneri` (`IdPartener`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `TZ_SablonSalariiLinie__PartC` FOREIGN KEY (`IdPartCredit`) REFERENCES `Parteneri` (`IdPartener`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `CK_TZ_SablonSalariiLinie_PartD` CHECK ((`PartDebitMod` = 'FIX') = (`IdPartDebit` is not null)),
  CONSTRAINT `CK_TZ_SablonSalariiLinie_PartC` CHECK ((`PartCreditMod` = 'FIX') = (`IdPartCredit` is not null))
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- Check (read only):
-- SHOW CREATE TABLE `AVACONT_SURSA`.`TZ_SablonSalarii`;
-- SHOW CREATE TABLE `AVACONT_SURSA`.`TZ_SablonSalariiLinie`;
-- Then: AvacontPush, tab «Sincronizare schema», SAFE, on every unit database.
