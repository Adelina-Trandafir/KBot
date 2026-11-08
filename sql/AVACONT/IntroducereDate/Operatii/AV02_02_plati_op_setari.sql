-- AV02-02: payments of invoices, orders of payment (OP) and the setting table of the accounting.
-- Access: PlatiFacturi, OPuri, OPME (+ the scheme flag _OP9.C6 / LEGUNIT.OP that became the setting PlajaComunaOPuri).
-- Design and reasons: docs/PLANS/PLAN_04EF_01_PLATI_CONTARI_OP9_OPURI2017.md, docs/worklog/SLICE-AV02-02-plati-op-setari.md.
-- Run by the operator on AVACONT_SURSA only, then schema sync to every unit database (same procedure as AV02_01). NOT RUN anywhere yet.
-- Safe to run again (CREATE TABLE IF NOT EXISTS). utf8mb3 / utf8mb3_general_ci (house rule). Order matters (foreign keys): the four tables below, top to bottom.
--
-- Rules written into the keys:
--  * Nothing is deleted by cascade (accounting data): every foreign key is RESTRICT. The server deletes the allocations, then the OP, then the payment operation,
--    inside one transaction, and only while the OP is NOT locked (not in an OPME and not confirmed by a bank statement line).
--  * One OP = one payment operation (TZ_OPuri.IdOper is UNIQUE). An OP may sit in one OPME (IdOPME, optional).
--  * The number of the OP (NrOP) is given by the server: highest + 1 over the subunit, or over all subunits when PlajaComunaOPuri is ON (see TZ_Settings).
--    The key is NOT unique on purpose: with a common range the numbers of the subunits that do not share it can repeat.
--  * The Access ids are kept only in the columns ending in «Access» (the migration can run again and adds only what is missing).
--  * Every table has the pair CreatedAt / UpdatedAt (house rule for new tables).

-- The settings of the accounting at the level of the database: one row per setting, for the whole unit (IdSubunitate NULL) or for one subunit.
-- Model: VC_Settings. Reading order for a subunit: its own row, else the row of the unit (NULL), else the default written in the code.
-- A NULL column cannot be part of a primary key, so the key is an own id and the uniqueness of (SettingKey, subunit) is given by the generated column
-- IdSubunitateCheie (0 when the setting is for the whole unit): the database itself refuses a second row for the same setting and the same subunit.
CREATE TABLE IF NOT EXISTS `TZ_Settings` (
  `IdSetting` int(11) NOT NULL AUTO_INCREMENT,
  `SettingKey` varchar(64) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'ASCII name read by the code, e.g. PlajaComunaOPuri',
  `IdSubunitate` int(11) NULL DEFAULT NULL COMMENT 'TZ_Subunitati: the setting of that subunit; NULL = the setting of the whole unit',
  `IdSubunitateCheie` int(11) AS (IFNULL(`IdSubunitate`, 0)) PERSISTENT COMMENT 'Generated: 0 when IdSubunitate is NULL; makes (SettingKey, subunit) unique',
  `Valoare` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'The value as text; a yes/no setting holds 0 or 1',
  `Version` bigint(20) NOT NULL DEFAULT 1 COMMENT 'Raised on every change (as VC_Settings.Version)',
  `CreatedAt` datetime NULL DEFAULT current_timestamp() COMMENT 'When the row was created',
  `UpdatedAt` datetime NULL DEFAULT NULL ON UPDATE current_timestamp() COMMENT 'When the row was last changed (NULL = not changed since it was created)',
  PRIMARY KEY (`IdSetting`) USING BTREE,
  UNIQUE INDEX `UK_TZ_Settings_Cheie`(`SettingKey` ASC, `IdSubunitateCheie` ASC) USING BTREE,
  -- ON UPDATE RESTRICT (not CASCADE): MariaDB refuses a generated column (IdSubunitateCheie) built on a column whose foreign key cascades (error 1901).
  CONSTRAINT `TZ_Settings__Subunitate` FOREIGN KEY (`IdSubunitate`) REFERENCES `TZ_Subunitati` (`IdSubunitate`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- The header of a multiple order of payment (a lot of OPs sent together). Access OPME.
CREATE TABLE IF NOT EXISTS `TZ_OPME` (
  `IdOPME` int(11) NOT NULL AUTO_INCREMENT,
  `IdSubunitate` int(11) NOT NULL COMMENT 'TZ_Subunitati: the subunit that issues the lot',
  `An` smallint(6) NOT NULL,
  `NrOPME` int(11) NOT NULL DEFAULT 0 COMMENT 'Number of the lot (Access NrOPME)',
  `DataDoc` date NULL DEFAULT NULL,
  `NrRec` int(11) NOT NULL DEFAULT 0 COMMENT 'Access NrRec, kept as it is (the plan reads it as the number of orders in the lot; the receipt is kept apart in K-BOT: to confirm)',
  `Total` decimal(18, 2) NOT NULL DEFAULT 0.00,
  `Platitor` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CIF` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Adresa` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `IdOPMEAccess` int(11) NULL DEFAULT NULL COMMENT 'Access OPME.IDOPME; NULL = made in K-BOT',
  `UN` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Who made it (e-mail); NULL = migrated from Access',
  `CreatedAt` datetime NULL DEFAULT current_timestamp() COMMENT 'When the row was created (migrated rows: the Access DTQ)',
  `UpdatedAt` datetime NULL DEFAULT NULL ON UPDATE current_timestamp() COMMENT 'When the row was last changed (NULL = not changed since it was created)',
  PRIMARY KEY (`IdOPME`) USING BTREE,
  UNIQUE INDEX `UK_TZ_OPME_Access`(`IdSubunitate` ASC, `IdOPMEAccess` ASC) USING BTREE,
  INDEX `IX_TZ_OPME_Numar`(`IdSubunitate` ASC, `An` ASC, `NrOPME` ASC) USING BTREE,
  CONSTRAINT `TZ_OPME__Subunitate` FOREIGN KEY (`IdSubunitate`) REFERENCES `TZ_Subunitati` (`IdSubunitate`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- One order of payment = one payment operation. Access OPuri (Oper = the payment operation, IDOPME = the lot).
CREATE TABLE IF NOT EXISTS `TZ_OPuri` (
  `IdOP` int(11) NOT NULL AUTO_INCREMENT,
  `IdOper` int(11) NOT NULL COMMENT 'TZ_Oper: the payment operation this order pays (one order = one payment = one operation)',
  `IdSubunitate` int(11) NOT NULL COMMENT 'TZ_Subunitati: the issuing subunit (the numbering and the lock read it)',
  `IdOPME` int(11) NULL DEFAULT NULL COMMENT 'TZ_OPME: the lot this order was added to; NULL = a simple order. A non-NULL value LOCKS the order',
  `NrOP` int(11) NOT NULL COMMENT 'Number of the order, given by the server on save (highest + 1 by PlajaComunaOPuri); the number shown before saving is provisional',
  `DataOP` date NOT NULL,
  `Valoare` decimal(18, 2) NOT NULL DEFAULT 0.00 COMMENT 'Total of the order = the total of its allocations (TZ_PlatiFacturi.Platit)',
  `Explicatie` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '' COMMENT 'ASCII only, no comma, 1..75 characters (rule of the order text)',
  `Documente` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Numbers of the documents paid, separated by ;',
  `IdPartener` int(11) NULL DEFAULT NULL COMMENT 'Parteneri.IdPartener (Access CodPartener); NULL = unknown (migrated order whose partner is not in the unit)',
  `DenPartener` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CFPartener` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `ContBeneficiar` varchar(34) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'IBAN of the beneficiary',
  `bBIC` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'BIC of the beneficiary bank',
  `DenUnitate` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CFUnitate` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `ContUnitate` varchar(34) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'IBAN of the unit (treasury account)',
  `pBIC` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'BIC of the payer bank (TREZROBU)',
  `CodAngajament` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodIndicator` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `CodProgram` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `Auto` tinyint(1) NOT NULL DEFAULT 0,
  `DataIN` datetime NULL DEFAULT NULL COMMENT 'When the order was made (Access OPuri.DataIN)',
  `IdOPAccess` int(11) NULL DEFAULT NULL COMMENT 'Access OPuri.IDOP; NULL = made in K-BOT. Bank statement lines of Access carry this id in FX_Extrase.IDOP',
  `UN` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Who made it (e-mail); NULL = migrated from Access',
  `CreatedAt` datetime NULL DEFAULT current_timestamp() COMMENT 'When the row was created (migrated rows: the Access DTQ)',
  `UpdatedAt` datetime NULL DEFAULT NULL ON UPDATE current_timestamp() COMMENT 'When the row was last changed (NULL = not changed since it was created)',
  PRIMARY KEY (`IdOP`) USING BTREE,
  UNIQUE INDEX `UK_TZ_OPuri_Oper`(`IdOper` ASC) USING BTREE,
  UNIQUE INDEX `UK_TZ_OPuri_Access`(`IdSubunitate` ASC, `IdOPAccess` ASC) USING BTREE,
  INDEX `IX_TZ_OPuri_Numar`(`IdSubunitate` ASC, `NrOP` ASC) USING BTREE,
  INDEX `IX_TZ_OPuri_NrOP`(`NrOP` ASC) USING BTREE,
  INDEX `IX_TZ_OPuri_OPME`(`IdOPME` ASC) USING BTREE,
  INDEX `IX_TZ_OPuri_Partener`(`IdPartener` ASC) USING BTREE,
  CONSTRAINT `TZ_OPuri__Oper` FOREIGN KEY (`IdOper`) REFERENCES `TZ_Oper` (`IdOper`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `TZ_OPuri__Subunitate` FOREIGN KEY (`IdSubunitate`) REFERENCES `TZ_Subunitati` (`IdSubunitate`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `TZ_OPuri__OPME` FOREIGN KEY (`IdOPME`) REFERENCES `TZ_OPME` (`IdOPME`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `TZ_OPuri__Partener` FOREIGN KEY (`IdPartener`) REFERENCES `Parteneri` (`IdPartener`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- The allocation of a payment on an invoice. Access PlatiFacturi: IdPlata = the payment operation, IdFactura / IdFacturaAA = the evidence operation of the
-- current / previous year, Platit = the amount allocated (different from the total of the order), IDEFT = the received e-invoice.
CREATE TABLE IF NOT EXISTS `TZ_PlatiFacturi` (
  `IdAlocare` int(11) NOT NULL AUTO_INCREMENT,
  `IdPlata` int(11) NOT NULL COMMENT 'TZ_Oper: the payment operation',
  `IdOP` int(11) NULL DEFAULT NULL COMMENT 'TZ_OPuri: the order of this payment; NULL = a payment without order (migrated or a payment that has none)',
  `IdEvidentiere` int(11) NULL DEFAULT NULL COMMENT 'TZ_Oper: the evidence operation of the invoice (current year); NULL when the invoice is of the previous year',
  `IdEvidentiereAnterior` int(11) NULL DEFAULT NULL COMMENT 'Access Oper.IdOperatie of the evidence of the PREVIOUS year (Access IdFacturaAA): that year is not in this database, so no foreign key',
  `IdPrimita` int(11) NULL DEFAULT NULL COMMENT 'EF_Primite: the received e-invoice paid (Access IDEFT); NULL = an invoice typed by hand',
  `NumarDocument` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Number of the invoice (Access NrDoc)',
  `DataDocument` date NULL DEFAULT NULL COMMENT 'Date of the invoice (Access DtDoc)',
  `ValoareFactura` decimal(18, 2) NOT NULL DEFAULT 0.00 COMMENT 'Amount of the invoice at the time of the payment',
  `Platit` decimal(18, 2) NOT NULL DEFAULT 0.00 COMMENT 'Amount of this payment allocated on this invoice; negative on a credit note',
  `Diferenta` decimal(18, 2) NOT NULL DEFAULT 0.00 COMMENT 'ValoareFactura - Platit at the time of the payment (Access Diferenta)',
  `ValoarePlata` decimal(18, 2) NOT NULL DEFAULT 0.00 COMMENT 'Total of the payment operation (Access ValoarePlata)',
  `DataPlata` date NULL DEFAULT NULL,
  `NrAL` decimal(18, 0) NULL DEFAULT NULL COMMENT 'Access NrAL (kept as it is, meaning to confirm)',
  `Numerar` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Cash payment (Access Numerar)',
  `AlteDetalii` text CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL,
  `IdAlocareAccess` int(11) NULL DEFAULT NULL COMMENT 'Access PlatiFacturi.ID; NULL = made in K-BOT',
  `UN` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Who made it (e-mail); NULL = migrated from Access',
  `CreatedAt` datetime NULL DEFAULT current_timestamp() COMMENT 'When the row was created (migrated rows: the Access DTQ)',
  `UpdatedAt` datetime NULL DEFAULT NULL ON UPDATE current_timestamp() COMMENT 'When the row was last changed (NULL = not changed since it was created)',
  PRIMARY KEY (`IdAlocare`) USING BTREE,
  UNIQUE INDEX `UK_TZ_PlatiFacturi_Access`(`IdPlata` ASC, `IdAlocareAccess` ASC) USING BTREE,
  INDEX `IX_TZ_PlatiFacturi_OP`(`IdOP` ASC) USING BTREE,
  INDEX `IX_TZ_PlatiFacturi_Evidentiere`(`IdEvidentiere` ASC) USING BTREE,
  INDEX `IX_TZ_PlatiFacturi_Primita`(`IdPrimita` ASC) USING BTREE,
  CONSTRAINT `TZ_PlatiFacturi__Plata` FOREIGN KEY (`IdPlata`) REFERENCES `TZ_Oper` (`IdOper`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `TZ_PlatiFacturi__OP` FOREIGN KEY (`IdOP`) REFERENCES `TZ_OPuri` (`IdOP`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `TZ_PlatiFacturi__Evidentiere` FOREIGN KEY (`IdEvidentiere`) REFERENCES `TZ_Oper` (`IdOper`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `TZ_PlatiFacturi__Primita` FOREIGN KEY (`IdPrimita`) REFERENCES `EF_Primite` (`IdPrimita`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;
