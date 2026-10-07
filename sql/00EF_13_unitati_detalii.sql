-- =====================================================================================
-- Slice 00EF-13 -- the unit as the ISSUER of the E-Factura invoices, and its bank accounts, move from the UNIT
-- databases (EF_Furnizor, EF_FurnizorConturi) to the COMMON database: one row per unit, no more table per unit.
-- RUN THIS ON `AVACONT_COMUN` ONLY (HeidiSQL / mysql, as root). NOT RUN anywhere yet. Safe to run again
-- (CREATE TABLE IF NOT EXISTS). Same engine / character set / collation as the rest: InnoDB, utf8mb3 / utf8mb3_general_ci.
--
-- This file REPLACES the two tables that 00EF_02 (EF_Furnizor), 00EF_06 (NumarInitial) and 00EF_12 (EF_FurnizorConturi)
-- used to create in AVACONT_SURSA; those three files no longer create them. AVACONT_SURSA / the unit databases do NOT
-- need the schema sync for this slice: nothing is added to them, two tables are no longer born in them.
--
-- ORDER:
--   1. this file, on AVACONT_COMUN;
--   2. ONLY for a unit database that already got the old tables (the old DDL was never run on the live server, so
--      normally none): the copy at the bottom of this file, then DROP the old tables of that unit.
--
-- The key of both tables is `DC` = the database name of the unit = AVACONT_COMUN.Unitati.DC, the same key
-- Unitati_Ani and Unitati_Utilizatori use. The server takes it from the session (`g.session.db_name`), never from
-- the client.
--
-- The «Reprezentant» (contact) column of EF_Furnizor is gone on purpose: the window no longer asks for it.
-- =====================================================================================

USE `AVACONT_COMUN`;

-- ---------------------------------------------------------------------------------
-- The unit as the INVOICE ISSUER (was EF_Furnizor, one row per unit database). The name and the tax code of the unit
-- are ALSO in Unitati (NumeUnitate, CF); the copy here is what goes into the XML, so the operator can fix it without
-- touching Unitati.
--   SerieFactura / NumarInitial: read-only for the window as soon as the unit has one issued invoice (the server
--     refuses a change then, see facturi.furnizor_set).
--   AnafPreluat: 1 once the button «Preia de la ANAF» was used (it works ONCE per unit: ANAF's name and address
--     overwrite what is typed, so it is allowed a single time); DataAnafPreluat = when.
-- ---------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Unitati_Detalii` (
  `DC` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Unitati.DC = the unit database name',
  `Denumire` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `CodFiscal` varchar(32) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'As sent to ANAF: digits, or RO + digits when the unit is a VAT payer',
  `Adresa` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Orasul` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Judetul` varchar(8) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'County code the XML puts after «RO-» (e.g. PH)',
  `Mail` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Telefon` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `SerieFactura` varchar(10) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Was Scheme.T3 of DPIFV; goes in front of the number: Serie_Numar',
  `NumarInitial` int(11) NOT NULL DEFAULT 1 COMMENT 'First invoice number of a series that has no invoice yet (was Scheme.C5 of DPIFV)',
  `AfiseazaPrimiteNoi` tinyint(4) NOT NULL DEFAULT 0 COMMENT 'Was Scheme.C2 of DPIFV: tick the new received invoices before downloading them',
  `AnafPreluat` tinyint(4) NOT NULL DEFAULT 0 COMMENT '1 = the name and address were already taken from ANAF once; the button is then closed',
  `DataAnafPreluat` datetime NULL DEFAULT NULL,
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`DC`) USING BTREE,
  CONSTRAINT `FK_Unitati_Detalii_Unitati` FOREIGN KEY (`DC`) REFERENCES `Unitati` (`DC`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- ---------------------------------------------------------------------------------
-- The bank accounts (IBAN) of the unit as the ISSUER (was EF_FurnizorConturi). These are the accounts of the CURRENT
-- UNIT, NOT the accounts of the partners it pays: those live on the partners. Customers' accounts are EF_Clienti.Cont.
-- Banca = the name of the bank DEDUCED from the account: characters 5-8 of the IBAN are the bank code, looked up in
-- AVACONT_COMUN.BIC (Cod -> Banca), the same rule the invoice XML uses. The server writes it on every save; NULL when
-- the code is not in BIC. Nobody types it. The invoice keeps its own copy of the account as text
-- (EF_Facturi.ContPlata), so deleting an account here never changes an invoice that was already issued.
-- ---------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Unitati_Conturi` (
  `IdCont` int(11) NOT NULL AUTO_INCREMENT,
  `DC` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `Cont` varchar(34) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'IBAN, upper case, no spaces; the server checks the length and the mod-97 digits',
  `Banca` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Deduced from characters 5-8 of the IBAN (BIC.Banca); NULL = code not in BIC',
  `DataAdaugare` datetime NULL DEFAULT current_timestamp(),
  `DataModificare` datetime NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`IdCont`) USING BTREE,
  UNIQUE INDEX `UQ_Unitati_Conturi_DC_Cont`(`DC` ASC, `Cont` ASC) USING BTREE,
  CONSTRAINT `FK_Unitati_Conturi_Unitati` FOREIGN KEY (`DC`) REFERENCES `Unitati` (`DC`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- =====================================================================================
-- Check afterwards (one statement at a time):
--   SHOW CREATE TABLE Unitati_Detalii;      SHOW CREATE TABLE Unitati_Conturi;      -- utf8mb3_general_ci, 1 FK each
-- =====================================================================================

-- =====================================================================================
-- COPY from a unit database that already has the OLD tables (change `001_GR23` twice; one unit at a time; NOT run).
-- The old EF_Furnizor never had AnafPreluat, so it starts at 0. Reprezentant is not carried.
--
--   INSERT INTO AVACONT_COMUN.Unitati_Detalii
--          (DC, Denumire, CodFiscal, Adresa, Orasul, Judetul, Mail, Telefon, SerieFactura, NumarInitial, AfiseazaPrimiteNoi)
--   SELECT '001_GR23', Denumire, CodFiscal, Adresa, Orasul, Judetul, Mail, Telefon, SerieFactura, NumarInitial, AfiseazaPrimiteNoi
--     FROM `001_GR23`.EF_Furnizor WHERE Id = 1
--   ON DUPLICATE KEY UPDATE Denumire = VALUES(Denumire);
--
--   INSERT IGNORE INTO AVACONT_COMUN.Unitati_Conturi (DC, Cont, Banca)
--   SELECT '001_GR23', Cont, Banca FROM `001_GR23`.EF_FurnizorConturi;
--
--   DROP TABLE `001_GR23`.EF_FurnizorConturi;   DROP TABLE `001_GR23`.EF_Furnizor;
-- =====================================================================================
