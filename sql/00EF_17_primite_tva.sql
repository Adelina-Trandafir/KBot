-- 00EF-17: VAT per rate of a received e-invoice (one row per TaxSubtotal of the XML).
-- EF_Primite keeps the totals (TVA = all VAT) and CotaTVA = the rate of the biggest taxable amount; this table has them all.
-- Run on AVACONT_SURSA, then schema sync to every unit database. Already stored invoices get no rows here until
-- they are read again (delete the row in EF_Mesaje and run the sync, or ask for a refill).
CREATE TABLE IF NOT EXISTS `EF_PrimiteTVA` (
  `IdTva` int(11) NOT NULL AUTO_INCREMENT,
  `IdPrimita` int(11) NOT NULL,
  `Categorie` varchar(8) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'UBL TaxCategory/ID: S, Z, E, AE, ...',
  `CotaTVA` decimal(6,2) NOT NULL DEFAULT 0.00,
  `Baza` decimal(18,2) NOT NULL DEFAULT 0.00 COMMENT 'Taxable amount',
  `TVA` decimal(18,2) NOT NULL DEFAULT 0.00,
  PRIMARY KEY (`IdTva`) USING BTREE,
  INDEX `IX_EF_PrimiteTVA_IdPrimita`(`IdPrimita` ASC) USING BTREE,
  CONSTRAINT `FK_EF_PrimiteTVA_Primita` FOREIGN KEY (`IdPrimita`) REFERENCES `EF_Primite` (`IdPrimita`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;
