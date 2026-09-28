-- =====================================================================================
-- Slice 0088 -- «Nota contabila corectie CAB» (form F1135): the correction note K-BOT makes for
-- the «ERRRRRRRRRR» operations FOREXE lists under «Operatiuni necorectate».
--
--   FX_Operatiuni         + CodAngajament: the «Angajament» column of the FOREXE table
--                         («ERRRRRRRRRR»), so the uncorrelated operations can be listed again
--                         later (menu «Operatiuni necorelate») without reading FOREXE.
--   FX_NoteCAB            one note (header: number, date, entity, signature, upload into CAB).
--   FX_NoteCAB_Corectii   one row per ERR operation corrected by the note: the storno (row 1 of
--                         the pair) and the angajament / indicator it goes to (row 2). A note holds
--                         EVERY operation the operator correlated in one save.
--   FX_NoteCAB_PDF        the note's PDF -- same shape as FX_ORD_PDF, served by
--                         routes/forexe/pdf.py (family «nc»).
--   FX_PDF_SEMNATURI      + IDNC, so the signature log (slice 0079) covers the notes too.
--
-- APPLY ON EVERY UNIT DATABASE (and on AVACONT_SURSA, the template for new units), BEFORE the
-- new operatiuni.py / note_cab.py / pdf.py reach the VPS.
--
-- Shapes checked against MariaDB_Schema/AVACONT_SURSA.sql (dump refreshed 28.09.2026).
-- =====================================================================================

-- The «Angajament» FOREXE showed for the operation. NULL on rows saved before this slice.
ALTER TABLE `FX_Operatiuni`
  ADD COLUMN IF NOT EXISTS `CodAngajament` varchar(11) NULL DEFAULT NULL AFTER `Program`;
-- Slice 0084 left Suma as int(11): «-368,50» would lose its bani.
ALTER TABLE `FX_Operatiuni` MODIFY COLUMN `Suma` decimal(15,2) NULL DEFAULT NULL;
-- The key the save uses (ReferintaTrezor + NrDoc): an operation is stored once.
ALTER TABLE `FX_Operatiuni`
  ADD INDEX IF NOT EXISTS `IX_FX_Operatiuni_Referinta` (`ReferintaTrezor` ASC, `NrDoc` ASC);

CREATE TABLE IF NOT EXISTS `FX_NoteCAB` (
  `IDNC`             int(11)       NOT NULL AUTO_INCREMENT,
  -- Printed as 10 digits («0000000023»). Unique per year (An = YEAR(DataNota)).
  `NrNota`           int(11)       NOT NULL,
  `An`               int(4)        NOT NULL,
  `DataNota`         date          NOT NULL,
  `DenumireEP`       varchar(30)   NOT NULL,
  `CifEP`            varchar(10)   NOT NULL,
  -- Signer roles in the stored PDF (S1, S2), written by routes/forexe/pdf.py; '' = unsigned.
  `Semnatura`        varchar(255)  NOT NULL DEFAULT '',
  -- The upload into FOREXE («Transmitere documente electronice»).
  `Trimis`           tinyint(1)    NOT NULL DEFAULT 0,
  `DataTrimitere`    datetime      NULL DEFAULT NULL,
  `RaspunsTrimitere` varchar(2000) NULL DEFAULT NULL,
  `UN`               varchar(128)  NULL DEFAULT NULL,
  `DataAdaugare`     datetime      NULL DEFAULT current_timestamp(),
  `DataModificare`   datetime      NULL DEFAULT NULL ON UPDATE current_timestamp(),
  PRIMARY KEY (`IDNC`) USING BTREE,
  UNIQUE INDEX `UQ_FX_NoteCAB_An_NrNota` (`An` ASC, `NrNota` ASC) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

CREATE TABLE IF NOT EXISTS `FX_NoteCAB_Corectii` (
  `IDNCC`              int(11)       NOT NULL AUTO_INCREMENT,
  `IDNC`               int(11)       NOT NULL,
  -- Order inside the note: pair k prints as rows 2k-1 (storno) and 2k (correction).
  `NrOrdine`           int(11)       NOT NULL,
  -- The ERR operation (FX_Operatiuni). One correction per operation: UQ on the key below.
  `IDFXP`              int(11)       NULL DEFAULT NULL,
  `ReferintaTrezor`    varchar(15)   NOT NULL,
  `NrDoc`              varchar(255)  NOT NULL,
  -- Storno row: the operation as FOREXE shows it.
  `SimbolCont`         varchar(15)   NOT NULL,
  `CodProgram`         varchar(10)   NOT NULL,
  `DataOperInitiala`   date          NOT NULL,
  -- 'D' = «Suma debit», 'C' = «Suma credit» (an «Incasare» is 'C').
  `Coloana`            char(1)       NOT NULL,
  -- The storno amount: always negative. The correction row carries -Suma.
  `Suma`               decimal(15,2) NOT NULL,
  -- Correction row: the angajament / indicator the operator chose.
  `CodAngajament`      varchar(255)  NOT NULL,
  `CodIndicator`       varchar(3)    NOT NULL,
  `CodAI`              varchar(50)   NULL DEFAULT NULL,
  `SimbolContCorectie` varchar(15)   NOT NULL,
  `CodProgramCorectie` varchar(10)   NOT NULL,
  `Explicatii`         varchar(70)   NOT NULL,
  PRIMARY KEY (`IDNCC`) USING BTREE,
  UNIQUE INDEX `UQ_FX_NoteCAB_Corectii_Operatiune` (`ReferintaTrezor` ASC, `NrDoc` ASC) USING BTREE,
  INDEX `IX_FX_NoteCAB_Corectii_IDNC` (`IDNC` ASC, `NrOrdine` ASC) USING BTREE,
  INDEX `IX_FX_NoteCAB_Corectii_CodAngajament` (`CodAngajament` ASC) USING BTREE,
  INDEX `IX_FX_NoteCAB_Corectii_IDFXP` (`IDFXP` ASC) USING BTREE,
  CONSTRAINT `FK_FX_NoteCAB_Corectii_Nota` FOREIGN KEY (`IDNC`)
    REFERENCES `FX_NoteCAB` (`IDNC`) ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT `FK_FX_NoteCAB_Corectii_Angajament` FOREIGN KEY (`CodAngajament`)
    REFERENCES `FX_Angajamente` (`CodAngajament`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `FK_FX_NoteCAB_Corectii_Operatiune` FOREIGN KEY (`IDFXP`)
    REFERENCES `FX_Operatiuni` (`IDFXP`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `CK_FX_NoteCAB_Corectii_Coloana` CHECK (`Coloana` IN ('D', 'C')),
  CONSTRAINT `CK_FX_NoteCAB_Corectii_Suma` CHECK (`Suma` < 0)
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

CREATE TABLE IF NOT EXISTS `FX_NoteCAB_PDF` (
  `IDPDF`      int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `IDNC`       int(11)          NOT NULL,
  `NumeFisier` varchar(255)     NOT NULL,
  `Dimensiune` int(10) UNSIGNED NOT NULL,
  `Sha256`     char(64)         NOT NULL,
  `Continut`   longblob         NULL DEFAULT NULL,
  `Bucati`     mediumblob       NULL DEFAULT NULL,
  `DataModif`  datetime         NOT NULL,
  PRIMARY KEY (`IDPDF`) USING BTREE,
  UNIQUE INDEX `UQ_FX_NoteCAB_PDF_IDNC` (`IDNC` ASC) USING BTREE,
  CONSTRAINT `FK_FX_NoteCAB_PDF_NC` FOREIGN KEY (`IDNC`)
    REFERENCES `FX_NoteCAB` (`IDNC`) ON DELETE CASCADE ON UPDATE RESTRICT,
  CONSTRAINT `CK_FX_NoteCAB_PDF_FORMA` CHECK (`Continut` IS NULL <> (`Bucati` IS NULL))
) ENGINE = InnoDB CHARACTER SET = utf8mb3 COLLATE = utf8mb3_general_ci ROW_FORMAT = Dynamic;

-- The signature log: a third document family. The check becomes «exactly one of three».
ALTER TABLE `FX_PDF_SEMNATURI`
  ADD COLUMN IF NOT EXISTS `IDNC` int(11) NULL DEFAULT NULL AFTER `IDORDP`,
  ADD INDEX IF NOT EXISTS `IX_FX_PDF_SEMNATURI_IDNC` (`IDNC` ASC, `Camp` ASC);
ALTER TABLE `FX_PDF_SEMNATURI` DROP CONSTRAINT IF EXISTS `CK_FX_PDF_SEMNATURI_DOC`;
ALTER TABLE `FX_PDF_SEMNATURI`
  ADD CONSTRAINT `CK_FX_PDF_SEMNATURI_DOC`
    CHECK ((`IDREV` IS NOT NULL) + (`IDORDP` IS NOT NULL) + (`IDNC` IS NOT NULL) = 1),
  ADD CONSTRAINT `FK_FX_PDF_SEMNATURI_NC` FOREIGN KEY (`IDNC`)
    REFERENCES `FX_NoteCAB` (`IDNC`) ON DELETE CASCADE ON UPDATE RESTRICT;
