-- =====================================================================================
-- Slice 00EF-02 -- E-Factura tables of the UNIT databases. RUN THIS ON `AVACONT_SURSA` ONLY (HeidiSQL / mysql,
-- as root): AVACONT_SURSA is the template, and the schema sync copies its STRUCTURE to every unit database.
-- NOT RUN anywhere yet. Safe to run again (CREATE TABLE IF NOT EXISTS). Same engine / character set / collation as
-- the rest: InnoDB, utf8mb3 / utf8mb3_general_ci.
--
-- SLICE 00EF-13 (07.10.2026): the issuer's data (EF_Furnizor) and its accounts are NO LONGER a table of each unit
-- database: they are AVACONT_COMUN.Unitati_Detalii / Unitati_Conturi (sql/00EF_13_unitati_detalii.sql). The table
-- below that used to be here is gone; this file now has EIGHT tables.
--
-- ORDER (the whole 00EF-02):
--   1. this file, on AVACONT_SURSA;
--   2. AvacontPush, tab «Sincronizare schema»: SAFE, on every unit database (the eight tables appear in each);
--   3. 00EF_02_efactura_comun.sql, on AVACONT_COMUN (the token tables and EF_UM; not touched by the sync);
--   4. the operator writes the rows of AVACONT_COMUN.EF_UM.
--
-- Two cautions about the template: (a) it must be the database that is current when this runs, so the statement
-- below names it; (b) the Migrator's final AUTO_INCREMENT step lists the tables it converts, and these tables are
-- NOT in that list (they are born with AUTO_INCREMENT; the Migrator tab «E-Factura» inserts explicit keys, which
-- is allowed, and the counter then continues after the largest one).

USE `AVACONT_SURSA`;
--
-- Origin of every table: Access, read in Surse/RawExport (tables/ + modules). Dropped on
-- purpose: everything about accounting (IdOperatie, EFT_O, S, Complet, PlatiFacturi, ...):
-- not part of slice 00EF.
--
--   issued invoices   Factura -> EF_Facturi        FacturaC -> EF_FacturiLinii
--                     ClientiEF -> EF_Clienti      EF_UM -> AVACONT_COMUN.EF_UM (one list for all units)
--                     UNIT (address, contact) + Scheme row 'DPIFV' (C2, T3) -> AVACONT_COMUN.Unitati_Detalii (00EF-13)
--   received invoices EF -> EF_Mesaje   EFT -> EF_Primite   EFS -> EF_PrimiteLinii
--                     EFT_C -> EF_PrimiteNote   EFT_M -> EF_PrimiteMesaje
--
-- Names are ASCII (rule 0). Column names keep the Access words the operator knows.
-- The token table is NOT here: it is common to all units, see 00EF_02_efactura_comun.sql.
-- =====================================================================================


-- Units of measure (Access EF_UM) are NOT here: the list is the same for every unit (2113 UN/ECE codes, file
-- Surse/RawExport/tables/TABLE_VALUES/EF_UM.txt), so it is ONE table in AVACONT_COMUN, like BIC. See
-- 00EF_02_efactura_comun.sql. Eight tables in this file.

-- ---------------------------------------------------------------------------------
-- Customers of the issued invoices (Access ClientiEF).
-- IndFiscal = the «RO» prefix when the customer is a VAT payer. CNP = the customer is a
-- private person (the code field then holds a personal number: never log it).
-- ---------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `EF_Clienti` (
  `IdClient` int(11) NOT NULL AUTO_INCREMENT,
  `DenumireClient` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `CodFiscal` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `IndFiscal` varchar(8) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Cont` varchar(34) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'IBAN',
  `Banca` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Adresa` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Judetul` varchar(8) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Orasul` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Sector` varchar(8) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CNP` tinyint(4) NOT NULL DEFAULT 0,
  `DataAdaugare` datetime NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`IdClient`) USING BTREE,
  INDEX `IX_EF_Clienti_CodFiscal`(`CodFiscal` ASC) USING BTREE,
  INDEX `IX_EF_Clienti_Denumire`(`DenumireClient` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ---------------------------------------------------------------------------------
-- Issued invoices (Access Factura). Differences from Access, all deliberate:
--   * IdOperatie dropped (accounting).
--   * NumarFactura was a Double with a unique index on it ALONE; it is an integer and the
--     unique key is (SerieFactura, NumarFactura), because the XML identifier is Serie_Numar.
--   * DataFactura is a date; the amounts live in the lines.
--   * id_incarcare / id_descarcare: the identifiers ANAF returns; id_descarcare also takes
--     the word «Err» in Access, so it stays text.
--   * ATT -> AtasamentOriginal (attach the original invoice).
--   * DTQ -> DataAdaugare / DataModificare, as elsewhere.
-- TipFactura: 380 = invoice, 384 = corrected invoice (the two values of the combo in the form).
-- IdFacturaA / SerieFacturaA / NumarFacturaA: the invoice this one corrects (kept as in Access;
-- the form EFACTURA_ADD fills them, how exactly is read in 00EF-07).
-- ---------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `EF_Facturi` (
  `IdFactura` int(11) NOT NULL AUTO_INCREMENT,
  `IdClient` int(11) NOT NULL,
  `SerieFactura` varchar(10) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '',
  `NumarFactura` int(11) NOT NULL,
  `DataFactura` date NOT NULL,
  `TipFactura` varchar(3) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '380',
  `Comentarii` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `BT_13` varchar(30) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Order reference sent as cac:OrderReference/cbc:ID',
  `ContPlata` varchar(34) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'The unit IBAN printed as payment account; characters 5-8 are the bank code (AVACONT_COMUN.BIC)',
  `Anulata` tinyint(4) NOT NULL DEFAULT 0,
  `Trimisa` tinyint(4) NOT NULL DEFAULT 0,
  `id_incarcare` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'ANAF upload index, set when the XML was accepted for upload',
  `id_descarcare` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'ANAF download id when the status is ok; the word Err when ANAF refused it',
  `AtasamentOriginal` tinyint(4) NOT NULL DEFAULT 0,
  `EroareAnaf` varchar(2000) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Slice 00EF-07: why ANAF refused the invoice (id_descarcare = Err); no secret in it',
  `IdFacturaA` int(11) NULL DEFAULT NULL,
  `SerieFacturaA` varchar(10) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `NumarFacturaA` varchar(10) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Corectata` int(11) NOT NULL DEFAULT 0 COMMENT 'Access: Double default 0, meaning not documented; kept as a number until 00EF-07 reads the form',
  `DataAdaugare` datetime NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`IdFactura`) USING BTREE,
  UNIQUE INDEX `UQ_EF_Facturi_SerieNumar`(`SerieFactura` ASC, `NumarFactura` ASC) USING BTREE,
  INDEX `IX_EF_Facturi_IdClient`(`IdClient` ASC) USING BTREE,
  INDEX `IX_EF_Facturi_id_incarcare`(`id_incarcare` ASC) USING BTREE,
  INDEX `IX_EF_Facturi_id_descarcare`(`id_descarcare` ASC) USING BTREE,
  CONSTRAINT `FK_EF_Facturi_Client` FOREIGN KEY (`IdClient`) REFERENCES `EF_Clienti` (`IdClient`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ---------------------------------------------------------------------------------
-- Lines of an issued invoice (Access FacturaC). Valoare = what the XML sends as
-- LineExtensionAmount; Access stored it as a Double next to Cant and PU. Platit and Grup are
-- kept (Platit: the line was paid; Grup: grouping number of the form), not read by the XML.
-- NrCrt is text in Access (the line identifier written to cbc:ID).
-- ---------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `EF_FacturiLinii` (
  `IdContinut` int(11) NOT NULL AUTO_INCREMENT,
  `IdFactura` int(11) NOT NULL,
  `NrCrt` varchar(16) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '',
  `Continut` text CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Um` varchar(8) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '' COMMENT 'AVACONT_COMUN.EF_UM.Cod (another database, so no foreign key; Access had none either, and old rows may hold a code that is gone)',
  `Cant` decimal(18,3) NOT NULL DEFAULT 0.000,
  `PU` decimal(18,4) NOT NULL DEFAULT 0.0000,
  `Valoare` decimal(18,2) NOT NULL DEFAULT 0.00,
  `Platit` tinyint(4) NOT NULL DEFAULT 0,
  `Grup` int(11) NOT NULL DEFAULT 0,
  PRIMARY KEY (`IdContinut`) USING BTREE,
  INDEX `IX_EF_FacturiLinii_IdFactura`(`IdFactura` ASC) USING BTREE,
  CONSTRAINT `FK_EF_FacturiLinii_Factura` FOREIGN KEY (`IdFactura`) REFERENCES `EF_Facturi` (`IdFactura`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- =====================================================================================
-- RECEIVED invoices (suppliers' invoices downloaded from ANAF). Read by the view
-- «E-Factura» of KbotForm (joined to FX_DDF_Parteneri.CodFiscal). NO accounting columns.
-- =====================================================================================

-- One ANAF message per row (Access EF). IdSol is the key everything hangs from (Access:
-- the primary key was id_sol, text). The original XML is kept here so the invoice can be
-- shown and re-read without calling ANAF again; LONGBLOB because the file is UTF-8 and the
-- column character set is utf8mb3.
CREATE TABLE IF NOT EXISTS `EF_Mesaje` (
  `IdMesaj` int(11) NOT NULL AUTO_INCREMENT,
  `IdSol` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'ANAF id_solicitare: the key of the message and of the XML file inside the downloaded zip',
  `IdIncarcare` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Access EF.ID: the upload id of the invoice at the supplier; the name of the downloaded zip',
  `CifEmitent` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Access EF.cif: tax code of the supplier, digits only',
  `DataMesaj` date NULL DEFAULT NULL,
  `CuiUnitate` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Access EF.cui_unit: the unit that received it',
  `Nou` tinyint(4) NOT NULL DEFAULT 1,
  `NumeFisier` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `XmlContinut` longblob NULL DEFAULT NULL,
  `DataAdaugare` datetime NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`IdMesaj`) USING BTREE,
  UNIQUE INDEX `UQ_EF_Mesaje_IdSol`(`IdSol` ASC) USING BTREE,
  INDEX `IX_EF_Mesaje_CifEmitent`(`CifEmitent` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- One invoice per row (Access EFT). CuiNormalizat = CUI without the «RO» prefix, spaces and
-- dots, written by the server on every insert/update; the join with FX_DDF_Parteneri.CodFiscal
-- strips the same things from that side. Tip: FC invoice, NC credit note (or the XML's
-- document name when it is neither). Semn: -1 for a credit note with a positive total, else 1
-- (Access EFT.NC; a credit note that already has a negative total is not multiplied again).
CREATE TABLE IF NOT EXISTS `EF_Primite` (
  `IdPrimita` int(11) NOT NULL AUTO_INCREMENT,
  `IdSol` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `NrFact` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DataFact` date NULL DEFAULT NULL,
  `DataScad` date NULL DEFAULT NULL,
  `CotaTVA` decimal(6,2) NOT NULL DEFAULT 0.00,
  `TVA` decimal(18,2) NOT NULL DEFAULT 0.00,
  `Valoare` decimal(18,2) NOT NULL DEFAULT 0.00 COMMENT 'Taxable amount',
  `Total` decimal(18,2) NOT NULL DEFAULT 0.00 COMMENT 'Payable amount',
  `CUI` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CuiNormalizat` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DenumireP` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Supplier name',
  `Adresa` varchar(500) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Atasament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Name of an attached file, when the XML carries one',
  `Tip` varchar(16) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT 'FC',
  `Semn` tinyint(4) NOT NULL DEFAULT 1,
  `Ref` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Number (and date) of the invoice a credit note refers to',
  `IdPrimitaRef` int(11) NULL DEFAULT NULL,
  `DataAdaugare` datetime NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`IdPrimita`) USING BTREE,
  INDEX `IX_EF_Primite_IdSol`(`IdSol` ASC) USING BTREE,
  INDEX `IX_EF_Primite_CuiNormalizat`(`CuiNormalizat` ASC, `DataFact` ASC) USING BTREE,
  INDEX `IX_EF_Primite_NrFact`(`NrFact` ASC) USING BTREE,
  CONSTRAINT `FK_EF_Primite_Mesaj` FOREIGN KEY (`IdSol`) REFERENCES `EF_Mesaje` (`IdSol`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_EF_Primite_Ref` FOREIGN KEY (`IdPrimitaRef`) REFERENCES `EF_Primite` (`IdPrimita`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- Lines of a received invoice (Access EFS). Line identifier ID is the XML cbc:ID (text).
CREATE TABLE IF NOT EXISTS `EF_PrimiteLinii` (
  `IdLinie` int(11) NOT NULL AUTO_INCREMENT,
  `IdPrimita` int(11) NOT NULL,
  `NrLinie` varchar(16) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Denumire` varchar(500) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Explicatie` text CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Unit` varchar(16) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'unitCode of the XML',
  `Cant` decimal(18,3) NOT NULL DEFAULT 0.000,
  `Pret` decimal(18,4) NOT NULL DEFAULT 0.0000,
  `Valoare` decimal(18,2) NOT NULL DEFAULT 0.00,
  PRIMARY KEY (`IdLinie`) USING BTREE,
  INDEX `IX_EF_PrimiteLinii_IdPrimita`(`IdPrimita` ASC) USING BTREE,
  CONSTRAINT `FK_EF_PrimiteLinii_Primita` FOREIGN KEY (`IdPrimita`) REFERENCES `EF_Primite` (`IdPrimita`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- Notes of a received invoice (Access EFT_C): the XML's cbc:Note and similar text nodes.
CREATE TABLE IF NOT EXISTS `EF_PrimiteNote` (
  `IdNota` int(11) NOT NULL AUTO_INCREMENT,
  `IdPrimita` int(11) NOT NULL,
  `Nota` text CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DataAdaugare` datetime NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`IdNota`) USING BTREE,
  INDEX `IX_EF_PrimiteNote_IdPrimita`(`IdPrimita` ASC) USING BTREE,
  CONSTRAINT `FK_EF_PrimiteNote_Primita` FOREIGN KEY (`IdPrimita`) REFERENCES `EF_Primite` (`IdPrimita`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- Messages exchanged about a received invoice (Access EFT_M), e.g. the buyer's message sent
-- through TrimiteMesajFactura. IdMesajAnaf = Access EFT_M.ID (the ANAF message id).
CREATE TABLE IF NOT EXISTS `EF_PrimiteMesaje` (
  `IdMsg` int(11) NOT NULL AUTO_INCREMENT,
  `IdPrimita` int(11) NOT NULL,
  `IdSol` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `IdMesajAnaf` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Mesaj` text CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `DataMesaj` datetime NULL DEFAULT NULL,
  `DataAdaugare` datetime NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`IdMsg`) USING BTREE,
  INDEX `IX_EF_PrimiteMesaje_IdPrimita`(`IdPrimita` ASC) USING BTREE,
  INDEX `IX_EF_PrimiteMesaje_IdSol`(`IdSol` ASC) USING BTREE,
  CONSTRAINT `FK_EF_PrimiteMesaje_Primita` FOREIGN KEY (`IdPrimita`) REFERENCES `EF_Primite` (`IdPrimita`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- =====================================================================================
-- Check afterwards (one statement at a time):
--   SELECT TABLE_NAME, TABLE_COLLATION FROM information_schema.TABLES
--    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME LIKE 'EF\_%';     -- 8 rows, utf8mb3_general_ci
--   SELECT COUNT(*) FROM information_schema.REFERENTIAL_CONSTRAINTS
--    WHERE CONSTRAINT_SCHEMA = DATABASE() AND CONSTRAINT_NAME LIKE 'FK\_EF\_%';   -- 7
-- =====================================================================================
