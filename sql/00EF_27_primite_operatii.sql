-- Facturi primite: legaturile cu operatiile contabile (Access EFT_O).
-- De executat manual numai pe AVACONT_SURSA, apoi de propagat structura prin sincronizarea schemei catre bazele
-- unitatilor (inclusiv DC-ul importat). Tabelul se umple deocamdata doar prin KBot.Migrator (fila E-Factura).
--
-- O factura primita este «prelucrata» cand are cel putin un rand aici; suma ramasa = EF_Primite.Total (cu Semn) minus SUM(Suma).
--
-- IdOperatie = Oper.IdOperatie din Access. Operatiile contabile nu sunt inca in MariaDB: de aceea NU are cheie straina
-- si nu se leaga de nimic; cand vor exista, se leaga aici.
-- IdClsf     = Clasificatii.IDClsf, ca pe celelalte tabele FX (slice 0080-01). Access EFT_O.IdClsf tine id-ul ACCESS al
-- clasificatiei; migratorul il converteste cautand, pe IdUnitate-ul randului, exact un rand din Clasificatii dupa
-- (IdClsfAcc, IdUnitate) si scrie IDClsf-ul gasit. Id-ul Access NU se pastreaza aici. 0 / NULL in Access = NULL.
-- Fara cheie straina: Clasificatii se sterge in cascada la stergerea unei unitati, iar o cheie straina RESTRICT ar bloca
-- sincronizarea nomenclatorului.
-- IdUnitate  = EFT_O.IdUnitate din Access (aceeasi valoare ca Clasificatii.IdUnitate); o unitate cu mai multe surse
-- (02A / 02E) are un IdUnitate pe sursa, deci randurile aceleiasi facturi pot avea IdUnitate diferit.
CREATE TABLE IF NOT EXISTS `EF_PrimiteOperatii` (
  `IdOperatiePrimita` int(11) NOT NULL COMMENT 'Access EFT_O.IDEFTO, kept',
  `IdPrimita` int(11) NOT NULL COMMENT 'EF_Primite.IdPrimita (Access EFT_O.IDEFT)',
  `IdOperatie` int(11) NOT NULL DEFAULT 0 COMMENT 'Access Oper.IdOperatie; no foreign key, the operations are not in MariaDB yet',
  `IdClsf` int(11) NULL DEFAULT NULL COMMENT 'Clasificatii.IDClsf (0080-01 rule); NULL = no classification',
  `IdUnitate` int(11) NULL DEFAULT NULL,
  `Sursa` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Source and sector as Access kept it, e.g. 02A',
  `Suma` decimal(20,4) NOT NULL DEFAULT 0.0000 COMMENT 'Booked amount; negative for a credit note',
  `DataAdaugare` datetime NULL DEFAULT current_timestamp() COMMENT 'Access EFT_O.DTQ',
  PRIMARY KEY (`IdOperatiePrimita`) USING BTREE,
  INDEX `IX_EF_PrimiteOperatii_IdPrimita`(`IdPrimita` ASC) USING BTREE,
  INDEX `IX_EF_PrimiteOperatii_IdOperatie`(`IdOperatie` ASC) USING BTREE,
  INDEX `IX_EF_PrimiteOperatii_IdClsf`(`IdClsf` ASC) USING BTREE,
  CONSTRAINT `FK_EF_PrimiteOperatii_Primita` FOREIGN KEY (`IdPrimita`) REFERENCES `EF_Primite` (`IdPrimita`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;
