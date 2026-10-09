-- 00EF-17: manual (and remembered) link between a received e-invoice and a DDF.
-- Run on AVACONT_SURSA, then schema sync to every unit database (same procedure as 00EF_02).
-- The AUTOMATIC link needs no row: a received invoice belongs to every DDF whose FX_DDF_Parteneri.CodFiscal matches its
-- EF_Primite.CuiNormalizat (RO, spaces and dots stripped on both sides). A row here is the operator's own choice:
-- a supplier whose code differs, or an invoice bound to a DDF whose partner is another firm.
CREATE TABLE IF NOT EXISTS `EF_PrimiteAsocieri` (
  `IdAsociere` int(11) NOT NULL AUTO_INCREMENT,
  `IdPrimita` int(11) NOT NULL,
  `IDDF` int(11) NOT NULL,
  `UN` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'who linked it',
  `DataAdaugare` datetime NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`IdAsociere`) USING BTREE,
  UNIQUE INDEX `UQ_EF_PrimiteAsocieri`(`IdPrimita` ASC, `IDDF` ASC) USING BTREE,
  INDEX `IX_EF_PrimiteAsocieri_IDDF`(`IDDF` ASC) USING BTREE,
  CONSTRAINT `FK_EF_PrimiteAsocieri_Primita` FOREIGN KEY (`IdPrimita`) REFERENCES `EF_Primite` (`IdPrimita`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_EF_PrimiteAsocieri_DDF` FOREIGN KEY (`IDDF`) REFERENCES `FX_DDF` (`IDDF`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;
