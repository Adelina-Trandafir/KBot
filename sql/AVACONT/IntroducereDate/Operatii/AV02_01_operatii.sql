-- AV02-01: the accounting operations of Avacont (stage 2, data entry): notes, documents, operations, debit and credit lines.
-- Access: NoteC, Documente, Oper, Debit, Credit. Design and reasons: docs/worklog/SLICE-AV02-01-tabele-postare.md.
-- Run by the operator on AVACONT_SURSA only, then schema sync to every unit database (same procedure as AV01_04). NOT RUN anywhere yet.
-- Safe to run again (CREATE TABLE IF NOT EXISTS). utf8mb3 / utf8mb3_general_ci (house rule). Order matters (foreign keys): the five tables below, top to bottom.
--
-- Rules written into the keys:
--  * Nothing is deleted by cascade (accounting data): every foreign key is RESTRICT. The server deletes the lines, then the operations, then the document, then the note,
--    inside one transaction, and only for a user with the right «Stergere N.C.».
--  * debit total = credit total of an operation, an account of the document's subunit, amounts: checked by the SERVER on save (MariaDB cannot check them across rows).
--  * The Access ids are kept only in the columns ending in «Access» (they let the migration run again and add only what is missing, and let other tables point at
--    the migrated rows); a row made in K-BOT leaves them NULL. Ids of different Access files collide, so they are never primary keys.
--  * Every table has the pair CreatedAt (set when the row is made; the migration puts the Access DTQ there) and UpdatedAt (MariaDB sets it on every change).
-- Left out on purpose (see the design): everything of FOREXE in Access (Oper.AngajamentLegal / DataAngLeg), IDSS, IDC, IdBursa (always 0), Oper.OP and Documente.NRNIR (always empty),
-- Oper.Simpla (the program writes 0), Oper.IDEFT (replaced by EF_PrimiteOperatii), OPuri / PlatiFacturi / Actiuni (AV02-02).

-- The accounting note: the number a set of documents is booked under, per subunit and year. Access NoteC (NumarNC, DataNc = when the note was made).
CREATE TABLE IF NOT EXISTS `TZ_NoteC` (
  `IdNota` int(11) NOT NULL AUTO_INCREMENT,
  `IdSubunitate` int(11) NOT NULL COMMENT 'TZ_Subunitati: the note belongs to one subunit',
  `An` smallint(6) NOT NULL COMMENT 'Year of the note (the Access file was one per year)',
  `NumarNC` int(11) NOT NULL COMMENT 'Number of the note, given by the server: highest of the subunit and year + 1',
  `DataNc` datetime NOT NULL DEFAULT current_timestamp() COMMENT 'When the note was made (Access NoteC.DataNc)',
  `UN` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Who made it (e-mail); NULL = migrated from Access',
  `CreatedAt` datetime NULL DEFAULT current_timestamp() COMMENT 'When the row was created (migrated rows: the Access DTQ)',
  `UpdatedAt` datetime NULL DEFAULT NULL ON UPDATE current_timestamp() COMMENT 'When the row was last changed (NULL = not changed since it was created)',
  PRIMARY KEY (`IdNota`) USING BTREE,
  UNIQUE INDEX `UK_TZ_NoteC_Numar`(`IdSubunitate` ASC, `An` ASC, `NumarNC` ASC) USING BTREE,
  CONSTRAINT `TZ_NoteC__Subunitate` FOREIGN KEY (`IdSubunitate`) REFERENCES `TZ_Subunitati` (`IdSubunitate`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- The document (invoice, order of payment, statement, cheque, ...). Access Documente.
CREATE TABLE IF NOT EXISTS `TZ_Documente` (
  `IdDocument` int(11) NOT NULL AUTO_INCREMENT,
  `IdNota` int(11) NOT NULL COMMENT 'TZ_NoteC: every document is booked under a note',
  `NumarDocument` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL COMMENT 'Number written on the document',
  `DataDocument` date NOT NULL,
  `FelDocument` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '' COMMENT 'Kind of document as the operator names it: factura, OP, CEC, nota contabila, stat salarii ...',
  `Fel` varchar(5) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Phase: E evidence, P payment, A other, I closing, and the salary kinds ANG / PC / PN / VA (stage 3); NULL = typed by hand',
  `Auto` tinyint(1) NOT NULL DEFAULT 0 COMMENT '1 = made by a program flow (template, invoice, statement); 0 = typed by hand',
  `EditManual` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Access EditManual: the document may be edited by hand (meaning to confirm)',
  `Platit` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Access Documente.Platit: an invoice document that has its payment (meaning to confirm)',
  `IdDocumentAccess` int(11) NULL DEFAULT NULL COMMENT 'Access Documente.IdDocument; NULL = made in K-BOT',
  `IdSalariiAccess` int(11) NULL DEFAULT NULL COMMENT 'Access Documente.IDSalarii: kept to re-link the statement when salaries are ported (stage 3)',
  `UN` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Who made it (e-mail); NULL = migrated from Access',
  `CreatedAt` datetime NULL DEFAULT current_timestamp() COMMENT 'When the row was created (migrated rows: the Access DTQ)',
  `UpdatedAt` datetime NULL DEFAULT NULL ON UPDATE current_timestamp() COMMENT 'When the row was last changed (NULL = not changed since it was created)',
  PRIMARY KEY (`IdDocument`) USING BTREE,
  UNIQUE INDEX `UK_TZ_Documente_Access`(`IdNota` ASC, `IdDocumentAccess` ASC) USING BTREE,
  INDEX `IX_TZ_Documente_Data`(`DataDocument` ASC) USING BTREE,
  INDEX `IX_TZ_Documente_Numar`(`NumarDocument` ASC) USING BTREE,
  INDEX `IX_TZ_Documente_FelDocument`(`FelDocument` ASC) USING BTREE,
  CONSTRAINT `TZ_Documente__Nota` FOREIGN KEY (`IdNota`) REFERENCES `TZ_NoteC` (`IdNota`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- The operation: one booking of a document, with its journal, classification and the budget flags. Access Oper.
CREATE TABLE IF NOT EXISTS `TZ_Oper` (
  `IdOper` int(11) NOT NULL AUTO_INCREMENT,
  `IdDocument` int(11) NOT NULL,
  `IdJurnal` int(11) NOT NULL COMMENT 'TZ_Jurnale (Access CodJurnal)',
  `IdClsf` int(11) NULL DEFAULT NULL COMMENT 'Clasificatii.IDClsf; NULL = an operation without a classification',
  `IdSablon` int(11) NULL DEFAULT NULL COMMENT 'The template it was made from (Access IDA); NULL = by hand or migrated (template ids do not carry over)',
  `DataOperatie` date NOT NULL,
  `Explicatie` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL DEFAULT '',
  `Tip` varchar(5) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Access Oper.Tip: E, A, I, P, PC, V, 10 (salaries) ...; free text until the stages that write it are ported',
  `Fel` varchar(5) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Access Oper.Fel: ANG / PC / PN / VA on salary operations',
  `Compartiment` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Department (Access Comp)',
  `Cfpp` tinyint(1) NOT NULL DEFAULT 1 COMMENT 'Control Financiar Preventiv (Access CFPP; the program writes 1)',
  `Angajament` tinyint(1) NOT NULL DEFAULT 0,
  `Propunere` tinyint(1) NOT NULL DEFAULT 0,
  `Ordonantare` tinyint(1) NOT NULL DEFAULT 0,
  `Chelt` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Effective expense (decided 23.10.2026: it stays)',
  `NrAng` int(11) NULL DEFAULT NULL COMMENT 'Number of the commitment, per subunit and year, given by the server',
  `NrOpe` int(11) NULL DEFAULT NULL COMMENT 'Accounting number of the ordonantare (Access NrOPE): given by the server per subunit and year in the DATE order of the operations; independent of FX_ORD.NrORD (the two series are not the same)',
  `IdOrdTbl` int(11) NULL DEFAULT NULL COMMENT 'FX_ORD_TBL.IDORDTBLP: the line of the ordonantare made from this operation (the ORD of K-BOT); one row = one operation (checked by the server, the key is not unique on purpose); NULL = not ordonantat yet, or an Access operation that could not be matched safely',
  `Auto` tinyint(1) NOT NULL DEFAULT 0,
  `Platit` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Access Oper.Platit (true on the salary operations): meaning to confirm',
  `IdOperatieAccess` int(11) NULL DEFAULT NULL COMMENT 'Access Oper.IdOperatie; NULL = made in K-BOT. EF_PrimiteOperatii.IdOperatie of the migrated invoices points here',
  `IdxAccess` int(11) NULL DEFAULT NULL COMMENT 'Access Oper.IDX (the salary line): kept to re-link when salaries are ported (stage 3)',
  `UN` varchar(255) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL DEFAULT NULL COMMENT 'Who made it (e-mail); NULL = migrated from Access',
  `CreatedAt` datetime NULL DEFAULT current_timestamp() COMMENT 'When the row was created (migrated rows: the Access DTQ)',
  `UpdatedAt` datetime NULL DEFAULT NULL ON UPDATE current_timestamp() COMMENT 'When the row was last changed (NULL = not changed since it was created)',
  PRIMARY KEY (`IdOper`) USING BTREE,
  UNIQUE INDEX `UK_TZ_Oper_Access`(`IdDocument` ASC, `IdOperatieAccess` ASC) USING BTREE,
  INDEX `IX_TZ_Oper_Jurnal`(`IdJurnal` ASC) USING BTREE,
  INDEX `IX_TZ_Oper_Clsf`(`IdClsf` ASC) USING BTREE,
  INDEX `IX_TZ_Oper_Sablon`(`IdSablon` ASC) USING BTREE,
  INDEX `IX_TZ_Oper_Data`(`DataOperatie` ASC) USING BTREE,
  INDEX `IX_TZ_Oper_NrAng`(`NrAng` ASC) USING BTREE,
  INDEX `IX_TZ_Oper_NrOpe`(`NrOpe` ASC) USING BTREE,
  INDEX `IX_TZ_Oper_OrdTbl`(`IdOrdTbl` ASC) USING BTREE,
  INDEX `IX_TZ_Oper_OperatieAccess`(`IdOperatieAccess` ASC) USING BTREE,
  CONSTRAINT `TZ_Oper__Document` FOREIGN KEY (`IdDocument`) REFERENCES `TZ_Documente` (`IdDocument`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `TZ_Oper__Jurnal` FOREIGN KEY (`IdJurnal`) REFERENCES `TZ_Jurnale` (`IdJurnal`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `TZ_Oper__Clsf` FOREIGN KEY (`IdClsf`) REFERENCES `Clasificatii` (`IDClsf`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `TZ_Oper__Sablon` FOREIGN KEY (`IdSablon`) REFERENCES `TZ_SablonOperatii` (`IdSablon`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `TZ_Oper__OrdTbl` FOREIGN KEY (`IdOrdTbl`) REFERENCES `FX_ORD_TBL` (`IDORDTBLP`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- The debit lines of an operation. Access Debit (IdOperatie, SimbolCont, CodPartener, SumaDebit). The account is the row of the chart of accounts of the
-- document's subunit (checked by the server); the partner is optional.
CREATE TABLE IF NOT EXISTS `TZ_Debit` (
  `IdLinie` int(11) NOT NULL AUTO_INCREMENT,
  `IdOper` int(11) NOT NULL,
  `IdCont` int(11) NOT NULL COMMENT 'TZ_PlanCont.IdCont (the composed symbol of Access SimbolCont)',
  `IdPartener` int(11) NULL DEFAULT NULL COMMENT 'Parteneri.IdPartener (Access CodPartener); NULL = no partner',
  `Suma` decimal(18,2) NOT NULL DEFAULT 0.00,
  `CreatedAt` datetime NULL DEFAULT current_timestamp() COMMENT 'When the row was created (migrated rows: the Access DTQ)',
  `UpdatedAt` datetime NULL DEFAULT NULL ON UPDATE current_timestamp() COMMENT 'When the row was last changed (NULL = not changed since it was created)',
  PRIMARY KEY (`IdLinie`) USING BTREE,
  INDEX `IX_TZ_Debit_Oper`(`IdOper` ASC) USING BTREE,
  INDEX `IX_TZ_Debit_Cont`(`IdCont` ASC) USING BTREE,
  INDEX `IX_TZ_Debit_Partener`(`IdPartener` ASC) USING BTREE,
  CONSTRAINT `TZ_Debit__Oper` FOREIGN KEY (`IdOper`) REFERENCES `TZ_Oper` (`IdOper`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `TZ_Debit__Cont` FOREIGN KEY (`IdCont`) REFERENCES `TZ_PlanCont` (`IdCont`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `TZ_Debit__Partener` FOREIGN KEY (`IdPartener`) REFERENCES `Parteneri` (`IdPartener`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- The credit lines of an operation. Access Credit (IdOperatie, SimbolCont, CodPartener, SumaCredit). Same rules as TZ_Debit.
CREATE TABLE IF NOT EXISTS `TZ_Credit` (
  `IdLinie` int(11) NOT NULL AUTO_INCREMENT,
  `IdOper` int(11) NOT NULL,
  `IdCont` int(11) NOT NULL COMMENT 'TZ_PlanCont.IdCont (the composed symbol of Access SimbolCont)',
  `IdPartener` int(11) NULL DEFAULT NULL COMMENT 'Parteneri.IdPartener (Access CodPartener); NULL = no partner',
  `Suma` decimal(18,2) NOT NULL DEFAULT 0.00,
  `CreatedAt` datetime NULL DEFAULT current_timestamp() COMMENT 'When the row was created (migrated rows: the Access DTQ)',
  `UpdatedAt` datetime NULL DEFAULT NULL ON UPDATE current_timestamp() COMMENT 'When the row was last changed (NULL = not changed since it was created)',
  PRIMARY KEY (`IdLinie`) USING BTREE,
  INDEX `IX_TZ_Credit_Oper`(`IdOper` ASC) USING BTREE,
  INDEX `IX_TZ_Credit_Cont`(`IdCont` ASC) USING BTREE,
  INDEX `IX_TZ_Credit_Partener`(`IdPartener` ASC) USING BTREE,
  CONSTRAINT `TZ_Credit__Oper` FOREIGN KEY (`IdOper`) REFERENCES `TZ_Oper` (`IdOper`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `TZ_Credit__Cont` FOREIGN KEY (`IdCont`) REFERENCES `TZ_PlanCont` (`IdCont`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `TZ_Credit__Partener` FOREIGN KEY (`IdPartener`) REFERENCES `Parteneri` (`IdPartener`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;
