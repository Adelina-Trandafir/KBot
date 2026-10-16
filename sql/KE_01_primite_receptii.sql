-- KE01: received e-invoices <-> receptions. Run by hand ONLY on AVACONT_SURSA, then propagate the structure with the schema
-- sync to the unit databases (same procedure as 00EF_17_primite_asocieri.sql). Plan: docs/KE_PLAN.md.
--
-- 1. Two columns on the lines of a received invoice:
--    IdClsf  = the accountant's choice: the budget classification of the line (Clasificatii.IDClsf, rule 0080-01). NULL = not chosen.
--              No foreign key: Clasificatii is deleted in cascade when a unit is deleted, and a RESTRICT foreign key would block the
--              sync of the nomenclature (same decision as EF_PrimiteOperatii.IdClsf).
--    CotaTVA = the VAT rate of the line, read from the invoice XML (InvoiceLine/Item/ClassifiedTaxCategory/Percent). NULL = not read
--              yet (invoices synchronised before KE01): the server reads it from the kept XML when the links are first opened.
ALTER TABLE `EF_PrimiteLinii`
  ADD COLUMN IF NOT EXISTS `IdClsf` int(11) NULL DEFAULT NULL COMMENT 'KE01: Clasificatii.IDClsf chosen by the accountant for this line; NULL = not chosen',
  ADD COLUMN IF NOT EXISTS `CotaTVA` decimal(20,4) NULL DEFAULT NULL COMMENT 'KE01: VAT rate of the line from the XML; NULL = not read yet';

-- 2. The link invoice <-> reception, per classification.
--    An invoice can cover several receptions, a reception can be covered by several invoices.
--    Suma = the amount WITH VAT allocated on that classification; negative for a credit note (its reception is a normal reception
--    with a negative sign).
--    The reception is named in two ways, one after the other:
--      IDRH / IDR = the numbers reserved by the K-BOT marker (POST /api/forexe/marcaj/rezerva) before the save on the site;
--      IDRR       = the identity of the reception: written at once for an existing / modified reception, or found from IDRH after the
--                   download.
--    The rest of a reception reads IDRR and, when it is empty, FX_Receptii_H.IDRR of IDRH.
--    No foreign key on CodAngajament / IDRR: a reception deleted on the site stays in the K-BOT database (FX_Receptii_R.Sters = 1)
--    and the link must stay visible, not vanish in cascade.
CREATE TABLE IF NOT EXISTS `EF_PrimiteReceptii` (
  `IdLegatura` int(11) NOT NULL AUTO_INCREMENT,
  `IdPrimita` int(11) NOT NULL,
  `CodAngajament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `IdClsf` int(11) NOT NULL COMMENT 'Clasificatii.IDClsf (0080-01 rule), no foreign key',
  `IDRR` int(11) NULL DEFAULT NULL COMMENT 'FX_Receptii_R.IDRR when known',
  `IDRH` int(11) NULL DEFAULT NULL COMMENT 'FX_Receptii_H.IDRH reserved by the K-BOT marker',
  `IDR` int(11) NULL DEFAULT NULL COMMENT 'FX_Receptii.IDR reserved by the K-BOT marker',
  `Suma` decimal(20,4) NOT NULL DEFAULT 0.0000 COMMENT 'Allocated amount with VAT; negative for a credit note',
  `Tip` varchar(16) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'existenta' COMMENT 'existenta / noua / modificata',
  `UN` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'who linked it',
  `DataAdaugare` datetime NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`IdLegatura`) USING BTREE,
  INDEX `IX_EF_PrimiteReceptii_IdPrimita`(`IdPrimita` ASC) USING BTREE,
  INDEX `IX_EF_PrimiteReceptii_IDRR`(`IDRR` ASC, `IdClsf` ASC) USING BTREE,
  INDEX `IX_EF_PrimiteReceptii_IDRH`(`IDRH` ASC) USING BTREE,
  INDEX `IX_EF_PrimiteReceptii_Cod`(`CodAngajament`(100) ASC) USING BTREE,
  CONSTRAINT `FK_EF_PrimiteReceptii_Primita` FOREIGN KEY (`IdPrimita`) REFERENCES `EF_Primite` (`IdPrimita`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;
